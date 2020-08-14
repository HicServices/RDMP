// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.Constraints;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using MySqlConnector;
using NUnit.Framework;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using YamlDotNet.Serialization;

namespace Tests.Common
{
    /// <summary>
    /// Base class for all integration tests which need to read/write to a database (sql server, mysql or oracle).
    /// </summary>
    [TestFixture]
    [Category("Database")]
    public class DatabaseTests
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        private static TestDatabasesSettings TestDatabaseSettings;

        public CatalogueRepository CatalogueRepository
        {
            get { return (CatalogueRepository) RepositoryLocator.CatalogueRepository; }
        }
        
        public DataExportRepository DataExportRepository 
        {
            get { return (DataExportRepository) RepositoryLocator.DataExportRepository; }
        }
        
        protected SqlConnectionStringBuilder UnitTestLoggingConnectionString;
        protected SqlConnectionStringBuilder DataQualityEngineConnectionString;
        
        
        protected DiscoveredServer DiscoveredServerICanCreateRandomDatabasesAndTablesOn;

        private readonly DiscoveredServer _discoveredMySqlServer;
        private readonly DiscoveredServer _discoveredOracleServer;
        private readonly DiscoveredServer _discoveredPostgresServer;
        private DiscoveredServer _discoveredSqlServer;

        static private Startup _startup;

        static DatabaseTests()
        {
            CatalogueRepository.SuppressHelpLoading = true;
            
            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();

            ReadSettingsFile();
        }

