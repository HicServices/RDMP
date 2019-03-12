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
using System.Reflection;
using System.Text.RegularExpressions;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using DatabaseCreation;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using MapsDirectlyToDatabaseTable;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using RDMPStartup;
using RDMPStartup.Events;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Tests.Common
{
    [TestFixture]
    [Category("Database")]
    public class DatabaseTests
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        private static TestDatabasesSettings TestDatabaseSettings;

        public CatalogueRepository CatalogueRepository
        {
            get { return RepositoryLocator.CatalogueRepository; }
        }
        
        public IDataExportRepository DataExportRepository 
        {
            get { return RepositoryLocator.DataExportRepository; }
        }
        
        protected SqlConnectionStringBuilder UnitTestLoggingConnectionString;
        protected SqlConnectionStringBuilder DataQualityEngineConnectionString;
        
        protected DiscoveredDatabase DiscoveredDatabaseICanCreateRandomTablesIn;
        protected DiscoveredServer DiscoveredServerICanCreateRandomDatabasesAndTablesOn;

        private readonly DiscoveredServer _discoveredMySqlServer;
        private readonly DiscoveredServer _discoveredOracleServer;

        static private Startup _startup;

        static DatabaseTests()
        {
            CatalogueRepository.SuppressHelpLoading = true;
            
            ImplementationManager.Load(
                typeof(MicrosoftSQLImplementation).Assembly,
                typeof(OracleImplementation).Assembly,
                typeof(MySqlImplementation).Assembly
                );

            ReadSettingsFile();
        }

        private static void ReadSettingsFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Tests.Common.TestDatabases.txt";

            //see if there is a local text file first
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var f = dir.GetFiles("TestDatabases.txt").SingleOrDefault();
            
            //there is a local text file so favour that one
            if (f != null)
            {
                TestDatabaseSettings = ReadSettingsFileFromStream(f.OpenRead());
            }
            else
            {
                //otherwise use the embedded resource file
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    TestDatabaseSettings = ReadSettingsFileFromStream(stream);
            }
        }

        public DatabaseTests()
        {

            var opts = new DatabaseCreationProgramOptions()
            {
                ServerName = TestDatabaseSettings.ServerName,
                Prefix = TestDatabaseNames.Prefix
            };

            
            RepositoryLocator = new DatabaseCreationRepositoryFinder(opts);

            Console.WriteLine("Expecting Unit Test Catalogue To Be At Server=" + RepositoryLocator.CatalogueRepository.DiscoveredServer.Name + " Database=" + RepositoryLocator.CatalogueRepository.DiscoveredServer.GetCurrentDatabase());
            Assert.IsTrue(RepositoryLocator.CatalogueRepository.DiscoveredServer.Exists(), "Catalogue database does not exist, run DatabaseCreation.exe to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to CreateDatabases.exe e.g. 'DatabaseCreation.exe localhost\\sqlexpress TEST_')");
            Console.WriteLine("Found Catalogue");

            Console.WriteLine("Expecting Unit Test Data Export To Be At Server=" + RepositoryLocator.DataExportRepository.DiscoveredServer.Name + " Database= " + RepositoryLocator.DataExportRepository.DiscoveredServer.GetCurrentDatabase());
            Assert.IsTrue(DataExportRepository.DiscoveredServer.Exists(), "Data Export database does not exist, run DatabaseCreation.exe to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to CreateDatabases.exe e.g. 'DatabaseCreation.exe localhost\\sqlexpress TEST_')");
            Console.WriteLine("Found DataExport");
            
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);

            RunBlitzDatabases(RepositoryLocator);

            var defaults = CatalogueRepository.GetServerDefaults();

            DataQualityEngineConnectionString = CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, DatabaseCreationProgram.DefaultDQEDatabaseName, PermissableDefaults.DQE,typeof(DataQualityEngine.Database.Class1).Assembly);
            UnitTestLoggingConnectionString = CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, DatabaseCreationProgram.DefaultLoggingDatabaseName, PermissableDefaults.LiveLoggingServer_ID, typeof(HIC.Logging.Database.Class1).Assembly);
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn = new DiscoveredServer(CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, null, PermissableDefaults.RAWDataLoadServer, null));

            CreateScratchArea();
            
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
        }

        

        private static TestDatabasesSettings ReadSettingsFileFromStream(Stream stream)
        {
            var settings = new TestDatabasesSettings();

            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();


                foreach (PropertyInfo p in typeof(TestDatabasesSettings).GetProperties())
                {
                    var match = Regex.Match(result, "^" + p.Name + ":(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    
                    if(match.Success)
                        p.SetValue(settings, match.Groups[1].Value.Trim());

                }
            }
            return settings;
        }

        private SqlConnectionStringBuilder CreateServerPointerInCatalogue(IServerDefaults defaults, string prefix, string databaseName, PermissableDefaults defaultToSet,Assembly creator)
        {
            var opts = new DatabaseCreationProgramOptions()
            {
                ServerName = TestDatabaseSettings.ServerName,
                Prefix = prefix
            };

            var builder = opts.GetBuilder(databaseName);

            if (string.IsNullOrWhiteSpace(databaseName))
                builder.InitialCatalog = "";

            //create a new pointer
            var externalServerPointer = new ExternalDatabaseServer(CatalogueRepository, databaseName??"RAW",creator)
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
            using (var con = repositoryLocator.CatalogueRepository.GetConnection())
            {
                var catalogueDatabaseName = ((TableRepository) repositoryLocator.CatalogueRepository).DiscoveredServer.GetCurrentDatabase().GetRuntimeName();
                var dataExportDatabaseName = ((TableRepository) repositoryLocator.DataExportRepository).DiscoveredServer.GetCurrentDatabase().GetRuntimeName();

                UsefulStuff.ExecuteBatchNonQuery(string.Format(BlitzDatabases, catalogueDatabaseName, dataExportDatabaseName),con.Connection);
            }
        }

        [OneTimeSetUp]
        protected virtual void SetUp()
        {
            //if it is the first time
            if (_startup == null)
            {
                _startup = new Startup(RepositoryLocator);

                _startup.DatabaseFound += StartupOnDatabaseFound;
                _startup.MEFFileDownloaded += StartupOnMEFFileDownloaded;
                _startup.PluginPatcherFound += StartupOnPluginPatcherFound;
                _startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
            }

            RepositoryLocator.CatalogueRepository.MEF.Setup(_startup.MEFSafeDirectoryCatalog);
        }

        [OneTimeTearDown]
        void DropCreatedDatabases()
        {
            foreach (DiscoveredDatabase db in forCleanup)
                if (db.Exists())
                    db.Drop();
        }
        private void StartupOnDatabaseFound(object sender, PlatformDatabaseFoundEventArgs args)
        { 
            //its a healthy message, jolly good
            if (args.Status == RDMPPlatformDatabaseStatus.Healthy)
                return;

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
        
        private void CreateScratchArea()
        {
            var scratchDatabaseName = TestDatabaseNames.GetConsistentName("ScratchArea");

            DiscoveredDatabaseICanCreateRandomTablesIn = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(scratchDatabaseName);

            //if it already exists drop it
            if(DiscoveredDatabaseICanCreateRandomTablesIn.Exists())
                DiscoveredDatabaseICanCreateRandomTablesIn.Drop();

            //create it
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase(scratchDatabaseName);
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

delete from {1}..Project_DataUser
delete from {1}..DataUser

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
        /// <param name="justDropTablesIfPossible">Determines behaviour when the test database already exists.  False to drop and recreate it. True to just drop tables (faster)</param>
        /// <returns></returns>
        protected DiscoveredDatabase GetCleanedServer(DatabaseType type, string dbnName = null, bool justDropTablesIfPossible = false)
        {
            if (dbnName == null)
                dbnName = DiscoveredDatabaseICanCreateRandomTablesIn.GetRuntimeName();

            DiscoveredServer wc1;
            DiscoveredDatabase wc2;
            var toReturn =  GetCleanedServer(type, dbnName, out wc1, out wc2,justDropTablesIfPossible);
            forCleanup.Add(toReturn);
            return toReturn;
        }

        /// <summary>
        /// Gets an empty database on the test server of the appropriate DBMS
        /// </summary>
        /// <param name="type">The DBMS you want a server of (a valid connection string must exist in TestDatabases.txt)</param>
        /// <param name="justDropTablesIfPossible">Determines behaviour when the test database already exists.  False to drop and recreate it. True to just drop tables (faster)</param>
        /// <returns></returns>
        protected DiscoveredDatabase GetCleanedServer(DatabaseType type, bool justDropTablesIfPossible)
        {
            return GetCleanedServer(type, null, justDropTablesIfPossible);
        }

        /// <summary>
        /// Gets an empty database on the test server of the appropriate DBMS
        /// </summary>
        /// <param name="type">The DBMS you want a server of (a valid connection string must exist in TestDatabases.txt)</param>
        /// <param name="dbnName">null for default test database name (recommended unless you are testing moving data from one database to another on the same test server)</param>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="justDropTablesIfPossible">Determines behaviour when the test database already exists.  False to drop and recreate it. True to just drop tables (faster)</param>
        /// <returns></returns>
        protected DiscoveredDatabase GetCleanedServer(DatabaseType type, string dbnName, out DiscoveredServer server, out DiscoveredDatabase database, bool justDropTablesIfPossible = false)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    server = new DiscoveredServer(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder);
                    break;
                case DatabaseType.MySql:
                    server = _discoveredMySqlServer == null ? null : new DiscoveredServer(_discoveredMySqlServer.Builder);
                    break;
                case DatabaseType.Oracle:
                    server = _discoveredOracleServer == null ? null : new DiscoveredServer(_discoveredOracleServer.Builder);
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

            database = server.ExpectDatabase(dbnName);

            if (justDropTablesIfPossible && database.Exists())
            {
                foreach (var t in database.DiscoverTables(true))
                    t.Drop();
                foreach (var t in database.DiscoverTableValuedFunctions())
                    t.Drop();
            }
            else
                database.Create(true);

            server.ChangeDatabase(dbnName);

            Assert.IsTrue(database.Exists());

            return database;
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
                    CatalogueRepository.TableInfoToCredentialsLinker.BreakAllLinksBetween(cred, ti);

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