using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using Diagnostics;
using HIC.Logging;
using HIC.Logging.PastEvents;
using NUnit.Framework;
using RDMPAutomationService;
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
        private AutomationServiceSlot _slot;
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
            //List outstanding exceptions (of which there should be none
            foreach (var ex in CatalogueRepository.GetAllObjects<AutomationServiceException>())
            {
                Console.WriteLine("UNEXPECTED Automation Exception:");
                Console.WriteLine(ex.Exception);

            }

            //there should be no outstanding exceptions in the automation
            Assert.IsFalse(CatalogueRepository.GetAllObjects<AutomationServiceException>().Any());

            //the number of rows before data is automatically loaded (hopefully below)
            _rowsBefore = DataAccessPortal.GetInstance()
                .ExpectDatabase(_stage1_setupCatalogue.DemographyTableInfo, DataAccessContext.InternalDataProcessing)
                .ExpectTable("TestTableForDMP")
                .GetRowCount();

        }

        public void RunAutomationServiceToCompletion(int timeoutInMilliseconds, out int newRows)
        {

            //create a slot for the automation to run in
            _slot = new AutomationServiceSlot(CatalogueRepository);
            _slot.DQEMaxConcurrentJobs = 0;
            _slot.DLEMaxConcurrentJobs = 1;
            _slot.CacheMaxConcurrentJobs = 0;
            _slot.SaveToDatabase();

            //start an automation loop in the slot, it should pickup the load

            var mockOptions = new MockAutomationServiceOptions(RepositoryLocator)
            {
                ServerName = "BLAH",
                ForceSlot = 0
            };

            var loop = new RDMPAutomationLoop(mockOptions, (type, s) => { Console.WriteLine("{0}: {1}", type.ToString().ToUpper(), s); });
            loop.Start();

            //wait 10 seconds for the load to start
            int timeout = timeoutInMilliseconds;

            while ((timeout -= 10) > 0 && !loop.AutomationDestination.OnGoingTasks.Any())
                Thread.Sleep(10);

            if (timeout <= 0)
                throw new Exception("Timed out waiting for AutomationDestination to accept the OnGoingTask");

            Console.WriteLine("Took " + (timeoutInMilliseconds - timeout) + " ms for AutomationDestination to accept the OnGoingTask");

            //task should have just appeared so it shouldn't be stoppable at this time
            Assert.IsFalse(loop.AutomationDestination.CanStop());

            //But we tell it to stop anyway, this should que up the close down operation
            loop.Stop = true;

            //wait for it to complete it's current tasks
            timeout = timeoutInMilliseconds;
            while ((timeout -= 10) > 0)
                Thread.Sleep(10);

            //if (timeout <= 0)
            //    throw new Exception("Timed out waiting for Automation to finish OnGoingTasks");

            //Console.WriteLine("Took " + (timeoutInMilliseconds - timeout) + " ms for Automation to finish OnGoingTasks");

            var exceptions = CatalogueRepository.GetAllObjects<AutomationServiceException>();

            foreach (AutomationServiceException exception in exceptions)
                Console.WriteLine("DATA LOAD EXCEPTION:" + exception);

            //shouldn't have been any exceptions
            Assert.IsFalse(exceptions.Any());

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
            
            int timeout = timeoutInMilliseconds;

            //wait for the job to disapear
            while ((timeout -= 100) > 0 && _slot.AutomationJobs.Length > 0)  //AutomationJobs is a Relationship property so refreshes from database each time it is interrogated
                Thread.Sleep(100);

            string errorJobs = "";
            //Should be no jobs running but if there are any then tell user about them before throwing
            foreach (var job in _slot.AutomationJobs)
                errorJobs += job.ID + " " + job.Description + " " + job.LastKnownStatus;

            //should be no jobs running
            Assert.AreEqual(0, _slot.AutomationJobs.Length, "The following unexpected jobs were found:" + Environment.NewLine + errorJobs);

            _slot.DeleteInDatabase();
            
            _stage1_setupCatalogue.DestroyEnvironment();
            _testFolder.Delete(true);
        }
    }
}