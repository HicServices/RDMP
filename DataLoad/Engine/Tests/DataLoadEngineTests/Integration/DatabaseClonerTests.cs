using System;
using System.Collections;
using System.Data.SqlClient;
using System.Linq;
using AnonymisationTests;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibraryTests;
using DataExportLibrary.Repositories;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class DatabaseClonerTests : TestsRequiringFullAnonymisationSuite
    {
        private Exception _setupException;

        private UserAcceptanceTestEnvironmentHelper _userAcceptanceTestEnvironmentHelper;

        [TestFixtureSetUp]
        protected void CallUserAcceptanceTestEnvironmentHelper_SetUp()
        {
            try
            {
                _userAcceptanceTestEnvironmentHelper = new UserAcceptanceTestEnvironmentHelper(
                    DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString,
                    CatalogueRepository.ConnectionString,
                    UnitTestLoggingConnectionString.ConnectionString, 
                    ANOStore_Database.Server.Builder.ConnectionString, 
                    IdentifierDump_Database.Server.Builder.ConnectionString,
                    RepositoryLocator);

                _userAcceptanceTestEnvironmentHelper.SetUp();
            }
            catch (Exception e)
            {
                _setupException = e;
            }
        }

        [TestFixtureTearDown]
        protected void AfterAllDatabaseClonerTests()
        {
            _userAcceptanceTestEnvironmentHelper.TearDown();
        }

        [SetUp]
        public void BeforeEachTest()
        {
            if (_setupException != null)
            {
                Console.WriteLine(_setupException.Message);
                throw _setupException;
            }
        }

        [Test]
        public void TestRawDatabaseCreationWithoutANOConfiguration()
        {
            var testEnvironment = _userAcceptanceTestEnvironmentHelper.TestEnvironment;
            var databaseNamingScheme = new FixedStagingDatabaseNamer("fish");
            var hicDatabaseConfig = new HICDatabaseConfiguration(testEnvironment.DemographyCatalogue.LoadMetadata);
            
            var rawDbInfo = hicDatabaseConfig.DeployInfo[LoadBubble.Raw];
            
            if (rawDbInfo.Exists())
            {
                foreach (var t in rawDbInfo.DiscoverTables(true))
                    t.Drop();

                rawDbInfo.Drop();
            }
            
            var cloner = new DatabaseCloner(hicDatabaseConfig);
            cloner.CreateDatabaseForStage(LoadBubble.Raw);
            cloner.CreateTablesInDatabaseFromCatalogueInfo(testEnvironment.DemographyTableInfo, LoadBubble.Raw);
            
            var table = hicDatabaseConfig.DeployInfo.DatabaseInfoList[LoadBubble.Raw].ExpectTable(testEnvironment.DemographyTableInfo.GetRuntimeName());
            Assert.IsTrue(table.Exists());

            // ensure that the RAW tables doesn't have any hic_ fields in it
            Assert.IsFalse(table.DiscoverColumns().Any(c => c.GetRuntimeName().StartsWith("hic_")));

            cloner.LoadCompletedSoDispose(ExitCodeType.Success, null);
        }


        [Test]
        public void TestRawDatabaseCreationWithANOConfiguration()
        {
            var testEnvironment = _userAcceptanceTestEnvironmentHelper.TestEnvironment;
            var hicDatabaseConfig = new HICDatabaseConfiguration(testEnvironment.DemographyCatalogue.LoadMetadata);

            var rawDbInfo = hicDatabaseConfig.DeployInfo[LoadBubble.Raw];

            if (rawDbInfo.Exists())
            {
                foreach (DiscoveredTable t in rawDbInfo.DiscoverTables(true))
                    t.Drop();

                rawDbInfo.Drop();
            }

            var cloner = new DatabaseCloner(hicDatabaseConfig);
            cloner.CreateDatabaseForStage(LoadBubble.Raw);
            cloner.CreateTablesInDatabaseFromCatalogueInfo(testEnvironment.DemographyTableInfo, LoadBubble.Raw);

            // The data type of the ANOCHI column in the LIVE database should be varchar(12), but in the RAW database it should be non-ANO and varchar(10)

            // Check column in 'live' database is as expected
            var liveDbInfo = hicDatabaseConfig.DeployInfo[LoadBubble.Live];
            var liveTable = liveDbInfo.ExpectTable(testEnvironment.DemographyTableInfo.GetRuntimeName());

            Assert.IsTrue(liveTable.Exists());
            
            var anoCHIColumn = liveTable.DiscoverColumn("ANOCHI");
            Assert.IsTrue(anoCHIColumn.DataType.SQLType.Equals("varchar(12)"));

            // now check the RAW column is the 'non-ANO' version
            var rawTable = rawDbInfo.ExpectTable(testEnvironment.DemographyTableInfo.GetRuntimeName());
            
            Assert.IsTrue(rawTable.Exists());

            var columns = rawTable.DiscoverColumns();
            var chiCol = columns.SingleOrDefault(c => c.GetRuntimeName().Equals("CHI"));

            Assert.IsNotNull(chiCol, "CHI column not found in RAW database");
            Assert.AreEqual("varchar(10)", chiCol.DataType.SQLType);
            
            cloner.LoadCompletedSoDispose(ExitCodeType.Success, null);
        }
    }
}
