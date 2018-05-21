using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Triggers.Exceptions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Exceptions;

namespace CatalogueLibrary.Triggers.Implementations
{
    /// <summary>
    /// Creates an _Archive table to match a live table and a Database Trigger On Update which moves old versions of records to the _Archive table when the main table
    /// is UPDATEd.  An _Archive table is an exact match of columns as the live table (which must have primary keys) but also includes several audit fields (date it 
    /// was archived etc).  The _Archive table can be used to view the changes that occured during data loading (See DiffDatabaseDataFetcher) and/or generate a 
    /// 'way back machine' view of the data at a given date in the past (See CreateViewOldVersionsTableValuedFunction method).
    /// 
    /// <para>This class is super Microsoft Sql Server specific.  It is not suitable to create backup triggers on tables in which you expect high volitility (lots of frequent
    /// updates all the time).</para>
    /// 
    /// <para>Also contains methods for confirming that a trigger exists on a given table, that the primary keys still match when it was created and the _Archive table hasn't
    /// got a different schema to the live table (e.g. if you made a change to the live table this would pick up that the _Archive wasn't updated).</para>
    /// </summary>
    internal class MicrosoftSQLTriggerImplementer:TriggerImplementer
    {
        public MicrosoftSQLTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true) : base(table, createDataLoadRunIDAlso)
        {
        }

        public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
        {
            using (var con = _server.GetConnection())
            {
                con.Open();
                
                string triggerName = "[dbo]." + _table.GetRuntimeName() + "_OnUpdate";

                problemsDroppingTrigger = "";
                thingsThatWorkedDroppingTrigger = "";

                var cmdDropTrigger = _server.GetCommand("DROP TRIGGER " + triggerName, con);
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

                var cmdDropArchiveIndex = _server.GetCommand("DROP INDEX PKsIndex ON " + _archiveTable.GetRuntimeName(), con);
                try
                {
                    cmdDropArchiveIndex.ExecuteNonQuery();

                    thingsThatWorkedDroppingTrigger += "Dropped index PKsIndex on Archive table successfully" + Environment.NewLine;
                }
                catch (Exception exception)
                {
                    problemsDroppingTrigger += "Failed to drop Archive Index:" + exception.Message + Environment.NewLine;
                }

                var cmdDropArchiveLegacyView = _server.GetCommand("DROP FUNCTION " + _table.GetRuntimeName() + "_Legacy", con);
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
        }

        public override void CreateTrigger(ICheckNotifier notifier, int createArchiveIndexTimeout = 30)
        {

            //if _Archive exists skip creating it
            bool skipCreatingArchive = _archiveTable.Exists();
            
            //check _Archive does not already exist
            foreach (string forbiddenColumnName in new[] { "hic_validTo", "hic_userID", "hic_status" })
                if (_columns.Any(c=>c.GetRuntimeName().Equals(forbiddenColumnName)))
                    throw new Exception("Table " + _table + " already contains a column called " + forbiddenColumnName + " this column is reserved for Archiving");

            bool b_mustCreate_validFrom = !_columns.Any(c=>c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom));
            bool b_mustCreate_dataloadRunId = !_columns.Any(c => c.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID)) && _createDataLoadRunIdAlso;

            //forces column order dataloadrunID then valid from (doesnt prevent these being in the wrong place in the record but hey ho - possibly not an issue anyway since probably the 3 values in the archive are what matters for order - see the Trigger which populates *,X,Y,Z where * is all columns in mane table
            if(b_mustCreate_dataloadRunId && !b_mustCreate_validFrom)
                throw new Exception("Cannot create trigger because table contains "+SpecialFieldNames.ValidFrom+" but not " + SpecialFieldNames.DataLoadRunID + " (ID must be placed before valid from in column order)");
                
