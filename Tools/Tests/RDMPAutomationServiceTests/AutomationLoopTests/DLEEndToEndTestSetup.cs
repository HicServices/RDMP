using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;

using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using Diagnostics;
using HIC.Logging;
using HIC.Logging.PastEvents;
using NUnit.Framework;
using RDMPAutomationService;
using RDMPAutomationService.Logic.DLE;
using RDMPAutomationService.Options;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class DLEEndToEndTestSetup
    {
        public DiscoveredServer ServerICanCreateRandomDatabasesAndTablesOn { get; set; }
        public SqlConnectionStringBuilder UnitTestLoggingConnectionString { get; set; }
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
        public DiscoveredServer DiscoveredServerICanCreateRandomDatabasesAndTablesOn { get; set; }
        public CatalogueRepository CatalogueRepository { get; set; }

        public DLEEndToEndTestSetup(DiscoveredServer serverICanCreateRandomDatabasesAndTablesOn, SqlConnectionStringBuilder unitTestLoggingConnectionString, IRDMPPlatformRepositoryServiceLocator repositoryLocator, DiscoveredServer discoveredServerICanCreateRandomDatabasesAndTablesOn)
        {
            ServerICanCreateRandomDatabasesAndTablesOn = serverICanCreateRandomDatabasesAndTablesOn;
            UnitTestLoggingConnectionString = unitTestLoggingConnectionString;
            RepositoryLocator = repositoryLocator;
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn = discoveredServerICanCreateRandomDatabasesAndTablesOn;
            CatalogueRepository = repositoryLocator.CatalogueRepository;
        }

        private UserAcceptanceTestEnvironment _stage1_setupCatalogue;
        private int _rowsBefore;
        private Catalogue _testCatalogue;
        private DirectoryInfo _testFolder;

        public void SetUp(int timeoutInMilliseconds,out LoadMetadata lmd)
        {
            var rootFolder = new DirectoryInfo(".");
            _testFolder = rootFolder.CreateSubdirectory("TestTheTestDatasetSetup");
            var datasetFolder = _testFolder.CreateSubdirectory("TestDataset");
            
            _stage1_setupCatalogue = new UserAcceptanceTestEnvironment((SqlConnectionStringBuilder) ServerICanCreateRandomDatabasesAndTablesOn.Builder, datasetFolder.FullName, UnitTestLoggingConnectionString, "Internal", null, null, RepositoryLocator);
            _stage1_setupCatalogue.SilentRunning = true;

            //create it all
            _stage1_setupCatalogue.Check(new AcceptAllCheckNotifier());

            //what did we create?
            _testCatalogue = _stage1_setupCatalogue.DemographyCatalogue;
            lmd = _stage1_setupCatalogue.DemographyCatalogue.LoadMetadata;

        }

        public void RecordPreExecutionState()
        {
            //the number of rows before data is automatically loaded (hopefully below)
            _rowsBefore = DataAccessPortal.GetInstance()
                .ExpectDatabase(_stage1_setupCatalogue.DemographyTableInfo, DataAccessContext.InternalDataProcessing)
                .ExpectTable("TestTableForDMP")
                .GetRowCount();

        }

        public void RunAutomationServiceToCompletion(int timeoutInMilliseconds, out int newRows)
        {
            
            //start an automation loop in the slot, it should pickup the load
            var auto = new AutomatedDLELoad(new DleOptions() { LoadMetadata = _stage1_setupCatalogue.DemographyCatalogue.LoadMetadata.ID});
            auto.RunTask(RepositoryLocator);

            //also shouldn't be any logged errors
            var lm = new LogManager(_testCatalogue.LiveLoggingServer);

            var log = lm.GetLoadStatusOf(PastEventType.MostRecent, _testCatalogue.LoggingDataTask);

            if(log == null)
                throw new Exception("No log messages found for logging task " + _testCatalogue.LoggingDataTask);

            Assert.AreEqual(0, log.Errors.Count);

            //number after
            var rowsAfter = DataAccessPortal.GetInstance()
                .ExpectDatabase(_stage1_setupCatalogue.DemographyTableInfo, DataAccessContext.InternalDataProcessing)
                .ExpectTable("TestTableForDMP")
                .GetRowCount();

            newRows = rowsAfter - _rowsBefore;
        }

        public void VerifyNoErrorsAfterExecutionThenCleanup(int timeoutInMilliseconds)
        {
            //RAW should have been cleaned up
            Assert.IsFalse(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("DMP_Test_RAW").Exists());
            
            _stage1_setupCatalogue.DestroyEnvironment();
            _testFolder.Delete(true);
        }
    }
}