using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Sockets;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Since every agency handles cohort management differently the RDMP is built to supports diverse cohort source schemas.  Unlike the logging, dqe, catalogue databases etc there
    /// is no fixed managed schema for cohort databases.  Instead you simply have to tell the software where to find your patient identifiers in an ExternalCohortTable record.  This
    /// stores:
    /// 
    /// What table contains your cohort identifiers
    /// Which column is the private identifier
    /// Which column is the release identifier
    /// 
    /// In addition to this you must have a table which describes your cohorts which must have columns called id, projectNumber, description, version and dtCreated.  And you must
    /// have a table which stores custom data for the cohort with a column customTableName containing the names of any data that accompanies cohorts.
    /// 
    /// Both the cohort and custom table names table must have a foreign key into the definition table.  
    /// 
    /// You are free to add additional columns to these tables or even base them on views of other existing tables in your database.  You can have multiple ExternalCohortTable sources
    /// in your database for example if you need to support different identifier datatypes / formats.
    /// 
    /// If all this sounds too complicated you can use the CreateNewCohortDatabaseWizardUI to automatically generate a database that is compatible with the format requirements and has
    /// release identifiers assigned automatically either as autonums or GUIDs (I suggest using GUIDs to prevent accidental crosstalk from ever occuring if you handle magic numbers from
    /// other agencies). 
    /// </summary>
    public class ExternalCohortTable : VersionedDatabaseEntity, IDataAccessCredentials, IExternalCohortTable,INamed
    {
        #region Database Properties
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        #endregion

        [NoMappingToDatabase]
        public SelfCertifyingDataAccessPoint SelfCertifyingDataAccessPoint { get; private set; }

        public string DefinitionTableForeignKeyField
        {
            get { return _definitionTableForeignKeyField; }
            set { SetField(ref _definitionTableForeignKeyField , RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(value)); }
        }

        public string TableName
        {
            get { return _tableName; }
            set { SetField(ref _tableName, GetQuerySyntaxHelper().EnsureFullyQualified(Database,null, value)); }
        }

        public string DefinitionTableName
        {
            get { return _definitionTableName; }
            set { SetField(ref _definitionTableName, GetQuerySyntaxHelper().EnsureFullyQualified(Database, null, value)); }
        }

        private string _customTablesTableName;

        public string CustomTablesTableName
        {
            get { return _customTablesTableName; }
            set { SetField(ref _customTablesTableName, GetQuerySyntaxHelper().EnsureFullyQualified(Database, null, value)); }
        }

        public string PrivateIdentifierField
        {
            get { return _privateIdentifierField; }
            set { SetField(ref _privateIdentifierField, GetQuerySyntaxHelper().EnsureFullyQualified(Database,null, TableName, value)); }
        }

        /// <summary>
        /// When reading this, use GetReleaseIdentifier(ExtractableCohort cohort) where possible to respect cohort.OverrideReleaseIdentifierSQL
        /// </summary>
        public string ReleaseIdentifierField
        {
            get { return _releaseIdentifierField; }
            set { SetField(ref _releaseIdentifierField, GetQuerySyntaxHelper().EnsureFullyQualified(Database,null, TableName, value)); }
        }

        public static readonly string[] CohortDefinitionTable_RequiredFields = new[]
        {
            "id",
            // joins to CohortToDefinitionTableJoinColumn and is used as ID in all ExtractableCohort entities throughout DataExportManager
            "projectNumber",
            "description",
            "version",
            "dtCreated"
        };

        private string _privateIdentifierField;
        private string _releaseIdentifierField;
        private string _tableName;
        private string _definitionTableName;
        private string _definitionTableForeignKeyField;

        public override string ToString()
        {
            return Name;
        }
        
        public ExternalCohortTable(IDataExportRepository repository, string name)
        {
            Repository = repository;
            SelfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(repository.CatalogueRepository,DatabaseType.MicrosoftSQLServer);
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name ?? "NewExternalSource" + Guid.NewGuid()}
            });
        }

        public ExternalCohortTable(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            SoftwareVersion = r["SoftwareVersion"].ToString();
            Name = r["Name"] as string;

            SelfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(((DataExportRepository)repository).CatalogueRepository,DatabaseType.MicrosoftSQLServer);

            Server = r["Server"] as string;
            Username = r["Username"] as string;
            Password = r["Password"] as string;

            if(!string.IsNullOrWhiteSpace(Server))
                Server = RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(Server);

            Database = r["Database"] as string;
            if (!string.IsNullOrWhiteSpace(Database))
                Database = RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(Database);

            DefinitionTableForeignKeyField = r["DefinitionTableForeignKeyField"] as string;

            var syntaxHelper = GetQuerySyntaxHelper();
            TableName = syntaxHelper.EnsureFullyQualified(Database, null, r["TableName"] as string);
            DefinitionTableName = syntaxHelper.EnsureFullyQualified(Database, null, r["DefinitionTableName"] as string);
            CustomTablesTableName = syntaxHelper.EnsureFullyQualified(Database, null, r["CustomTablesTableName"] as string);

            PrivateIdentifierField = syntaxHelper.EnsureFullyQualified(Database,null, TableName, r["PrivateIdentifierField"] as string);
            ReleaseIdentifierField = syntaxHelper.EnsureFullyQualified(Database,null, TableName, r["ReleaseIdentifierField"] as string);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new QuerySyntaxHelperFactory().Create(SelfCertifyingDataAccessPoint.DatabaseType);
        }

        public string GetReleaseIdentifier(IExtractableCohort cohort)
        {
            if (cohort == null)
                return ReleaseIdentifierField;

            //prefer override unless it is blank or null
            string toReturn = string.IsNullOrWhiteSpace(cohort.OverrideReleaseIdentifierSQL)
                ? ReleaseIdentifierField
                : cohort.OverrideReleaseIdentifierSQL;

            if (toReturn.Equals(PrivateIdentifierField))
                throw new Exception("ReleaseIdentifier for cohort " + cohort.ID +
                                    " is the same as the PrivateIdentifierSQL, this is forbidden");
            
            var syntaxHelper = GetQuerySyntaxHelper();

            if (syntaxHelper.GetRuntimeName(toReturn).Equals(syntaxHelper.GetRuntimeName(PrivateIdentifierField)))
                throw new Exception("ReleaseIdentifier for cohort " + cohort.ID +
                                    " is the same as the PrivateIdentifierSQL, this is forbidden");

            return toReturn;
        }

        public void Check(ICheckNotifier notifier)
        {
            //make sure we can get to server
            CheckCohortDatabaseAccessible( notifier);

            CheckCohortDatabaseHasCorrectTables( notifier);

            if (string.Equals(PrivateIdentifierField, ReleaseIdentifierField))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "PrivateIdentifierField and ReleaseIdentifierField are the same, this means your cohort will extract IDENTIFIABLE data (no cohort identifier substitution takes place)",
                        CheckResult.Fail));
        }

        #region Stuff for checking the remote (not data export manager) table where the cohort is allegedly stored
        
        public bool IDExistsInCohortTable(int originID)
        {
            var server = DataAccessPortal.GetInstance().ExpectServer(this, DataAccessContext.DataExport);
            
            using (DbConnection con = server.GetConnection())
            {
                con.Open();

                string sql = @"select count(*) from " + DefinitionTableName + " where id = " + originID;

                var cmdGetDescriptionOfCohortFromConsus = server.GetCommand(sql, con);
                try
                {
                    return int.Parse(cmdGetDescriptionOfCohortFromConsus.ExecuteScalar().ToString()) >= 1;
                }
                catch (Exception e)
                {
                    throw new Exception(
                        "Could not connect to server " + Server + " (Database '" + Database +
                        "') which is the data source of ExternalCohortTable (source) called '" + Name + "' (ID=" + ID +
                        ")", e);
                }
            }

        }
        private void CheckCohortDatabaseHasCorrectTables(ICheckNotifier notifier)
        {
            try
            {
                var database = DataAccessPortal.GetInstance().ExpectDatabase(this, DataAccessContext.DataExport);

                DiscoveredTable cohortTable = database.ExpectTable(TableName);
                if (cohortTable.Exists())
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Found table " + cohortTable + " in database " + Database, CheckResult.Success, null));

                    var columns = cohortTable.DiscoverColumns();

                    ComplainIfColumnMissing(TableName, columns, PrivateIdentifierField, notifier);
                    ComplainIfColumnMissing(TableName, columns, ReleaseIdentifierField, notifier);
                    ComplainIfColumnMissing(TableName, columns, DefinitionTableForeignKeyField, notifier);
                }
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not find table " + TableName + " in database " + Database, CheckResult.Fail, null));

                DiscoveredTable foundCohortDefinitionTable = database.ExpectTable(DefinitionTableName);

                if (foundCohortDefinitionTable.Exists())
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Found table " + DefinitionTableName + " in database " + Database, CheckResult.Success, null));

                    var cols = foundCohortDefinitionTable.DiscoverColumns();
                    
                    foreach (string requiredField in ExternalCohortTable.CohortDefinitionTable_RequiredFields)
                        ComplainIfColumnMissing(DefinitionTableName, cols, requiredField, notifier);

                }
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not find table " + DefinitionTableName + " in database " + Database, CheckResult.Fail, null));

                
                if (string.IsNullOrWhiteSpace(CustomTablesTableName))
                    notifier.OnCheckPerformed(new CheckEventArgs("No CustomTablesTableName configured for ExternalCohortSource:" + Name,CheckResult.Fail, null));
                else
                {
                    DiscoveredTable foundCustomTable = database.ExpectTable(CustomTablesTableName);

                    if (foundCustomTable.Exists())
                    {
                        var columns = foundCustomTable.DiscoverColumns();
                        ComplainIfColumnMissing(CustomTablesTableName, columns, DefinitionTableForeignKeyField, notifier);
                        ComplainIfColumnMissing(CustomTablesTableName, columns, "customTableName", notifier);
                        ComplainIfColumnMissing(CustomTablesTableName, columns, "active", notifier);
                    }
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not find table " + CustomTablesTableName + " in database " + Database, CheckResult.Fail, null));
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not check table intactness for ExternalCohortTable '" + Name + "'", CheckResult.Fail, e));
            }
        }
        
        private void CheckCohortDatabaseAccessible(ICheckNotifier notifier)
        {
            try
            {
                DataAccessPortal.GetInstance().ExpectServer(this, DataAccessContext.DataExport).TestConnection();
              
                notifier.OnCheckPerformed(new CheckEventArgs("Connected to Cohort database '" + Name + "'", CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not connect to Cohort database called '" + Name + "'", CheckResult.Fail, e));
            }
        }
        
        public void PushToServer(ICohortDefinition newCohortDefinition, DbConnection con, DbTransaction transaction)
        {
            var cmdInsert = DatabaseCommandHelper.GetCommand("INSERT INTO " + DefinitionTableName + "(projectNumber,version,description) VALUES (@projectNumber,@version,@description); SELECT @@IDENTITY;", con, transaction);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@projectNumber",cmdInsert,newCohortDefinition.ProjectNumber);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@version",cmdInsert,newCohortDefinition.Version);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@description",cmdInsert,newCohortDefinition.Description);
            
            newCohortDefinition.ID = Convert.ToInt32(cmdInsert.ExecuteScalar());
        }
        #endregion

        private void ComplainIfColumnMissing(string tableNameFullyQualified, DiscoveredColumn[] columns, string colToFindCanBeFullyQualifiedIfYouLike, ICheckNotifier notifier)
        {
            string tofind = GetQuerySyntaxHelper().GetRuntimeName(colToFindCanBeFullyQualifiedIfYouLike);

            if (columns.Any(col=>col.GetRuntimeName().Equals(tofind)))
                notifier.OnCheckPerformed(new CheckEventArgs("Found required field " + tofind + " in table " + tableNameFullyQualified,
                    CheckResult.Success, null));
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not find required field " + tofind + " in table " + tableNameFullyQualified +
                    "(It had the following columns:" + columns.Aggregate("", (s, n) => s + n + ",") + ")",
                    CheckResult.Fail, null));
        }
        
        public DiscoveredDatabase GetExpectDatabase()
        {
            return DataAccessPortal.GetInstance().ExpectServer(this, DataAccessContext.DataExport).ExpectDatabase(Database);
        }

        #region IDataAccessCredentials and IDataAccessPoint delegation
        public string Password
        {
            get { return SelfCertifyingDataAccessPoint.Password; }
            set
            {
                if (Equals(SelfCertifyingDataAccessPoint.Password, value))
                    return;

                SelfCertifyingDataAccessPoint.Password = value; 
                OnPropertyChanged();
            }
        }

        public string GetDecryptedPassword()
        {
            return ((IEncryptedPasswordHost) this).GetDecryptedPassword();
        }

        public string Username
        {
            get { return SelfCertifyingDataAccessPoint.Username; }
            set
            {
                if (Equals(SelfCertifyingDataAccessPoint.Username, value))
                    return;

                SelfCertifyingDataAccessPoint.Username = value;
                OnPropertyChanged();
            }
        }

        public string Server
        {
            get { return SelfCertifyingDataAccessPoint.Server; }
            set
            {
                if (Equals(SelfCertifyingDataAccessPoint.Server, value))
                    return;

                SelfCertifyingDataAccessPoint.Server = value;
                OnPropertyChanged();
            }
        }

        public string Database
        {
            get { return SelfCertifyingDataAccessPoint.Database; }
            set
            {
                if (Equals(SelfCertifyingDataAccessPoint.Database, value))
                    return;

                SelfCertifyingDataAccessPoint.Database = value; 
                OnPropertyChanged(); 
            }
        }
        [NoMappingToDatabase]
        public DatabaseType DatabaseType
        {
            get { return SelfCertifyingDataAccessPoint.DatabaseType; }
        }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            return SelfCertifyingDataAccessPoint.GetCredentialsIfExists(context);
        }
        #endregion



        public string GetCountsDataTableSql()
        {
            
            string sql = @"SELECT 
id as OriginID,
count(*) as Count,
count(distinct {0}) as CountDistinct,
projectNumber as ProjectNumber,
version as Version,
description as Description,
dtCreated as dtCreated
  FROM
   {1}
   join 
   {2} on {3} = id
   group by 
   id,
   projectNumber,
   version,
   description,
   dtCreated";

            return string.Format(sql, ReleaseIdentifierField, TableName, DefinitionTableName, DefinitionTableForeignKeyField);
        }

        public  string GetCustomTableSql()
        {
            string sql = @"Select 
{0} as OriginID,
customTableName as CustomTableName,
active
FROM
{1}
";

            return string.Format(sql, DefinitionTableForeignKeyField,CustomTablesTableName);

        }

        public string GetExternalDataSql()
        {
            string sql = @"SELECT 
id as OriginID,
projectNumber as ProjectNumber,
version as Version,
description as Description,
dtCreated as dtCreated
  FROM
   {0}";

                return string.Format(sql, DefinitionTableName);

        }
    }
}