            using(var con = _server.GetConnection())
            {
                con.Open();

                //must add validFrom outside of transaction if we want SMO to pick it up
                if (b_mustCreate_dataloadRunId)
                {
                    var cmdAlterToAddDataLoadRunID = _server.GetCommand("ALTER TABLE " + _table + " ADD " + SpecialFieldNames.DataLoadRunID + " int null", con);
                    cmdAlterToAddDataLoadRunID.ExecuteNonQuery();
                }

                //must add validFrom outside of transaction if we want SMO to pick it up
                if (b_mustCreate_validFrom)
                {
                    var cmdAlterToAddValidTo = _server.GetCommand("ALTER TABLE " + _table + " ADD " + SpecialFieldNames.ValidFrom + " datetime DEFAULT getdate() ", con);
                    cmdAlterToAddValidTo.ExecuteNonQuery();
                }

                string createArchiveTableSQL = WorkOutArchiveTableCreationSQL();
                        
                if (!skipCreatingArchive)
                {

                    //select top 0 into _Archive
                    var cmdCreateArchive = _server.GetCommand(createArchiveTableSQL, con);

                    cmdCreateArchive.ExecuteNonQuery();
                        

                    //alter to add columns
                    var cmdAlterToAddRequiredColumns = _server.GetCommand(
                        "ALTER TABLE " + _archiveTable+ " ADD hic_validTo datetime;" +
                        "ALTER TABLE " + _archiveTable+ " ADD hic_userID varchar(128);" +
                        "ALTER TABLE " + _archiveTable+ " ADD hic_status char(1);", con); 
                                             
                    cmdAlterToAddRequiredColumns.ExecuteNonQuery();
                }

                string trigger = GetCreateTriggerSQL();
                var cmdAddTrigger = _server.GetCommand(trigger, con);
                cmdAddTrigger.ExecuteNonQuery();
                    
                //Add key so that we can more easily do comparisons on primary key between main table and archive
                string idxCompositeKeyBody = "";

                foreach (var key in _primaryKeys)
                    idxCompositeKeyBody += "[" + key.GetRuntimeName() + @"] ASC,";

                //remove trailing comma
                idxCompositeKeyBody = idxCompositeKeyBody.TrimEnd(',');

                string createIndexSQL = @"CREATE NONCLUSTERED INDEX [PKsIndex] ON [dbo].[" + _archiveTable.GetRuntimeName() + "](" +idxCompositeKeyBody + ")";
                var cmdCreateIndex = _server.GetCommand(createIndexSQL, con);

                try
                {
                    cmdCreateIndex.CommandTimeout = createArchiveIndexTimeout;
                    cmdCreateIndex.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Could not create index on archive table because of timeout, possibly your _Archive table has a lot of data in it",
                        CheckResult.Fail, e));
                            