        private static void ReadSettingsFile()
        {
            const string settingsFile = "TestDatabases.txt";

            //see if there is a local text file first
            var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,settingsFile));

            if (!f.Exists) 
                throw new FileNotFoundException("Could not find file '" + f.FullName + "'");

            using(StreamReader s = new StreamReader(f.OpenRead()))
            {
                var deserializer = new DeserializerBuilder()
                    .Build();
            
                TestDatabaseSettings = (TestDatabasesSettings) deserializer.Deserialize(s, typeof(TestDatabasesSettings));
            }
        }

        public DatabaseTests()
        {

            var opts = new PlatformDatabaseCreationOptions()
            {
                ServerName = TestDatabaseSettings.ServerName,
                Prefix = TestDatabaseNames.Prefix,
                Username = TestDatabaseSettings.Username,
                Password = TestDatabaseSettings.Password
            };

            
            RepositoryLocator = new PlatformDatabaseCreationRepositoryFinder(opts);

            
            Console.WriteLine("Expecting Unit Test Catalogue To Be At Server=" + CatalogueRepository.DiscoveredServer.Name + " Database=" + CatalogueRepository.DiscoveredServer.GetCurrentDatabase());
            Assert.IsTrue(CatalogueRepository.DiscoveredServer.Exists(), "Catalogue database does not exist, run 'rdmp.exe install ...' to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to CreateDatabases.exe e.g. 'rdmp.exe install localhost\\sqlexpress TEST_')");
            Console.WriteLine("Found Catalogue");

            Console.WriteLine("Expecting Unit Test Data Export To Be At Server=" + DataExportRepository.DiscoveredServer.Name + " Database= " + DataExportRepository.DiscoveredServer.GetCurrentDatabase());
            Assert.IsTrue(DataExportRepository.DiscoveredServer.Exists(), "Data Export database does not exist, run 'rdmp.exe install ...' to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to CreateDatabases.exe e.g. 'rdmp.exe install localhost\\sqlexpress TEST_')");
            Console.WriteLine("Found DataExport");
            
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);

            RunBlitzDatabases(RepositoryLocator);

            var defaults = CatalogueRepository.GetServerDefaults();

            DataQualityEngineConnectionString = CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, PlatformDatabaseCreation.DefaultDQEDatabaseName, PermissableDefaults.DQE,new DataQualityEnginePatcher());
            UnitTestLoggingConnectionString = CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, PlatformDatabaseCreation.DefaultLoggingDatabaseName, PermissableDefaults.LiveLoggingServer_ID, new LoggingDatabasePatcher());
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn = new DiscoveredServer(CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, null, PermissableDefaults.RAWDataLoadServer, null));

            _discoveredSqlServer = new DiscoveredServer(TestDatabaseSettings.ServerName,null,DatabaseType.MicrosoftSQLServer,TestDatabaseSettings.Username,TestDatabaseSettings.Password);

            if (TestDatabaseSettings.MySql != null)
            {
                var builder = new MySqlConnectionStringBuilder(TestDatabaseSettings.MySql);
                
                foreach (string k in builder.Keys)
                {
                    if (k == "server" || k == "database" || k== "user id" || k =="password")
                        continue;

                    new ConnectionStringKeyword(CatalogueRepository, DatabaseType.MySql, k, builder[k].ToString());
                }
                _discoveredMySqlServer = new DiscoveredServer(builder);
            }

            if (TestDatabaseSettings.Oracle != null)
                _discoveredOracleServer = new DiscoveredServer(TestDatabaseSettings.Oracle, DatabaseType.Oracle);

            if(TestDatabaseSettings.PostgreSql != null)
                _discoveredPostgresServer = new DiscoveredServer(TestDatabaseSettings.PostgreSql, DatabaseType.PostgreSql);
        }

        private SqlConnectionStringBuilder CreateServerPointerInCatalogue(IServerDefaults defaults, string prefix, string databaseName, PermissableDefaults defaultToSet,IPatcher patcher)
        {
            var opts = new PlatformDatabaseCreationOptions()
            {
                ServerName = TestDatabaseSettings.ServerName,
                Prefix = prefix,
                Username = TestDatabaseSettings.Username,
                Password = TestDatabaseSettings.Password
            };

            var builder = opts.GetBuilder(databaseName);

            if (string.IsNullOrWhiteSpace(databaseName))
                builder.InitialCatalog = "";

            //create a new pointer
            var externalServerPointer = new ExternalDatabaseServer(CatalogueRepository, databaseName??"RAW",patcher)
            {
                Server = builder.DataSource,
                Database = builder.InitialCatalog,
                Password = builder.Password,
                Username = builder.UserID
            };

            externalServerPointer.SaveToDatabase();

            //now make it the default DQE
            defaults.SetDefault(defaultToSet, externalServerPointer);
            
            return builder;
        }
        
        /// <summary>
        /// Deletes all objects in the Catalogue and DataExport databases
        /// </summary>
        /// <param name="repositoryLocator"></param>
        protected void RunBlitzDatabases(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            using (var con = CatalogueRepository.GetConnection())
            {
                var catalogueDatabaseName = ((TableRepository) repositoryLocator.CatalogueRepository).DiscoveredServer.GetCurrentDatabase().GetRuntimeName();
                var dataExportDatabaseName = ((TableRepository) repositoryLocator.DataExportRepository).DiscoveredServer.GetCurrentDatabase().GetRuntimeName();

                UsefulStuff.ExecuteBatchNonQuery(string.Format(BlitzDatabases, catalogueDatabaseName, dataExportDatabaseName),con.Connection);
            }
        }

        /// <summary>
        /// Deletes all data tables except <see cref="ServerDefaults"/>, <see cref="ExternalDatabaseServer"/> and some other core tables which are required for access to
        /// DQE, logging etc
        /// </summary>
        protected void BlitzMainDataTables()
        {
            using (var con = CatalogueRepository.GetConnection())
            {
                var catalogueDatabaseName = ((TableRepository)RepositoryLocator.CatalogueRepository).DiscoveredServer.GetCurrentDatabase().GetRuntimeName();
                var dataExportDatabaseName = ((TableRepository)RepositoryLocator.DataExportRepository).DiscoveredServer.GetCurrentDatabase().GetRuntimeName();

                UsefulStuff.ExecuteBatchNonQuery(string.Format(BlitzDatabasesLite, catalogueDatabaseName, dataExportDatabaseName), con.Connection);
            }
        }

        protected void RunBlitzDatabases()
        {
            RunBlitzDatabases(RepositoryLocator);
        }

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            //if it is the first time
            if (_startup == null)
            {
                _startup = new Startup(new EnvironmentInfo(),RepositoryLocator);

                _startup.DatabaseFound += StartupOnDatabaseFound;
                _startup.MEFFileDownloaded += StartupOnMEFFileDownloaded;
                _startup.PluginPatcherFound += StartupOnPluginPatcherFound;
                _startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
            }

            RepositoryLocator.CatalogueRepository.MEF.Setup(_startup.MEFSafeDirectoryCatalog);
        }

        /// <summary>
        /// override to specify setup behaviour
        /// </summary>
        [SetUp]
        protected virtual void SetUp()
        {

        }
   
        private void StartupOnDatabaseFound(object sender, PlatformDatabaseFoundEventArgs args)
        { 
            //its a healthy message, jolly good
            if (args.Status == RDMPPlatformDatabaseStatus.Healthy)
                return;

             if(args.Status == RDMPPlatformDatabaseStatus.SoftwareOutOfDate)
                Assert.Fail(@"Your TEST database schema is out of date with the API version you are testing with, 'run rdmp.exe install ...' to install the version which matches your nuget package.");

            if (args.Exception != null)
                Assert.Fail(args.SummariseAsString());

            //its a tier appropriate fatal error message
            if (args.Status == RDMPPlatformDatabaseStatus.Broken || args.Status == RDMPPlatformDatabaseStatus.Unreachable)
                Assert.Fail(args.SummariseAsString());

            //its slightly dodgy about it's version numbers
            if (args.Status == RDMPPlatformDatabaseStatus.RequiresPatching)
                Assert.Fail(args.SummariseAsString());

        }
        private void StartupOnPluginPatcherFound(object sender, PluginPatcherFoundEventArgs args)
        {
            Assert.IsTrue(args.Status == PluginPatcherStatus.Healthy, "PluginPatcherStatus is " + args.Status + " for plugin " + args.Type.Name + Environment.NewLine + (args.Exception == null ? "No exception" : ExceptionHelper.ExceptionToListOfInnerMessages(args.Exception)));
        }

        private void StartupOnMEFFileDownloaded(object sender, MEFFileDownloadProgressEventArgs args)
        {
            Assert.IsTrue(args.Status == MEFFileDownloadEventStatus.Success || args.Status == MEFFileDownloadEventStatus.FailedDueToFileLock, "MEFFileDownloadEventStatus is " + args.Status + " for plugin " + args.FileBeingProcessed + Environment.NewLine + (args.Exception == null ? "No exception" : ExceptionHelper.ExceptionToListOfInnerMessages(args.Exception)));
        }
        
        
        public const string BlitzDatabases = @"
