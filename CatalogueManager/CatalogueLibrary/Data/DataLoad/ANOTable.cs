using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Defines an anonymisation method for a group of related columns of the same datatype.  For example 'ANOGPCode' could be an instance/record that defines input of type 
    /// varchar(5) and anonymises into 3 digits and 2 characters with a suffix of _G.  This product would then be used by all ColumnInfos that contain GP codes (current GP
    /// previous GP, Prescriber code etc).  Anonymisation occurs at  ColumnInfo level after being loaded from a RAW data load bubble as is pushed to the STAGING bubble.
    /// 
    /// Each ANOTable describes a corresponding table on an ANO server (see the Server_ID property - we refer to this as an ANOStore) including details of the transformation and a UNIQUE name/suffix.  This let's you
    /// quickly identify what data has be annonymised by what ANOTable when you find a random excel sheet kicking around on a hard disk devoid of context.
    ///  
    /// It is very important to curate your ANOTables properly or you could end up with irrecoverable data, for example sticking to a single ANO server, taking regular backups
    /// NEVER deleting ANOTables that reference existing data  (in the ANOStore database).
    /// 
    /// The ANOTable.cs class does it's best to ensure you do not corrupt the data in your Catalogue e.g. by loading 10 files then changing the transform then loading 10 more which
    /// would result in mixed typed data in the final anonymised state but if you edit the ANOTable data directly you could still end up with corrupted configurations.  Fortunately
    /// in almost all cases it should be possible to manually salvage the configuration without loosing data.  The only time you would loose data is when you delete the identifier substitutions
    /// off the ANOStore server (see DeleteANOTableInANOStore method which will let you do this for empty tables)
    /// 
    /// </summary>
    public class ANOTable : VersionedDatabaseEntity, ISaveable, IDeleteable,ICheckable,IRevertable, IHasDependencies
    {
        public const string ANOPrefix = "ANO";

        private string _identifiableDataType;
        private string _anonymousDataType;

        #region Database Properties

        private string _tableName;
        private int _numberOfIntegersToUseInAnonymousRepresentation;
        private int _numberOfCharactersToUseInAnonymousRepresentation;
        private string _suffix;
        private int _serverID;

        public string TableName
        {
            get { return _tableName; }
            set { SetField(ref  _tableName, value); }
        }

        public int NumberOfIntegersToUseInAnonymousRepresentation
        {
            get { return _numberOfIntegersToUseInAnonymousRepresentation; }
            set { SetField(ref  _numberOfIntegersToUseInAnonymousRepresentation, value); }
        }

        public int NumberOfCharactersToUseInAnonymousRepresentation
        {
            get { return _numberOfCharactersToUseInAnonymousRepresentation; }
            set { SetField(ref  _numberOfCharactersToUseInAnonymousRepresentation, value); }
        }
        public int Server_ID
        {
            get { return _serverID; }
            set { SetField(ref  _serverID, value); }
        }

        /// <summary>
        /// Once it is created you shouldn't be able to edit it's Suffix incase it is pushed and if it isn't pushed why didnt you just create it with the correct suffix in the first place?
        /// </summary>
        public string Suffix
        {
            get { return _suffix; }
            set { SetField(ref _suffix, value); }
        }
        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public ExternalDatabaseServer Server {
            get { return Repository.GetObjectByID<ExternalDatabaseServer>(Server_ID); }
        }
        #endregion

        public ANOTable(ICatalogueRepository repository, ExternalDatabaseServer externalDatabaseServer, string tableName, string suffix)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new NullReferenceException("ANOTable must have a name");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"TableName", tableName},
                {"Suffix", suffix},
                {"Server_ID", externalDatabaseServer.ID}
            });
        }

        internal ANOTable(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Server_ID = Convert.ToInt32(r["Server_ID"]);
            TableName = r["TableName"].ToString();

            NumberOfIntegersToUseInAnonymousRepresentation = Convert.ToInt32(r["NumberOfIntegersToUseInAnonymousRepresentation"].ToString());
            NumberOfCharactersToUseInAnonymousRepresentation = Convert.ToInt32(r["NumberOfCharactersToUseInAnonymousRepresentation"].ToString());
            Suffix = r["Suffix"].ToString();
        }

        public override void SaveToDatabase()
        {
            Check(new ThrowImmediatelyCheckNotifier());
            Repository.SaveToDatabase(this);
        }

        public override void DeleteInDatabase()
        {
            DeleteANOTableInANOStore();
            Repository.DeleteFromDatabase(this);
        }
        
        public override string ToString()
        {
            return TableName;
        }
        
        public void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(Suffix))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "You must choose a suffix for your ANO identifiers so that they can be distinguished from regular identifiers",
                        CheckResult.Fail));
            else
            if (Suffix.StartsWith("_"))
                notifier.OnCheckPerformed(new CheckEventArgs("Suffix will automatically include an underscore, there is no need to add it",CheckResult.Fail));
            
            if (NumberOfIntegersToUseInAnonymousRepresentation < 0)
                notifier.OnCheckPerformed(new CheckEventArgs("NumberOfIntegersToUseInAnonymousRepresentation cannot be negative", CheckResult.Fail));

            if (NumberOfCharactersToUseInAnonymousRepresentation < 0)
                notifier.OnCheckPerformed(new CheckEventArgs("NumberOfCharactersToUseInAnonymousRepresentation cannot be negative", CheckResult.Fail));
            
            if (NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation == 0)
                notifier.OnCheckPerformed(new CheckEventArgs("Anonymous representations must have at least 1 integer or character", CheckResult.Fail));
            try
            {
                if (!IsTablePushed())
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not find table " + TableName + " on server " + Server,CheckResult.Warning));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to get list of tables on server " + Server,CheckResult.Fail, e));
            }
        }

        public bool IsTablePushed()
        {
            return GetPushedTable() != null;
        }

        public DiscoveredTable GetPushedTable()
        {
            var tables = DataAccessPortal.GetInstance()
                .ExpectDatabase(Server, DataAccessContext.DataLoad)
                .DiscoverTables(false);

            return tables.SingleOrDefault(t => t.GetRuntimeName().Equals(TableName));
        }

        /// <summary>
        /// Connects to the remote ANO Server and creates a swap table of Identifier to ANOIdentifier
        /// </summary>
        /// <param name="identifiableDatatype">The datatype of the identifiable data table</param>
        public void PushToANOServerAsNewTable(string identifiableDatatype, ICheckNotifier notifier, DbConnection forceConnection=null,DbTransaction forceTransaction = null)
        {
            var server = DataAccessPortal.GetInstance().ExpectServer(Server, DataAccessContext.DataLoad);

            //matches varchar(100) and has capture group 100
            Regex regexGetLengthOfCharType =new Regex(@".*char.*\((\d*)\)");
            Match match = regexGetLengthOfCharType.Match(identifiableDatatype);

            //if user supplies varchar(100) and says he wants 3 ints and 3 chars in his anonymous identifiers he will soon run out of combinations 
            
            if (match.Success)
            {
                int length = Convert.ToInt32(match.Groups[1].Value);

                if (length >
                    NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "You asked to create a table with a datatype of length " + length + "(" + identifiableDatatype +
                            ") but you did not allocate an equal or greater number of anonymous identifier types (NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation=" +
                            (NumberOfCharactersToUseInAnonymousRepresentation +
                             NumberOfIntegersToUseInAnonymousRepresentation) + ")", CheckResult.Warning));
            }

            var con = forceConnection ?? server.GetConnection();//use the forced connection or open a new one
            
            try
            {
                if(forceConnection == null)
                    con.Open();
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not connect to ano server " + Server,CheckResult.Fail,e));
                return;
            }

            //if table name is ANOChi there are 2 columns Chi and ANOChi in it
            string anonymousColumnName = TableName;
            string identifiableColumnName = TableName.Substring("ANO".Length);

            string anonymousDatatype = "varchar(" +
                                        (NumberOfCharactersToUseInAnonymousRepresentation +
                                        NumberOfIntegersToUseInAnonymousRepresentation + "_".Length + Suffix.Length) + ")";


            var sql = "CREATE TABLE " + TableName + Environment.NewLine + " (" + Environment.NewLine +
                      identifiableColumnName + " " + identifiableDatatype + " NOT NULL," + Environment.NewLine +
                      anonymousColumnName + " " + anonymousDatatype +  "NOT NULL";

            sql += @",
