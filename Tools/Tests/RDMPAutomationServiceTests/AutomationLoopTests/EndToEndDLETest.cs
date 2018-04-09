using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Checks;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndDLETest : AutomationTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void RunEndToEndDLETest(bool simulateLoadNotRequired)
        {
            const int timeoutInMilliseconds = 120000;

            var setup = new DLEEndToEndTestSetup(
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn, 
                UnitTestLoggingConnectionString,
                RepositoryLocator,
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn);

            LoadMetadata lmd;
            setup.SetUp(timeoutInMilliseconds,out lmd);

            var loadPeriodically = new LoadPeriodically(CatalogueRepository, lmd, 10);//create a periodic load

            //which should be due
            Assert.IsTrue(loadPeriodically.IsLoadDue(null));


            if (simulateLoadNotRequired)
            {
                string locationOfForLoadingDirectory = Path.Combine(lmd.LocationOfFlatFiles, @"Data\ForLoading");
                var flatFile = Directory.GetFiles(locationOfForLoadingDirectory).Single();
                File.Delete(flatFile);
            }

            setup.RecordPreExecutionState();

            int newRows;
            setup.RunAutomationServiceToCompletion(timeoutInMilliseconds,out newRows);
            
            if(simulateLoadNotRequired)
                Assert.AreEqual(0, newRows);//if load was not required (file not found then the same number of rows should be there as at the start)
            else
                Assert.AreEqual(10, newRows);

            //Check the load periodically isn't due now
            loadPeriodically.RevertToDatabaseState();
            Assert.IsFalse(loadPeriodically.IsLoadDue(null));

            loadPeriodically.DeleteInDatabase();

            setup.VerifyNoErrorsAfterExecutionThenCleanup(timeoutInMilliseconds);
        }
    }
}
