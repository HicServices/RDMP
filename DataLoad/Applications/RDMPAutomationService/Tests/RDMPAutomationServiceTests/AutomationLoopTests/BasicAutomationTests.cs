using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using RDMPAutomationService;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class BasicAutomationTests : AutomationTests
    {
        [Test]
        public void TestsNoAvailableSlots_ErrorLoggedForService()
        {
            var loop = new RDMPAutomationLoop(RepositoryLocator, null, logAction);
            loop.Start();

            int timeout = 0;
            while (loop.StillRunning)
            {
                if ((timeout += 1000) > 10000)
                    Assert.Fail("Timed out waiting for automation service to stop running");

                Thread.Sleep(1000);
            }

            var error = CatalogueRepository.GetAllObjects<AutomationServiceException>().Single();

            Assert.IsTrue(error.Exception.StartsWith("Cannot start automation service because there are no free AutomationServiceSlots, they must all be locked?"));

            error.DeleteInDatabase();
        }


        [Test]
        public void TestLifelineTicks()
        {
            bool startupComplete = false;
            var slot = new AutomationServiceSlot(CatalogueRepository);

            Assert.IsFalse(slot.LockedBecauseRunning);

            var loop = new RDMPAutomationLoop(mockOptions, logAction);
            loop.StartCompleted += (sender, args) => { startupComplete = true; };
            loop.Start();

            int timeout = 30000;

            while (timeout > 0)
            {
                Thread.Sleep(100);
                timeout -= 100;
                slot.RevertToDatabaseState();

                //wait till it has started 
                if (loop.StillRunning && slot.LockedBecauseRunning && startupComplete)
                {
                    //check it's lifeline is ticking
                    Assert.IsTrue(slot.Lifeline.HasValue);
                    
                    break;
                }
            }

            if (timeout <= 0)
                throw new TimeoutException("Slot did not get locked and start up properly before the timeout had expired");

            //30 seconds for there to be at least 2 lifeline ticks
            timeout = 30000;
            List<DateTime> timesSeen = new List<DateTime>();

            while (timeout > 0)
            {
                Thread.Sleep(100);
                timeout -= 100;

                slot.RefreshLifelinePropertyFromDatabase();
                
                Assert.NotNull(slot.Lifeline);

                Assert.IsTrue(loop.StillRunning);
                Assert.IsFalse(loop.Stop);

                if(!timesSeen.Contains(slot.Lifeline.Value))
                {

                    timesSeen.Add(slot.Lifeline.Value);
                    Console.WriteLine("Saw Tick Time:" + slot.Lifeline.Value);
                }

                if (timesSeen.Count > 1)
                    break;
            }


            if (timeout <= 0)
                throw new TimeoutException("Did not see two unique lifeline values before the timeout had expired");
            
            timeout = 30000;
            
            //refresh it's state 
            loop.Stop = true; //and tell it to stop
            while (timeout > 0)
            {
                Thread.Sleep(100);
                timeout -= 100;
                slot.RevertToDatabaseState();
                
                //shouldn't be locked anymore
                if (!slot.LockedBecauseRunning)
                {
                    Assert.IsFalse(loop.StillRunning);
                    break;
                }
            }
           
            if (timeout <= 0)
                throw new TimeoutException("Slot did not shut down properly before the timeout had expired");
           
            
            //shouldn't be any errors either
            var errors = CatalogueRepository.GetAllObjects<AutomationServiceException>().ToArray();
            Assert.IsFalse(errors.Any(),string.Join(Environment.NewLine, errors.Select(e=>e.ToString())));
            
            slot.DeleteInDatabase();
        }
    }
}
