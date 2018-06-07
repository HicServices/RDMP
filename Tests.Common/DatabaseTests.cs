using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ANOStore.ANOEngineering;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CatalogueLibrary.Repositories;
using DatabaseCreation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using DataQualityEngine.Data;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using RDMPStartup;
using RDMPStartup.Events;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Rhino.Mocks;

namespace Tests.Common
{
    [Category("Database")]
    public class DatabaseTests
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        protected static string _serverName;
        private static string _mySqlServer;
        private static string _oracleServer;

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

        protected DiscoveredServer DiscoveredMySqlServer;
        protected DiscoveredServer DiscoveredOracleServer;

        static private Startup _startup;

        static DatabaseTests()
        {
            if (CatalogueRepository.SuppressHelpLoading == null)
                CatalogueRepository.SuppressHelpLoading = true;
            
            ReadSettingsFile(out _serverName, out TestDatabaseNames.Prefix,out _mySqlServer, out _oracleServer);
        }

        private static void ReadSettingsFile(out string serverName,out string prefix, out string mysql, out string oracle)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Tests.Common.TestDatabases.txt";

            //see if there is a local text file first
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var f = dir.GetFiles("TestDatabases.txt").SingleOrDefault();
            
            //there is a local text file so favour that one
            if (f != null)
            {
                ReadSettingsFileFromStream(f.OpenRead(),out serverName,out prefix, out mysql, out oracle);
                return;
            }

            //otherwise use the embedded resource file
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                ReadSettingsFileFromStream(stream,out serverName,out prefix, out mysql,out oracle);
            
        }

        public DatabaseTests()
        {
            RepositoryLocator = new DatabaseCreationRepositoryFinder(_serverName, TestDatabaseNames.Prefix);

            Console.WriteLine("Expecting Unit Test Catalogue To Be At:"+((TableRepository)RepositoryLocator.CatalogueRepository).DiscoveredServer.DescribeServer());
            Assert.IsTrue(((TableRepository)RepositoryLocator.CatalogueRepository).DiscoveredServer.Exists(), "Catalogue database does not exist, run DatabaseCreation.exe to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to CreateDatabases.exe e.g. 'DatabaseCreation.exe localhost\\sqlexpress TEST_')");
            Console.WriteLine("Found Catalogue!");

            Console.WriteLine("Expecting Unit Test Data Export database To Be At:" + ((TableRepository)RepositoryLocator.DataExportRepository).DiscoveredServer.DescribeServer());
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Assert.IsTrue(((TableRepository)RepositoryLocator.DataExportRepository).DiscoveredServer.Exists(), "Data Export database does not exist, run DatabaseCreation.exe to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to CreateDatabases.exe e.g. 'DatabaseCreation.exe localhost\\sqlexpress TEST_')");
            Console.WriteLine("Found DataExport!");

            RunBlitzDatabases(RepositoryLocator);

            var defaults = new ServerDefaults(CatalogueRepository);

            DataQualityEngineConnectionString = CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, DatabaseCreationProgram.DefaultDQEDatabaseName, ServerDefaults.PermissableDefaults.DQE,typeof(DataQualityEngine.Database.Class1).Assembly);
            UnitTestLoggingConnectionString = CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, DatabaseCreationProgram.DefaultLoggingDatabaseName, ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, typeof(HIC.Logging.Database.Class1).Assembly);
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn = new DiscoveredServer(CreateServerPointerInCatalogue(defaults, TestDatabaseNames.Prefix, null, ServerDefaults.PermissableDefaults.RAWDataLoadServer, null));

            CreateScratchArea();

            if (_mySqlServer != null)
                DiscoveredMySqlServer = new DiscoveredServer(new MySqlConnectionStringBuilder(_mySqlServer));

            if(_oracleServer != null)
                DiscoveredOracleServer = new DiscoveredServer(new OracleConnectionStringBuilder(_oracleServer));
        }

        

        private static void ReadSettingsFileFromStream(Stream stream, out string serverName, out string prefix, out string mySql, out string oracle)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();

                serverName = Regex.Match(result, "^ServerName:(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                prefix = Regex.Match(result, "^Prefix:(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase).Groups[1].Value.Trim();

                var mysqlMatch = Regex.Match(result, "^MySql:(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                mySql = mysqlMatch.Success? mysqlMatch.Groups[1].Value.Trim():null;


                var oracleMatch = Regex.Match(result, "^Oracle:(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                oracle = oracleMatch.Success ? oracleMatch.Groups[1].Value.Trim() : null;
            }
        }

        private SqlConnectionStringBuilder CreateServerPointerInCatalogue(ServerDefaults defaults, string prefix, string databaseName, ServerDefaults.PermissableDefaults defaultToSet,Assembly creator)
        {
            var builder = DatabaseCreationProgram.GetBuilder(_serverName, prefix, databaseName);

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

        [TestFixtureSetUp]
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

        [TestFixtureTearDown]
        void DropCreatedDatabases()
        {
            foreach (DiscoveredDatabase db in forCleanup)
                if (db.Exists())
                    db.ForceDrop();
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
                DiscoveredDatabaseICanCreateRandomTablesIn.ForceDrop();

            //create it
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase(scratchDatabaseName);
        }
        
        public const string BlitzDatabases = @"
--If you want to blitz everything out of your test catalogue and data export database(s) then run the following SQL (adjusting for database names):

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

delete from {0}..AutomationLockedCatalogues
delete from {0}..AutomationJob
delete from {0}..AutomateablePipeline
delete from {0}..AutomationServiceSlot
delete from {0}..AutomationServiceException

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

        protected DiscoveredDatabase GetCleanedServer(DatabaseType type, string dbnName)
        {
            DiscoveredServer wc1;
            DiscoveredDatabase wc2;
            var toReturn =  GetCleanedServer(type, dbnName, out wc1, out wc2);
            forCleanup.Add(toReturn);
            return toReturn;
        }

        protected DiscoveredDatabase GetCleanedServer(DatabaseType type,string dbnName, out DiscoveredServer server, out DiscoveredDatabase database)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
                    break;
                case DatabaseType.MYSQLServer:
                    server = DiscoveredMySqlServer;
                    break;
                case DatabaseType.Oracle:
                    server = DiscoveredOracleServer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            if (server == null)
                Assert.Inconclusive();

            if (!server.Exists())
                Assert.Inconclusive();

            server.TestConnection();

            database = server.ExpectDatabase(dbnName);

            if (database.Exists())
            {
                foreach (DiscoveredTable discoveredTable in database.DiscoverTables(false))
                    discoveredTable.Drop();

                database.Drop();
                Assert.IsFalse(database.Exists());
            }

            server.CreateDatabase(dbnName);

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