--If you want to blitz everything out of your test catalogue and data export database(s) then run the following SQL (adjusting for database names):

delete from {0}..ConnectionStringKeyword
delete from {0}..JoinableCohortAggregateConfigurationUse
delete from {0}..JoinableCohortAggregateConfiguration
delete from {0}..CohortIdentificationConfiguration
delete from {0}..CohortAggregateContainer

delete from {0}..AggregateConfiguration
delete from {0}..AggregateFilter
delete from {0}..AggregateFilterContainer
delete from {0}..AggregateFilterParameter

delete from {0}..AnyTableSqlParameter

delete from {0}..ColumnInfo
delete from {0}..ANOTable

delete from {0}..PreLoadDiscardedColumn
delete from {0}..TableInfo
delete from {0}..DataAccessCredentials

update {0}..Catalogue set PivotCategory_ExtractionInformation_ID = null
update {0}..Catalogue set TimeCoverage_ExtractionInformation_ID = null
GO

delete from {0}..ExtractionFilterParameterSetValue
delete from {0}..ExtractionFilterParameterSet

delete from {0}..ExtractionInformation

delete from {0}..CatalogueItemIssue
delete from {0}..IssueSystemUser

delete from {0}..SupportingDocument
delete from {0}..SupportingSQLTable

delete from {0}..GovernanceDocument
delete from {0}..GovernancePeriod_Catalogue
delete from {0}..GovernancePeriod

delete from {0}..Catalogue

delete from {0}..CacheProgress
delete from {0}..LoadProgress
delete from {0}..LoadMetadata

delete from {0}..ServerDefaults
delete from {0}..ExternalDatabaseServer
delete from {0}..Favourite

delete from {0}..PipelineComponentArgument
delete from {0}..Pipeline
delete from {0}..PipelineComponent

delete from {0}..ObjectExport
delete from {0}..ObjectImport

delete from {0}..LoadModuleAssembly
delete from {0}..Plugin

delete from {1}..ReleaseLog
delete from {1}..SupplementalExtractionResults
delete from {1}..CumulativeExtractionResults
delete from {1}..ExtractableColumn
delete from {1}..SelectedDataSets

