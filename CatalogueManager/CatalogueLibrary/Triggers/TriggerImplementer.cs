using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Triggers.Exceptions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Exceptions;

namespace CatalogueLibrary.Triggers
{
    /// <summary>
    /// Creates an _Archive table to match a live table and a Database Trigger On Update which moves old versions of records to the _Archive table when the main table
    /// is UPDATEd.  An _Archive table is an exact match of columns as the live table (which must have primary keys) but also includes several audit fields (date it 
    /// was archived etc).  The _Archive table can be used to view the changes that occured during data loading (See DiffDatabaseDataFetcher) and/or generate a 
    /// 'way back machine' view of the data at a given date in the past (See CreateViewOldVersionsTableValuedFunction method).
    /// 
    /// This class is super Microsoft Sql Server specific.  It is not suitable to create backup triggers on tables in which you expect high volitility (lots of frequent
    /// updates all the time).
    /// 
    /// Also contains methods for confirming that a trigger exists on a given table, that the primary keys still match when it was created and the _Archive table hasn't
    /// got a different schema to the live table (e.g. if you made a change to the live table this would pick up that the _Archive wasn't updated).
    /// </summary>
    public class TriggerImplementer
    {
        private readonly DiscoveredDatabase _dbInfo;
        private readonly string _table;
        private readonly bool _createDataLoadRunIdAlso;

        SqlConnection con;

        public TriggerImplementer(DiscoveredDatabase dbInfo,string table, bool createDataLoadRunIDAlso = true)
        {
            _dbInfo = dbInfo;
            _table = table;
            _createDataLoadRunIdAlso = createDataLoadRunIDAlso;
            con = (SqlConnection) dbInfo.Server.GetConnection();
        }

        public void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
        {
            con.Open();
            try
            {
                string triggerName = "[dbo]." + _table + "_OnUpdate";

                problemsDroppingTrigger = "";
                thingsThatWorkedDroppingTrigger = "";

                SqlCommand cmdDropTrigger = new SqlCommand("DROP TRIGGER " + triggerName, con);
                try
                {
                    thingsThatWorkedDroppingTrigger += "Dropped Trigger successfully" + Environment.NewLine;
                    cmdDropTrigger.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
                    problemsDroppingTrigger += "Failed to drop Trigger:" + exception.Message + Environment.NewLine; ;
                }

                SqlCommand cmdDropArchiveIndex = new SqlCommand("DROP INDEX PKsIndex ON " + _table + "_Archive", con);
                try
                {
                    cmdDropArchiveIndex.ExecuteNonQuery();

                    thingsThatWorkedDroppingTrigger += "Dropped index PKsIndex on Archive table successfully" + Environment.NewLine;
                }
                catch (Exception exception)
                {
                    problemsDroppingTrigger += "Failed to drop Archive Index:" + exception.Message + Environment.NewLine;
                }

                SqlCommand cmdDropArchiveLegacyView = new SqlCommand("DROP FUNCTION " + _table + "_Legacy", con);
                try
                {
                   cmdDropArchiveLegacyView.ExecuteNonQuery();
                   thingsThatWorkedDroppingTrigger += "Dropped Legacy Table View successfully" + Environment.NewLine;
                }
                catch (Exception exception)
                {
                    problemsDroppingTrigger += "Failed to drop Legacy Table View:" + exception.Message + Environment.NewLine;
                }

            }
            finally
            {
                con.Close();
                
            }
        }

        public void CreateTrigger(string[] primaryKeys, ICheckNotifier notifier, int createArchiveIndexTimeout= 30)
        {

                //if _Archive exists skip creating it
                bool skipCreatingArchive = _dbInfo.ExpectTable(_table + "_Archive").Exists();
            
                var columns = _dbInfo.ExpectTable(_table).DiscoverColumns();
            
                //check _Archive does not already exist
                foreach (string forbiddenColumnName in new[] { "hic_validTo", "hic_userID", "hic_status" })
                    if (columns.Any(c=>c.GetRuntimeName().Equals(forbiddenColumnName)))
                        throw new Exception("Table " + _table + " already contains a column called " + forbiddenColumnName + " this column is reserved for Archiving");

                bool b_mustCreate_validFrom = !columns.Any(c=>c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom));
                bool b_mustCreate_dataloadRunId = !columns.Any(c=>c.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID)) && _createDataLoadRunIdAlso;