CONSTRAINT PK_" + TableName+ @" PRIMARY KEY CLUSTERED 
(
	    "+identifiableColumnName+@" ASC
),
CONSTRAINT AK_" + TableName + @" UNIQUE(" + anonymousColumnName + @")
)";


            DbCommand cmd = server.GetCommand(sql, con);

            cmd.Transaction = forceTransaction;

            notifier.OnCheckPerformed(new CheckEventArgs("Decided appropriate create statement is:" + cmd.CommandText, CheckResult.Success));
            try
            {
                cmd.ExecuteNonQuery();

                if(forceConnection == null)//if we opened this ourselves
                    con.Close();//shut it
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Failed to successfully create the anonymous/identifier mapping Table in the ANO database on server " +
                        Server, CheckResult.Fail, e));
                return;
            }

            
            try
            {
                if(forceTransaction == null)//if there was no transaction then this has hit the LIVE ANO database and is for real, so save the ANOTable such that it is synchronized with reality
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Saving state because table has been pushed",CheckResult.Success));
                    SaveToDatabase();
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to save state after table was successfully? pushed to ANO server", CheckResult.Fail,e));
            }
            
        }

        public void DeleteANOTableInANOStore()
        {
            RevertToDatabaseState();

            //it must not be broken
            var server =  DataAccessPortal.GetInstance().ExpectServer(Server, DataAccessContext.DataLoad);
            
            var db = server.ExpectDatabase(Server.Database);

            //ANOTable references a database that does not exist so its ok to delete it
            if (!db.Exists())
                return;

            //ANOTable references a table that does not exist so it is ok to delete it (it would fail Check() anyway)
            if (!db.ExpectTable(TableName).Exists())
                return;
            
            using (var con = server.GetConnection())
            {
                con.Open();
                
                DbCommand cmdHowManyRows = server.GetCommand("Select count(*) from "+ TableName,con);
                cmdHowManyRows.CommandTimeout = 5000;
                
                int rowCount = Convert.ToInt32(cmdHowManyRows.ExecuteScalar());

                if(rowCount != 0)
                    throw new Exception("Cannot delete ANOTable because it references " + TableName + " which is a table on server " + Server + " which contains " + rowCount + " rows, deleting this reference would leave that table as an orphan, we can only delete when there are 0 rows in the table");

                DbCommand cmdDelete = server.GetCommand("Drop Table "+ TableName,con);
                cmdDelete.ExecuteNonQuery();

                con.Close();
            }
        }

        public string GetRuntimeDataType(LoadStage loadStage)
        {
            //cache answers
            if(_identifiableDataType == null)
            {
                var server = DataAccessPortal.GetInstance().ExpectServer(Server, DataAccessContext.DataLoad);
                
                DiscoveredColumn[] columnsFoundInANO = server.GetCurrentDatabase().ExpectTable(TableName).DiscoverColumns();

                string expectedIdentifiableName = TableName.Substring("ANO".Length);

                DiscoveredColumn anonymous = columnsFoundInANO.SingleOrDefault(c => c.GetRuntimeName().Equals(TableName));
                DiscoveredColumn identifiable = columnsFoundInANO.SingleOrDefault(c=>c.GetRuntimeName().Equals(expectedIdentifiableName));
                
                if(anonymous == null)
                    throw new Exception("Could not find a column called " + TableName + " in table " + TableName + " on server " + Server + " (Columns found were " + string.Join(",", columnsFoundInANO.Select(c=>c.GetRuntimeName()).ToArray()) + ")");

                if (identifiable == null)
                    throw new Exception("Could not find a column called " + expectedIdentifiableName + " in table " + TableName + " on server " + Server + " (Columns found were " + string.Join(",", columnsFoundInANO.Select(c => c.GetRuntimeName()).ToArray()) + ")");

                _identifiableDataType = identifiable.DataType.SQLType;
                _anonymousDataType = anonymous.DataType.SQLType;
            }

            //return cached answer
            switch (loadStage)
            {
                case LoadStage.GetFiles:
                    return _identifiableDataType;
                case LoadStage.Mounting:
                    return _identifiableDataType;
                case LoadStage.AdjustRaw:
                    return _identifiableDataType;
                case LoadStage.AdjustStaging:
                    return _anonymousDataType;
                case LoadStage.PostLoad:
                    return _anonymousDataType;
                default:
                    throw new ArgumentOutOfRangeException("loadStage");
            }
        }

        public int GetApproximateRowcount()
        {
            return DataAccessPortal.GetInstance()
                .ExpectDatabase(Server, DataAccessContext.DataLoad)
                .ExpectTable(TableName)
                .GetRowCount();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return Repository.GetAllObjectsWithParent<ColumnInfo>(this);
        }
    }
}