delete from {1}..GlobalExtractionFilterParameter
delete from {1}..ExtractionConfiguration

delete from {1}..ConfigurationProperties

delete from {1}..DeployedExtractionFilterParameter
delete from {1}..DeployedExtractionFilter
delete from {1}..FilterContainer

delete from {1}..ExtractableCohort
delete from {1}..ExternalCohortTable

delete from {1}..ExtractableDataSetPackage

delete from {1}..ExtractableDataSet
delete from {1}..Project
";



        public const string BlitzDatabasesLite = @"
--If you want to blitz everything out of your test catalogue and data export database(s) then run the following SQL (adjusting for database names):

delete from {0}..ConnectionStringKeyword
delete from {0}..JoinableCohortAggregateConfigurationUse
delete from {0}..JoinableCohortAggregateConfiguration
delete from {0}..CohortIdentificationConfiguration
delete from {0}..CohortAggregateContainer

delete from {0}..AggregateConfiguration
delete from {0}..AggregateFilter
delete from {0}..AggregateFilterContainer
delete from {0}..AggregateFilterParameter

delete from {0}..AnyTableSqlParameter

delete from {0}..ColumnInfo
delete from {0}..ANOTable

delete from {0}..PreLoadDiscardedColumn
delete from {0}..TableInfo
delete from {0}..DataAccessCredentials

update {0}..Catalogue set PivotCategory_ExtractionInformation_ID = null
update {0}..Catalogue set TimeCoverage_ExtractionInformation_ID = null
GO

delete from {0}..ExtractionFilterParameterSetValue
delete from {0}..ExtractionFilterParameterSet

delete from {0}..ExtractionInformation

delete from {0}..CatalogueItemIssue
delete from {0}..IssueSystemUser

delete from {0}..SupportingDocument
delete from {0}..SupportingSQLTable

delete from {0}..GovernanceDocument
delete from {0}..GovernancePeriod_Catalogue
delete from {0}..GovernancePeriod

delete from {0}..Catalogue

delete from {0}..CacheProgress
delete from {0}..LoadProgress
delete from {0}..LoadMetadata

delete from {0}..Favourite

delete from {0}..PipelineComponentArgument
delete from {0}..Pipeline
delete from {0}..PipelineComponent

delete from {0}..ObjectExport
delete from {0}..ObjectImport

delete from {0}..LoadModuleAssembly
delete from {0}..Plugin

delete from {1}..ReleaseLog
delete from {1}..SupplementalExtractionResults
delete from {1}..CumulativeExtractionResults
delete from {1}..ExtractableColumn
delete from {1}..SelectedDataSets

delete from {1}..GlobalExtractionFilterParameter
delete from {1}..ExtractionConfiguration

delete from {1}..ConfigurationProperties

delete from {1}..DeployedExtractionFilterParameter
delete from {1}..DeployedExtractionFilter
delete from {1}..FilterContainer

delete from {1}..ExtractableCohort
delete from {1}..ExternalCohortTable

delete from {1}..ExtractableDataSetPackage