                //forces column order dataloadrunID then valid from (doesnt prevent these being in the wrong place in the record but hey ho - possibly not an issue anyway since probably the 3 values in the archive are what matters for order - see the Trigger which populates *,X,Y,Z where * is all columns in mane table
                if(b_mustCreate_dataloadRunId && !b_mustCreate_validFrom)
                    throw new Exception("Cannot create trigger because table contains "+SpecialFieldNames.ValidFrom+" but not " + SpecialFieldNames.DataLoadRunID + " (ID must be placed before valid from in column order)");
                
                SqlTransaction transaction = null;

                if (string.IsNullOrWhiteSpace(_table))
                    return;
                try
                {
                    con.Open();

                    //must add validFrom outside of transaction if we want SMO to pick it up
                    if (b_mustCreate_dataloadRunId)
                    {
                        SqlCommand cmdAlterToAddDataLoadRunID = new SqlCommand("ALTER TABLE " + _table + " ADD " + SpecialFieldNames.DataLoadRunID + " int null", con, transaction);
                        cmdAlterToAddDataLoadRunID.ExecuteNonQuery();
                    }

                    //must add validFrom outside of transaction if we want SMO to pick it up
                    if (b_mustCreate_validFrom)
                    {
                        SqlCommand cmdAlterToAddValidTo = new SqlCommand("ALTER TABLE " + _table + " ADD "+SpecialFieldNames.ValidFrom+" datetime DEFAULT getdate() ", con, transaction);
                        cmdAlterToAddValidTo.ExecuteNonQuery();
                    }

                    
                    string archiveTableName = _table + "_Archive";
                    string createArchiveTableSQL = WorkOutArchiveTableCreationSQLUsingSMO(archiveTableName);


                    transaction = con.BeginTransaction();

                    if (!skipCreatingArchive)
                    {

                       //select top 0 into _Archive
                        SqlCommand cmdCreateArchive = new SqlCommand(createArchiveTableSQL, con, transaction);

                        cmdCreateArchive.ExecuteNonQuery();

                        //alter to add columns
                        SqlCommand cmdAlterToAddRequiredColumns = new SqlCommand(
                            "ALTER TABLE " + archiveTableName + " ADD hic_validTo datetime;" +
                            "ALTER TABLE " + archiveTableName + " ADD hic_userID varchar(128);" +
                            "ALTER TABLE " + archiveTableName + " ADD hic_status char(1);", con, transaction);

                        cmdAlterToAddRequiredColumns.ExecuteNonQuery();
                    }

                    string trigger = GetCreateTriggerSQL(primaryKeys,columns);
                    SqlCommand cmdAddTrigger = new SqlCommand(trigger, con, transaction);
                    cmdAddTrigger.ExecuteNonQuery();
                    
                    //Add key so that we can more easily do comparisons on primary key between main table and archive
                    string idxCompositeKeyBody = "";

                    foreach (string key in primaryKeys)
                        idxCompositeKeyBody += "[" + key + @"] ASC,";

                    //remove trailing comma
                    idxCompositeKeyBody = idxCompositeKeyBody.TrimEnd(',');

                    string createIndexSQL = @"CREATE NONCLUSTERED INDEX [PKsIndex] ON [dbo].[" + archiveTableName + "](" +idxCompositeKeyBody + ")";
                    SqlCommand cmdCreateIndex = new SqlCommand(createIndexSQL, con, transaction);

                    try
                    {
                        cmdCreateIndex.CommandTimeout = createArchiveIndexTimeout;
                        cmdCreateIndex.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        var tryAgain = notifier.OnCheckPerformed(new CheckEventArgs(
                            "Could not create index on archive table because of timeout, possibly your _Archive table has a lot of data in it",
                            CheckResult.Fail, e, "Create archive index with a timeout of 10000000 seconds"));

                        if(!tryAgain)
                            throw new Exception("User abandoned process by rejecting the proposed fix");
                        else
                        {
                            con.Close();
                            
                            notifier.OnCheckPerformed(new CheckEventArgs(
                                "Abandoned current attempt to make a backup trigger and started a new one with a longer timeout",
                                CheckResult.Success, null));

                            CreateTrigger(primaryKeys, notifier, 10000000);//recursively do the entire process again but with a longer timeout
                            return;//recursive call worked
                        }
                    }

                    CreateViewOldVersionsTableValuedFunction(primaryKeys,columns, _table, archiveTableName, createArchiveTableSQL, con, transaction);

                    transaction.Commit();

                }
                finally
                {

                    con.Close();
                }
       
        }

        private string GetCreateTriggerSQL(string[] primaryKeys, DiscoveredColumn[] columns)
        {
            string archiveTableName = _table + "_Archive";

            string triggerName = "[dbo]." + _table + "_OnUpdate";

            if (!primaryKeys.Any())
                throw new Exception("There must be at least 1 primary key");

            //this is the SQL to join on the main table to the deleted to record the hic_validFrom
            string updateValidToWhere = " UPDATE " + _table +
                                        " SET "+SpecialFieldNames.ValidFrom+" = GETDATE() FROM deleted where ";

            //its a combo field so join on both when filling in hic_validFrom
            foreach (string key in primaryKeys)
                updateValidToWhere += "[" + _table + "].[" + key + "] = deleted.[" + key + "] AND ";

            //trim off last AND
            updateValidToWhere = updateValidToWhere.Substring(0, updateValidToWhere.Length - "AND ".Length);

            string InsertedToDeletedJoin = "JOIN inserted i ON ";
            InsertedToDeletedJoin += GetTableToTableEqualsSqlWithPrimaryKeys("i", "d", primaryKeys);

            string equalsSqlTableToInserted = GetTableToTableEqualsSqlWithPrimaryKeys(_table,"inserted",primaryKeys);
            string equalsSqlTableToDeleted = GetTableToTableEqualsSqlWithPrimaryKeys(_table,"deleted",primaryKeys);


            string colList = string.Join(",", columns.Select(c => c.GetRuntimeName()).Union(new String[] { SpecialFieldNames.DataLoadRunID ,SpecialFieldNames.ValidFrom}));
            string dDotColList = string.Join(",", columns.Select(c => "d." + c.GetRuntimeName()).Union(new String[] { "d."+SpecialFieldNames.DataLoadRunID, "d."+SpecialFieldNames.ValidFrom }));

            return
            @"
CREATE TRIGGER " + triggerName + " ON [dbo].[" + _table + @"]
AFTER UPDATE, DELETE
AS BEGIN

declare @isPrimaryKeyChange bit = 0

--it will be a primary key change if deleted and inserted do not agree on primary key values
IF exists ( select 1 FROM deleted d RIGHT " + InsertedToDeletedJoin + @" WHERE d.[" + primaryKeys.First() + @"] is null)
begin
	UPDATE " + _table + @" SET " + SpecialFieldNames.ValidFrom + " = GETDATE() FROM inserted where " +
            equalsSqlTableToInserted + @"
	set @isPrimaryKeyChange = 1
end
else
begin
	UPDATE " + _table + @" SET " + SpecialFieldNames.ValidFrom + " = GETDATE() FROM deleted where " +
            equalsSqlTableToDeleted + @"
	set @isPrimaryKeyChange = 0
end

SET NOCOUNT ON

" + updateValidToWhere + @"

INSERT INTO " + archiveTableName + @" (" + colList + @",hic_validTo,hic_userID,hic_status) SELECT " + dDotColList +
            ", GETDATE(), SYSTEM_USER, CASE WHEN @isPrimaryKeyChange = 1 then 'K' WHEN i.[" + primaryKeys.First() +
            "] IS NULL THEN 'D' WHEN d.[" + primaryKeys.First() + @"] IS NULL THEN 'I' ELSE 'U' END
FROM deleted d 
LEFT " + InsertedToDeletedJoin + @"

SET NOCOUNT OFF

RETURN
END  
";
        }

        private string GetTableToTableEqualsSqlWithPrimaryKeys(string table1, string table2, string[] primaryKeys)
        {
            string toReturn = "";

            foreach (string key in primaryKeys)
                toReturn += " " + table1 + ".[" + key + "] = " + table2 + ".[" + key + "] AND ";

            //trim off last AND
            toReturn = toReturn.Substring(0, toReturn.Length - "AND ".Length);
            
            return toReturn;
        }

        private void CreateViewOldVersionsTableValuedFunction(string[] primaryKeys, DiscoveredColumn[] columns, string mainTable, string archiveTable, string sqlUsedToCreateArchiveTableSQL, SqlConnection con, SqlTransaction transaction)
        {
            string columnsInArchive = "";

            Match matchStartColumnExtraction = Regex.Match(sqlUsedToCreateArchiveTableSQL, "CREATE TABLE \\[.*\\(");

            if (!matchStartColumnExtraction.Success)
                throw new Exception("Could not find regex match at start of Archive table CREATE SQL");

            int startExtractingColumnsAt = matchStartColumnExtraction.Index + matchStartColumnExtraction.Length;
            //trim off excess crud at start and we should have just the columns bit of the create (plus crud at the end)
            columnsInArchive = sqlUsedToCreateArchiveTableSQL.Substring(startExtractingColumnsAt);

            //trim off excess crud at the end
            if (!columnsInArchive.Contains(") ON "))
                throw new Exception("Could not find match at end of Archive table CREATE SQL");

            columnsInArchive = columnsInArchive.Substring(0, columnsInArchive.IndexOf(") ON "));


            string sqlToRun = string.Format("CREATE FUNCTION [dbo].[{0}_Legacy]", mainTable);
            sqlToRun += Environment.NewLine;
            sqlToRun += "(" + Environment.NewLine;
            sqlToRun += "\t@index DATETIME" + Environment.NewLine;
            sqlToRun += ")" + Environment.NewLine;
            sqlToRun += "RETURNS @returntable TABLE" + Environment.NewLine;
            sqlToRun += "(" + Environment.NewLine;
            sqlToRun += "/*the return table will follow the structure of the Archive table*/" + Environment.NewLine;
            sqlToRun += columnsInArchive;

            //these were added during transaction so we have to specify them again here because transaction will not have been committed yet
            sqlToRun = sqlToRun.Trim();
            sqlToRun += "," + Environment.NewLine;
            sqlToRun += "\thic_validTo datetime," + Environment.NewLine;
            sqlToRun += "\thic_userID varchar(128),";
            sqlToRun += "\thic_status char(1)";


            sqlToRun += ")" + Environment.NewLine;
            sqlToRun += "AS" + Environment.NewLine;
            sqlToRun += "BEGIN" + Environment.NewLine;
            sqlToRun += Environment.NewLine;

            var liveCols = columns.Select(c => c.GetRuntimeName()).Union(new String[] {SpecialFieldNames.DataLoadRunID, SpecialFieldNames.ValidFrom}).ToArray();

            string archiveCols = string.Join(",", liveCols) + ",hic_validTo,hic_userID,hic_status";
            string cDotArchiveCols = string.Join(",", liveCols.Select(s => "c." + s)); 


            sqlToRun += "\tINSERT @returntable" + Environment.NewLine;
            sqlToRun += string.Format("\tSELECT "+archiveCols+" FROM {0} WHERE @index BETWEEN ISNULL(" + SpecialFieldNames.ValidFrom + ", '1899/01/01') AND hic_validTo" + Environment.NewLine, archiveTable);
            sqlToRun += Environment.NewLine;

            sqlToRun += "\tINSERT @returntable" + Environment.NewLine; ;
            sqlToRun += "\tSELECT " + cDotArchiveCols + ",NULL AS hic_validTo, NULL AS hic_userID, 'C' AS hic_status" + Environment.NewLine; //c is for current
            sqlToRun += string.Format("\tFROM {0} c" + Environment.NewLine, mainTable);
            sqlToRun += "\tLEFT OUTER JOIN @returntable a ON " + Environment.NewLine;

            for (int index = 0; index < primaryKeys.Length; index++)
            {
                sqlToRun += string.Format("\ta.{0}=c.{0} " + Environment.NewLine, 
                    RDMPQuerySyntaxHelper.EnsureValueIsWrapped(primaryKeys[index])); //add the primary key joins

                if (index + 1 < primaryKeys.Length)
                    sqlToRun += "\tAND" + Environment.NewLine; //add an AND because there are more coming
            }

            sqlToRun += string.Format("\tWHERE a.{0} IS NULL -- where archive record doesn't exist" + Environment.NewLine, primaryKeys.First());
            sqlToRun += "\tAND @index > ISNULL(c." + SpecialFieldNames.ValidFrom + ", '1899/01/01')" + Environment.NewLine;

            sqlToRun += Environment.NewLine;
            sqlToRun += "RETURN" + Environment.NewLine;
            sqlToRun += "END" + Environment.NewLine;

            SqlCommand cmd = new SqlCommand(sqlToRun, con);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();



            /*
             
CREATE FUNCTION [dbo].[practice_contacts_sdcrn_Legacy]
(
    @index DATETIME
)
RETURNS @returntable TABLE 
(
--the return table will follow the structure of the Archive table.
	[hba] [nvarchar](255) NULL,
	[organisation] [nvarchar](255) NULL,
	[practice_code] [nvarchar](255) NOT NULL,
	[address1] [nvarchar](255) NULL,
	[address2] [nvarchar](255) NULL,
	[address3] [nvarchar](255) NULL,
	[address4] [nvarchar](255) NULL,
	[postcode] [nvarchar](255) NULL,
	[practice_manager] [nvarchar](255) NULL,
	[practice_manager_email] [nvarchar](255) NULL,
	[hic_validFrom] [datetime] NULL,
	[hic_validTo] [datetime] NULL,
	[hic_userID] [varchar](128) NULL,
	[hic_status] [char](1) NULL
)
AS
BEGIN
		
		--TESTING
		--DECLARE @index DATETIME
		--SET @index = '2014/10/28'

	--select all records from the Archive table.
	INSERT @returntable
	SELECT * FROM Demography..practice_contacts_sdcrn_Archive WHERE @index BETWEEN ISNULL(hic_validFrom, '1899/01/01') AND hic_validTo

	--add all records available at index from current table where a record has not already been selected from the archive.
	INSERT @returntable
	SELECT c.*,NULL AS hic_validTo, NULL AS hic_userID, 'C' AS hic_status 
	FROM Demography..practice_contacts_sdcrn c
	LEFT OUTER JOIN @returntable a ON a.practice_code = c.practice_code  --join archive and current table on primary key, ie new only.
	WHERE a.practice_code IS NULL						--where archive record doesn't exist
	AND @index > ISNULL(c.hic_validFrom, '1899/01/01')	--and record is applicable to the index time

    RETURN 
END             * 
             */


        }

        private string WorkOutArchiveTableCreationSQLUsingSMO(string archiveTableName)
        {

            //script original table
            string createTableSQL = ScriptTableCreation();

            string toReplaceTableName = Regex.Escape("CREATE TABLE [dbo].[" + _table + "]");

            if (Regex.Matches(createTableSQL, toReplaceTableName).Count != 1)
                throw new Exception("Expected to find 1 occurrence of " + toReplaceTableName + " in the SQL " + createTableSQL);

            //rename table
            createTableSQL = Regex.Replace(createTableSQL, toReplaceTableName, "CREATE TABLE [dbo].[" + archiveTableName + "]");

            string toRemoveIdentities = "IDENTITY\\(\\d+,\\d+\\)";

            //drop identity bit
            createTableSQL = Regex.Replace(createTableSQL, toRemoveIdentities, "");

            return createTableSQL;
        }
        private string ScriptTableCreation()
        {
            StringBuilder resultBuilder = new StringBuilder();

            Server smoServer = new Server(new ServerConnection((SqlConnection) _dbInfo.Server.GetConnection()));
            var smoDatabase = new Microsoft.SqlServer.Management.Smo.Database(smoServer, _dbInfo.GetRuntimeName());

            Scripter scripter = new Scripter(smoServer);
            scripter.Options.ScriptDrops = false;
            scripter.Options.WithDependencies = false;
            scripter.Options.IncludeIfNotExists = false;
            scripter.Options.Indexes = false;
            scripter.Options.DriAllKeys = false;
            scripter.Options.DriAll = false;
            scripter.Options.DriForeignKeys = false;
            scripter.Options.DriPrimaryKey = false;
            scripter.Options.DriUniqueKeys = false;
            scripter.Options.DriDefaults = false;
            scripter.Options.Default = true;
            scripter.Options.Bindings = true;
            scripter.Options.DriAllConstraints = false;

            smoDatabase.Refresh();

            Urn[] urn = new Urn[1];
            foreach (Table t in smoDatabase.Tables)
            {

                if (t.Name == _table)
                {
                    urn[0] = t.Urn;
                    if (t.IsSystemObject == false)
                    {
                        foreach (string s in scripter.Script(urn))
                            resultBuilder.Append(s + Environment.NewLine);
                    }
                }
            }

            return resultBuilder.ToString();
        }


        public TriggerStatus CheckUpdateTriggerIsEnabledOnServer()
        {
            var updateTriggerName = _table + "_OnUpdate";
            var queryTriggerIsItDisabledOrMissing = "USE [" + _dbInfo.GetRuntimeName()+ @"]; 
if exists (select 1 from sys.triggers WHERE name=@triggerName) SELECT is_disabled  FROM sys.triggers WHERE name=@triggerName else select -1 is_disabled";
            
            try
            {

                using (var conn = (SqlConnection)_dbInfo.Server.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(queryTriggerIsItDisabledOrMissing, conn);
                    cmd.Parameters.Add(new SqlParameter("@triggerName",SqlDbType.VarChar));
                    cmd.Parameters["@triggerName"].Value = updateTriggerName;

                    var result = Convert.ToInt32(cmd.ExecuteScalar());

                    switch (result)
                    {
                        case 0: 
                            return TriggerStatus.Enabled;
                        case 1:
                            return TriggerStatus.Disabled;
                        case -1: 
                            return TriggerStatus.Missing;
                        default:
                            throw new NotSupportedException("Query returned unexpected value:" + result);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to check if trigger " + updateTriggerName + " is enabled: " + e);
            }
        }

        public bool CheckUpdateTriggerIsEnabled_Advanced( IEnumerable<string> expectedPrimaryKeys)
        {
            
            //check server has trigger and it is on 
            TriggerStatus isEnabledSimple = CheckUpdateTriggerIsEnabledOnServer();

            if (isEnabledSimple == TriggerStatus.Disabled || isEnabledSimple == TriggerStatus.Missing)
                return false;

            //now check the definition of it! - make sure it relates to primary keys etc
            var updateTriggerName = _table + "_OnUpdate";
            var query = "USE [" + _dbInfo.GetRuntimeName()+ "];SELECT OBJECT_DEFINITION (object_id) FROM sys.triggers WHERE name='" +
                        updateTriggerName + "' and is_disabled=0";

         

            try
            {
                con.Open();
                var cmd = new SqlCommand(query, con);
                var result = cmd.ExecuteScalar() as string;

                if (String.IsNullOrWhiteSpace(result))
                    throw new MissingObjectException("Trigger " + updateTriggerName +
                                                     " does not have an OBJECT_DEFINITION or is missing or is disabled");

                string expectedSQL = GetCreateTriggerSQL(expectedPrimaryKeys.ToArray(), _dbInfo.ExpectTable(_table).DiscoverColumns());

                expectedSQL = expectedSQL.Trim();
                result = result.Trim();

                if(!expectedSQL.Equals(result))
                    throw new ExpectedIdenticalStringsException("Trigger " + updateTriggerName + " is corrupt",expectedSQL,result);

                CheckColumnDefinitionsMatchArchive();

            }
            catch(IrreconcilableColumnDifferencesInArchiveException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Failed to check if trigger " + updateTriggerName + " is enabled.  See InnerException for details",
                    e);
            }
            finally
            {
                con.Close();
            }

            return true;
        }

        private void CheckColumnDefinitionsMatchArchive()
        {
            List<string> errors = new List<string>();

            string archiveTableName = _table + "_Archive";

            var mainTableCols = _dbInfo.ExpectTable(_table).DiscoverColumns().ToArray();
            var archiveTableCols = _dbInfo.ExpectTable(archiveTableName).DiscoverColumns().ToArray();

            foreach (DiscoveredColumn col in mainTableCols)
            {
                var colInArchive = archiveTableCols.SingleOrDefault(c => c.GetRuntimeName().Equals(col.GetRuntimeName()));
                
                if(colInArchive == null)
                    errors.Add("Column " + col.GetRuntimeName() + " appears in Table '" + _table + "' but not in archive table '" + archiveTableName +"'");
                else
                    if(!AreCompatibleDatatypes(col.DataType,colInArchive.DataType))
                        errors.Add("Column " + col.GetRuntimeName() + " has data type '" + col.DataType + "' in '" + _table + "' but in Archive table '" + archiveTableName +"' it is defined as '" + colInArchive.DataType  +"'");
            }

            if(errors.Any())
                throw new IrreconcilableColumnDifferencesInArchiveException("The following column mismatch errors were seen:" +Environment.NewLine  + string.Join(Environment.NewLine, errors));
        }

        private bool AreCompatibleDatatypes(DiscoveredDataType mainDataType, DiscoveredDataType archiveDataType)
        {
            var t1 = mainDataType.SQLType;
            var t2 = archiveDataType.SQLType;

            if (t1.Equals(t2,StringComparison.CurrentCultureIgnoreCase))
                return true;

            if (t1.ToLower().Contains("identity"))
                return t1.ToLower().Replace("identity", "").Trim().Equals(t2.ToLower().Trim());

            return false;
        }

        public enum TriggerStatus
        {
            Enabled,
            Disabled,
            Missing
        }
    }
}