                    return;
                }

                CreateViewOldVersionsTableValuedFunction( createArchiveTableSQL,con);
            }
        }

        private string GetCreateTriggerSQL()
        {
            string triggerName = "[dbo]." + _table.GetRuntimeName() + "_OnUpdate";

            if (!_primaryKeys.Any())
                throw new Exception("There must be at least 1 primary key");

            //this is the SQL to join on the main table to the deleted to record the hic_validFrom
            string updateValidToWhere = " UPDATE " + _table +
                                        " SET "+SpecialFieldNames.ValidFrom+" = GETDATE() FROM deleted where ";

            //its a combo field so join on both when filling in hic_validFrom
            foreach (DiscoveredColumn key in _primaryKeys)
                updateValidToWhere += "[" + _table + "].[" + key + "] = deleted.[" + key + "] AND ";

            //trim off last AND
            updateValidToWhere = updateValidToWhere.Substring(0, updateValidToWhere.Length - "AND ".Length);

            string InsertedToDeletedJoin = "JOIN inserted i ON ";
            InsertedToDeletedJoin += GetTableToTableEqualsSqlWithPrimaryKeys("i", "d");

            string equalsSqlTableToInserted = GetTableToTableEqualsSqlWithPrimaryKeys(_table.GetRuntimeName(),"inserted");
            string equalsSqlTableToDeleted = GetTableToTableEqualsSqlWithPrimaryKeys(_table.GetRuntimeName(), "deleted");


            string colList = string.Join(",", _columns.Select(c => c.GetRuntimeName()).Union(new String[] { SpecialFieldNames.DataLoadRunID ,SpecialFieldNames.ValidFrom}));
            string dDotColList = string.Join(",", _columns.Select(c => "d." + c.GetRuntimeName()).Union(new String[] { "d."+SpecialFieldNames.DataLoadRunID, "d."+SpecialFieldNames.ValidFrom }));

            return
                @"
CREATE TRIGGER " + triggerName + " ON [dbo].[" + _table + @"]
AFTER UPDATE, DELETE
AS BEGIN

declare @isPrimaryKeyChange bit = 0

--it will be a primary key change if deleted and inserted do not agree on primary key values
IF exists ( select 1 FROM deleted d RIGHT " + InsertedToDeletedJoin + @" WHERE d.[" + _primaryKeys.First().GetRuntimeName() + @"] is null)
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

INSERT INTO " + _archiveTable.GetRuntimeName() + @" (" + colList + @",hic_validTo,hic_userID,hic_status) SELECT " + dDotColList +
                ", GETDATE(), SYSTEM_USER, CASE WHEN @isPrimaryKeyChange = 1 then 'K' WHEN i.[" + _primaryKeys.First().GetRuntimeName() +
                "] IS NULL THEN 'D' WHEN d.[" + _primaryKeys.First().GetRuntimeName() + @"] IS NULL THEN 'I' ELSE 'U' END
FROM deleted d 
LEFT " + InsertedToDeletedJoin + @"

SET NOCOUNT OFF

RETURN
END  
";
        }

        private string GetTableToTableEqualsSqlWithPrimaryKeys(string table1, string table2)
        {
            string toReturn = "";

            foreach (DiscoveredColumn key in _primaryKeys)
                toReturn += " " + table1 + ".[" + key.GetRuntimeName() + "] = " + table2 + ".[" + key.GetRuntimeName() + "] AND ";

            //trim off last AND
            toReturn = toReturn.Substring(0, toReturn.Length - "AND ".Length);
            
            return toReturn;
        }

        private void CreateViewOldVersionsTableValuedFunction(string sqlUsedToCreateArchiveTableSQL,DbConnection con)
        {
            string columnsInArchive = "";

            Match matchStartColumnExtraction = Regex.Match(sqlUsedToCreateArchiveTableSQL, "CREATE TABLE .*\\(");

            if (!matchStartColumnExtraction.Success)
                throw new Exception("Could not find regex match at start of Archive table CREATE SQL");

            int startExtractingColumnsAt = matchStartColumnExtraction.Index + matchStartColumnExtraction.Length;
            //trim off excess crud at start and we should have just the columns bit of the create (plus crud at the end)
            columnsInArchive = sqlUsedToCreateArchiveTableSQL.Substring(startExtractingColumnsAt);

            //trim off excess crud at the end
            columnsInArchive = columnsInArchive.Trim(new[] {')', '\r', '\n'});
            
            string sqlToRun = string.Format("CREATE FUNCTION [dbo].[{0}_Legacy]", _table.GetRuntimeName());
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

            var liveCols = _columns.Select(c => c.GetRuntimeName()).Union(new String[] {SpecialFieldNames.DataLoadRunID, SpecialFieldNames.ValidFrom}).ToArray();

            string archiveCols = string.Join(",", liveCols) + ",hic_validTo,hic_userID,hic_status";
            string cDotArchiveCols = string.Join(",", liveCols.Select(s => "c." + s)); 


            sqlToRun += "\tINSERT @returntable" + Environment.NewLine;
            sqlToRun += string.Format("\tSELECT "+archiveCols+" FROM {0} WHERE @index BETWEEN ISNULL(" + SpecialFieldNames.ValidFrom + ", '1899/01/01') AND hic_validTo" + Environment.NewLine, _archiveTable);
            sqlToRun += Environment.NewLine;

            sqlToRun += "\tINSERT @returntable" + Environment.NewLine; ;
            sqlToRun += "\tSELECT " + cDotArchiveCols + ",NULL AS hic_validTo, NULL AS hic_userID, 'C' AS hic_status" + Environment.NewLine; //c is for current
            sqlToRun += string.Format("\tFROM {0} c" + Environment.NewLine, _table.GetRuntimeName());
            sqlToRun += "\tLEFT OUTER JOIN @returntable a ON " + Environment.NewLine;

            for (int index = 0; index < _primaryKeys.Length; index++)
            {
                sqlToRun += string.Format("\ta.{0}=c.{0} " + Environment.NewLine, 
                    RDMPQuerySyntaxHelper.EnsureValueIsWrapped(_primaryKeys[index].GetRuntimeName())); //add the primary key joins

                if (index + 1 < _primaryKeys.Length)
                    sqlToRun += "\tAND" + Environment.NewLine; //add an AND because there are more coming
            }

            sqlToRun += string.Format("\tWHERE a.{0} IS NULL -- where archive record doesn't exist" + Environment.NewLine, _primaryKeys.First().GetRuntimeName());
            sqlToRun += "\tAND @index > ISNULL(c." + SpecialFieldNames.ValidFrom + ", '1899/01/01')" + Environment.NewLine;

            sqlToRun += Environment.NewLine;
            sqlToRun += "RETURN" + Environment.NewLine;
            sqlToRun += "END" + Environment.NewLine;

            var cmd = _server.GetCommand(sqlToRun, con);
            cmd.ExecuteNonQuery();
        }

        private string WorkOutArchiveTableCreationSQL()
        {

            //script original table
            string createTableSQL = _table.ScriptTableCreation(true, true, true);

            string toReplaceTableName = Regex.Escape("CREATE TABLE " + _table );

            if (Regex.Matches(createTableSQL, toReplaceTableName).Count != 1)
                throw new Exception("Expected to find 1 occurrence of " + toReplaceTableName + " in the SQL " + createTableSQL);

            //rename table
            createTableSQL = Regex.Replace(createTableSQL, toReplaceTableName, "CREATE TABLE " + _archiveTable);

            string toRemoveIdentities = "IDENTITY\\(\\d+,\\d+\\)";

            //drop identity bit
            createTableSQL = Regex.Replace(createTableSQL, toRemoveIdentities, "");

            return createTableSQL;
        }

        public override TriggerStatus GetTriggerStatus()
        {
            var updateTriggerName = _table + "_OnUpdate";
            var queryTriggerIsItDisabledOrMissing = "USE [" + _table.Database.GetRuntimeName()+ @"]; 
if exists (select 1 from sys.triggers WHERE name=@triggerName) SELECT is_disabled  FROM sys.triggers WHERE name=@triggerName else select -1 is_disabled";
            
            try
            {
                using (var conn = _server.GetConnection())
                {
                    conn.Open();
                    var cmd = _server.GetCommand(queryTriggerIsItDisabledOrMissing, conn);
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

        public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
        {
            
            //check server has trigger and it is on 
            TriggerStatus isEnabledSimple = GetTriggerStatus();

            if (isEnabledSimple == TriggerStatus.Disabled || isEnabledSimple == TriggerStatus.Missing)
                return false;

            //now check the definition of it! - make sure it relates to primary keys etc
            var updateTriggerName = _table + "_OnUpdate";
            var query = "USE [" + _table.Database.GetRuntimeName()+ "];SELECT OBJECT_DEFINITION (object_id) FROM sys.triggers WHERE name='" + updateTriggerName + "' and is_disabled=0";

            try
            {
                using (var con = _server.GetConnection())
                {
                    con.Open();
                    var cmd = _server.GetCommand(query, con);
                    var result = cmd.ExecuteScalar() as string;

                    if (String.IsNullOrWhiteSpace(result))
                        throw new TriggerMissingException("Trigger " + updateTriggerName +
                                                          " does not have an OBJECT_DEFINITION or is missing or is disabled");

                    string expectedSQL = GetCreateTriggerSQL();

                    expectedSQL = expectedSQL.Trim();
                    result = result.Trim();

                    if (!expectedSQL.Equals(result))
                        throw new ExpectedIdenticalStringsException("Trigger " + updateTriggerName + " is corrupt",
                            expectedSQL, result);

                    CheckColumnDefinitionsMatchArchive();

                }
            }
            catch (IrreconcilableColumnDifferencesInArchiveException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Failed to check if trigger " + updateTriggerName + " is enabled.  See InnerException for details",e);
            }

            return true;
        }

        private void CheckColumnDefinitionsMatchArchive()
        {
            List<string> errors = new List<string>();
            
            var archiveTableCols =_archiveTable.DiscoverColumns().ToArray();

            foreach (DiscoveredColumn col in _columns)
            {
                var colInArchive = archiveTableCols.SingleOrDefault(c => c.GetRuntimeName().Equals(col.GetRuntimeName()));
                
                if(colInArchive == null)
                    errors.Add("Column " + col.GetRuntimeName() + " appears in Table '" + _table + "' but not in archive table '" + _archiveTable +"'");
                else
                    if(!AreCompatibleDatatypes(col.DataType,colInArchive.DataType))
                        errors.Add("Column " + col.GetRuntimeName() + " has data type '" + col.DataType + "' in '" + _table + "' but in Archive table '" + _archiveTable + "' it is defined as '" + colInArchive.DataType + "'");
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

    }
}