delete from {1}..ExtractableDataSet
delete from {1}..Project
";

        /// <summary>
        /// returns a Trimmed string in which all whitespace including newlines have been replaced by single spaces.  Useful for checking the exact values of expected
        /// queries built by query builders without having to worry about individual lines having leading/trailing whitespace etc.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected string CollapseWhitespace(string sql)
        {
            //replace all whitespace with single spaces
            return Regex.Replace(sql, @"\s+", " ").Trim();
        }
        
        HashSet<DiscoveredDatabase> forCleanup = new HashSet<DiscoveredDatabase>();

        /// <summary>
        /// Gets an empty database on the test server of the appropriate DBMS
        /// </summary>
        /// <param name="type">The DBMS you want a server of (a valid connection string must exist in TestDatabases.txt)</param>
        /// <param name="dbnName">null for default test database name (recommended unless you are testing moving data from one database to another on the same test server)</param>
        /// <returns></returns>
        protected DiscoveredDatabase GetCleanedServer(DatabaseType type, string dbnName = null)
        {
            //the standard scratch area database
            string standardName = TestDatabaseNames.GetConsistentName("ScratchArea");

            //if user specified the standard name or no name
            bool isStandardDb = dbnName == null || dbnName == standardName;
            
            //use the standard name if they haven't specified one
            if(dbnName == null)
                dbnName = standardName;

            DiscoveredServer server;

            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    server = _discoveredSqlServer == null ? null : new DiscoveredServer(_discoveredSqlServer.Builder);
                    break;
                case DatabaseType.MySql:
                    server = _discoveredMySqlServer == null ? null : new DiscoveredServer(_discoveredMySqlServer.Builder);
                    break;
                case DatabaseType.Oracle:
                    server = _discoveredOracleServer == null ? null : new DiscoveredServer(_discoveredOracleServer.Builder);
                    break;
                case DatabaseType.PostgreSql:
                    server = _discoveredPostgresServer == null ? null : new DiscoveredServer(_discoveredPostgresServer.Builder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            if (server == null)
                Assert.Inconclusive();

            //the microsoft one should exist! others are optional
            if (!server.Exists() && type != DatabaseType.MicrosoftSQLServer)
                Assert.Inconclusive();

            server.TestConnection();

            var database = server.ExpectDatabase(dbnName);

            if (database.Exists())
            {
                DeleteTables(database);
            }
            else
                database.Create(true);

            server.ChangeDatabase(dbnName);

            Assert.IsTrue(database.Exists());

            //if it had non standard naming mark it for deletion on cleanup
            if (!isStandardDb)
                forCleanup.Add(database);

            return database;
        }

        protected void DeleteTables(DiscoveredDatabase database)
        {
            var tables = new RelationshipTopologicalSort(database.DiscoverTables(true));

            foreach (var t in tables.Order.Reverse())
                try
                {
                    t.Drop();
                }
                catch (Exception ex)
                {
                    throw new Exception( $"Failed to drop table '{t.GetFullyQualifiedName()} during cleanup",ex);
                }
            
            foreach (var t in database.DiscoverTableValuedFunctions())
                try
                {
                    
                    t.Drop();
                }
                catch (Exception ex)
                {
                    throw new Exception( $"Failed to drop table '{t.GetFullyQualifiedName()} during cleanup",ex);
                }

        }

        protected Catalogue Import(DiscoveredTable tbl, out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated, out CatalogueItem[] catalogueItems, out ExtractionInformation[] extractionInformations)
        {
            var importer = new TableInfoImporter(CatalogueRepository, tbl);
            importer.DoImport(out tableInfoCreated,out columnInfosCreated);

            var forwardEngineer = new ForwardEngineerCatalogue(tableInfoCreated, columnInfosCreated,true);

            Catalogue catalogue;
            forwardEngineer.ExecuteForwardEngineering(out catalogue,out catalogueItems,out extractionInformations);

            return catalogue;
        }

        protected Catalogue Import(DiscoveredTable tbl)
        {
            TableInfo tableInfoCreated;
            ColumnInfo[] columnInfosCreated;
            CatalogueItem[] catalogueItems;
            ExtractionInformation[] extractionInformations;

            return Import(tbl, out tableInfoCreated, out columnInfosCreated, out catalogueItems,out extractionInformations);
        }

        protected Catalogue Import(DiscoveredTable tbl, out TableInfo tableInfoCreated,out ColumnInfo[] columnInfosCreated)
        {
            CatalogueItem[] catalogueItems;
            ExtractionInformation[] extractionInformations;

            return Import(tbl, out tableInfoCreated, out columnInfosCreated, out catalogueItems, out extractionInformations);
        }

        protected void VerifyRowExist(DataTable resultTable, params object[] rowObjects)
        {
            if (resultTable.Columns.Count != rowObjects.Length)
                Assert.Fail("VerifyRowExist failed, resultTable had " + resultTable.Columns.Count + " while you expected " + rowObjects.Length + " columns");

            foreach (DataRow r in resultTable.Rows)
            {
                bool matchAll = true;
                for (int i = 0; i < rowObjects.Length; i++)
                {
                    if (!AreBasicallyEquals(rowObjects[i], r[i]))
                        matchAll = false;
                }

                //found a row that matches on all params
                if (matchAll)
                    return;
            }

            Assert.Fail("VerifyRowExist failed, did not find expected rowObjects (" + string.Join(",", rowObjects.Select(o => "'" + o + "'")) + ") in the resultTable");
        }

        public static bool AreBasicallyEquals(object o, object o2, bool handleSlashRSlashN = true)
        {
            //if they are legit equals
            if (Equals(o, o2))
                return true;

            //if they are null but basically the same
            var oIsNull = o == null || o == DBNull.Value || o.ToString().Equals("0");
            var o2IsNull = o2 == null || o2 == DBNull.Value || o2.ToString().Equals("0");

            if (oIsNull || o2IsNull)
                return oIsNull == o2IsNull;

            //they are not null so tostring them deals with int vs long etc that DbDataAdapters can be a bit flaky on
            if (handleSlashRSlashN)
                return string.Equals(o.ToString().Replace("\r","").Replace("\n",""), o2.ToString().Replace("\r","").Replace("\n",""));
            
            return string.Equals(o.ToString(), o2.ToString());
        }


        [Flags]
        public enum TestLowPrivilegePermissions 
        {
            Reader = 1,
            Writer = 2,
            CreateAndDropTables = 4,

            All = Reader|Writer|CreateAndDropTables
        }

        protected void SetupLowPrivilegeUserRightsFor(DiscoveredDatabase db,TestLowPrivilegePermissions permissions)
        {
            SetupLowPrivilegeUserRightsFor(db, permissions, null);
        }
        protected void SetupLowPrivilegeUserRightsFor(TableInfo ti, TestLowPrivilegePermissions permissions)
        {
            var db = DataAccessPortal.GetInstance().ExpectDatabase(ti, DataAccessContext.InternalDataProcessing);
            SetupLowPrivilegeUserRightsFor(db, permissions, ti);
        }

        private void SetupLowPrivilegeUserRightsFor(DiscoveredDatabase db, TestLowPrivilegePermissions permissions, TableInfo ti)
        {
            var dbType = db.Server.DatabaseType;

            //get access to the database using the current credentials
            var username = TestDatabaseSettings.GetLowPrivilegeUsername(dbType);
            var password = TestDatabaseSettings.GetLowPrivilegePassword(dbType);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                Assert.Inconclusive();

            //give the user access to the table
            var sql = GrantAccessSql(username, dbType, permissions);

            using (var con = db.Server.GetConnection())
                UsefulStuff.ExecuteBatchNonQuery(sql, con);

            if (ti != null)
            {
                //remove any existing credentials
                foreach (DataAccessCredentials cred in CatalogueRepository.GetAllObjects<DataAccessCredentials>())
                    CatalogueRepository.TableInfoCredentialsManager.BreakAllLinksBetween(cred, ti);

                //set the new ones
                DataAccessCredentialsFactory credentialsFactory = new DataAccessCredentialsFactory(CatalogueRepository);
                credentialsFactory.Create(ti, username, password, DataAccessContext.Any);
            }
            
        }


        private string GrantAccessSql(string username, DatabaseType type, TestLowPrivilegePermissions permissions)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return string.Format(@"
if exists (select * from sys.sysusers where name = '{0}')
	drop user [{0}]
GO

CREATE USER [{0}] FOR LOGIN [{0}]
GO
{1} ALTER ROLE [db_datareader] ADD MEMBER [{0}]
{2} ALTER ROLE [db_datawriter] ADD MEMBER [{0}]
{3} ALTER ROLE [db_ddladmin] ADD MEMBER [{0}]
GO
", username,
 permissions.HasFlag(TestLowPrivilegePermissions.Reader) ? "" : "--",
 permissions.HasFlag(TestLowPrivilegePermissions.Reader) ? "" : "--",
 permissions.HasFlag(TestLowPrivilegePermissions.CreateAndDropTables) ? "" : "--");
                case DatabaseType.MySql:
                    break;
                case DatabaseType.Oracle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        protected void Clear(LoadDirectory loadDirectory)
        {
            DeleteFilesIn(loadDirectory.ForLoading);
            DeleteFilesIn(loadDirectory.ForArchiving);
            DeleteFilesIn(loadDirectory.Cache);
        }

        protected void DeleteFilesIn(DirectoryInfo dir)
        {
            foreach (var f in dir.GetFiles())
                f.Delete();

            foreach (var d in dir.GetDirectories())
                d.Delete(true);
        }

    }


    public static class TestDatabaseNames
    {
        public static string Prefix;

        public static string GetConsistentName(string databaseName)
        {
            return Prefix + databaseName;
        }
    }
}