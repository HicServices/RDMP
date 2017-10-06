using System.Linq;
using System.Threading;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using DataQualityEngine.Data;
using Diagnostics.TestData;
using NUnit.Framework;
using RDMPAutomationService;
using ReusableLibraryCode;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndDQETest : AutomationTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestEndToEndDQE(bool cancelEarly)
        {
            BulkTestsData bulkTests = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn, cancelEarly ? 100000 : 100);
            bulkTests.SetupTestData();
            bulkTests.ImportAsCatalogue();

            AutomationServiceSlot slot = new AutomationServiceSlot(CatalogueRepository);
            var loop = new RDMPAutomationLoop(RepositoryLocator, slot, logAction);

            slot.DLEMaxConcurrentJobs = 0;
            slot.DQEMaxConcurrentJobs = 1;
            slot.SaveToDatabase();

            //start the loop
            Assert.IsTrue(slot.IsAcceptingNewJobs(AutomationJobType.DQE));
            loop.Start();

            //give it time to get itself together
            Thread.Sleep(1000);
            Assert.IsTrue(slot.IsAcceptingNewJobs(AutomationJobType.DQE));//there shouldn't be any dqe runs executing just yet because theres no validation results
            Assert.IsTrue(loop.StillRunning);

            var dqeRepository = new DQERepository(CatalogueRepository);

            //no evaluations yet
            Assert.IsFalse(dqeRepository.GetAllEvaluationsFor(bulkTests.catalogue).Any());
            bulkTests.SetupValidationOnCatalogue();

            //wait a few seconds for it to detect it and run the DQE
            int timeout = 30000;
            bool cancelledsuccessfully = false;
            bool taskAppeared = false;


            while ((timeout -= 10) > 0)
            {
                Thread.Sleep(10);

                if (loop.AutomationDestination == null)
                    continue;

                var task = loop.AutomationDestination.OnGoingTasks.SingleOrDefault();

                if (task == null)
                {
                    if (taskAppeared)//it vanished again - excellent it has probably completed successfully
                        break;
                }
                else
                {
                    taskAppeared = true;

                    //it has finished so stop watching the looper
                    if (task.Job.LastKnownStatus == AutomationJobStatus.Finished)
                        break;

                    //it has successfully cancelled itself so we can stop watching the loop
                    if (task.Job.LastKnownStatus == AutomationJobStatus.Cancelled)
                        break;

                    if (cancelEarly && !cancelledsuccessfully)//job appeared quick cancel it!
                    {
                        //simulates the user clicking Cancel in the Dashboard
                        using (var con = ((CatalogueRepository)task.Repository).GetConnection())
                            Assert.AreEqual(1,
                                DatabaseCommandHelper.GetCommand(
                                    "UPDATE AutomationJob SET CancelRequested = 1 WHERE ID = " + task.Job.ID,
                                    con.Connection).ExecuteNonQuery());
                        cancelledsuccessfully = true;
                    }
                }

            }

            if (cancelEarly)
                if (timeout <= 0 && !cancelledsuccessfully)
                    Assert.Fail(
                        "ActivityManager did not manage to cancel the task in the window of 30 seconds of validation being setup on the test Catalogue");
                else
                {
                    var job = loop.AutomationDestination.OnGoingTasks.Single().Job;
                    Assert.IsTrue(job.LastKnownStatus == AutomationJobStatus.Cancelled);

                    Assert.IsNotNull(job.LoggingServer_ID);
                    Assert.IsNotNull(job.DataLoadRunID);
                }


            //catalogue is now compatible with DQE and has never been run! so automation should pick it up

            //Automation should have finished by now
            if (!cancelEarly)
                Assert.IsTrue(dqeRepository.GetAllEvaluationsFor(bulkTests.catalogue).Any());
            else
                Assert.IsFalse(dqeRepository.GetAllEvaluationsFor(bulkTests.catalogue).Any()); //cancel was issued so there shouldn't have been any runs

            //tell it to stop
            loop.Stop = true;

            Thread.Sleep(3000);
            Assert.IsFalse(loop.StillRunning);//it should have ended its main loop regardless of cancellation

            //cleanup
            if (!cancelEarly)
                dqeRepository.GetAllEvaluationsFor(bulkTests.catalogue).Single().DeleteInDatabase();

            if (cancelEarly)
                loop.AutomationDestination.OnGoingTasks.Single().Job.DeleteInDatabase();

            bulkTests.DeleteCatalogue();


            Assert.AreEqual(0,slot.AutomationJobs.Length);

            slot.DeleteInDatabase();
        }
    }
}
