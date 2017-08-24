using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Automation;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Automation
{
    public class AutomationServiceSlotTests:DatabaseTests
    {
        private AutomationServiceSlot _instance;

        [SetUp]
        public void CreateInstance()
        {
            _instance = new AutomationServiceSlot(CatalogueRepository, AutomationFailureStrategy.TryNext,
                AutomationFailureStrategy.TryNext, AutomationFailureStrategy.TryNext,AutomationDQEJobSelectionStrategy.MostRecentlyLoadedDataset);
        }

        [TearDown]
        public void DeleteInstance()
        {
            //its already deleted
            if (_instance.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted)
                return;

            //unlock it if its locked
            if (_instance.LockedBecauseRunning)
                _instance.Unlock();

            _instance.DeleteInDatabase();
        }

        [Test]
        public void TestChangingAndSavingProperties()
        {
            _instance.DQEFailureStrategy = AutomationFailureStrategy.Stop;
            var diff = _instance.HasLocalChanges();
            
            Assert.AreEqual(1,diff.Differences.Count());
            _instance.SaveToDatabase();
            Assert.AreEqual(0, _instance.HasLocalChanges().Differences.Count());
            
        }

        [Test]
        public void CannotDeleteIfLocked()
        {
            _instance.Lock();
            var ex = Assert.Throws<NotSupportedException>(_instance.DeleteInDatabase);

            Assert.IsTrue(ex.Message.Contains("because it is locked by " + _instance.LockHeldBy));
            _instance.Unlock();
            _instance.DeleteInDatabase();
        }

        [Test]
        [TestCase(AutomationJobType.DLE)]
        [TestCase(AutomationJobType.DQE)]
        [TestCase(AutomationJobType.Cache)]
        public void TestIsAcceptingJobs(AutomationJobType toTest)
        {
            //max jobs starts at 0
            Assert.IsFalse(_instance.IsAcceptingNewJobs(AutomationJobType.DLE));
            Assert.IsFalse(_instance.IsAcceptingNewJobs(AutomationJobType.DQE));
            Assert.IsFalse(_instance.IsAcceptingNewJobs(AutomationJobType.Cache));

            //set the max slots to 1 
            switch (toTest)
            {
                case AutomationJobType.DQE:
                    _instance.DQEMaxConcurrentJobs = 1;
                    break;
                case AutomationJobType.DLE:
                    _instance.DLEMaxConcurrentJobs = 1;
                    break;
                case AutomationJobType.Cache:
                    _instance.CacheMaxConcurrentJobs = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("toTest");
            }
            

            //now accepting jobs but ONLY for the one we are testing
            Assert.IsTrue(_instance.IsAcceptingNewJobs(AutomationJobType.DLE) == (toTest == AutomationJobType.DLE));
            Assert.IsTrue(_instance.IsAcceptingNewJobs(AutomationJobType.DQE) == (toTest == AutomationJobType.DQE));
            Assert.IsTrue(_instance.IsAcceptingNewJobs(AutomationJobType.Cache) == (toTest == AutomationJobType.Cache));

            //new job which takes up slot
            var newDleJob =_instance.AddNewJob(toTest,"doing stuff");

            //so no longer any slots for anything
            Assert.IsFalse(_instance.IsAcceptingNewJobs(AutomationJobType.DLE));
            Assert.IsFalse(_instance.IsAcceptingNewJobs(AutomationJobType.DQE));
            Assert.IsFalse(_instance.IsAcceptingNewJobs(AutomationJobType.Cache));

            //Simulate job finishing
            newDleJob.DeleteInDatabase();

            //now accepting jobs but ONLY for the one we are testing again
            Assert.IsTrue(_instance.IsAcceptingNewJobs(AutomationJobType.DLE) == (toTest == AutomationJobType.DLE));
            Assert.IsTrue(_instance.IsAcceptingNewJobs(AutomationJobType.DQE) == (toTest == AutomationJobType.DQE));
            Assert.IsTrue(_instance.IsAcceptingNewJobs(AutomationJobType.Cache) == (toTest == AutomationJobType.Cache));
        }


        [Test]
        [TestCase(AutomationJobType.DLE,AutomationFailureStrategy.Stop)]
        [TestCase(AutomationJobType.DLE, AutomationFailureStrategy.TryNext)]
        [TestCase(AutomationJobType.DQE,AutomationFailureStrategy.Stop)]
        [TestCase(AutomationJobType.DQE, AutomationFailureStrategy.TryNext)]
        [TestCase(AutomationJobType.Cache, AutomationFailureStrategy.Stop)]
        [TestCase(AutomationJobType.Cache, AutomationFailureStrategy.TryNext)]
        public void TestIsAcceptingJobs_Strategy(AutomationJobType type,AutomationFailureStrategy failureStrategy)
        {
            //max jobs starts at 0
            Assert.IsFalse(_instance.IsAcceptingNewJobs(type));
            
            //set the max slots to 1 
            switch (type)
            {
                case AutomationJobType.DQE:
                    _instance.DQEMaxConcurrentJobs = 2;
                    _instance.DQEFailureStrategy = failureStrategy;
                    break;
                case AutomationJobType.DLE:
                    _instance.DLEMaxConcurrentJobs = 2;
                    _instance.DLEFailureStrategy = failureStrategy;
                    break;
                case AutomationJobType.Cache:
                    _instance.CacheMaxConcurrentJobs = 2;
                    _instance.CacheFailureStrategy = failureStrategy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            
            _instance.SaveToDatabase();

            Assert.IsTrue(_instance.IsAcceptingNewJobs(type));
            AutomationJob job = null; 

            try
            {
                
                job = _instance.AddNewJob(type, "FailingTask");
                job.LastKnownStatus = AutomationJobStatus.Running;
                job.SaveToDatabase();

                Assert.IsTrue(_instance.IsAcceptingNewJobs(type));

                job.SetLastKnownStatus(AutomationJobStatus.Crashed);

                if (failureStrategy == AutomationFailureStrategy.Stop)
                    Assert.IsFalse(_instance.IsAcceptingNewJobs(type));
                else
                    Assert.IsTrue(_instance.IsAcceptingNewJobs(type));

            }
            finally 
            {
                if(job != null)
                    job.DeleteInDatabase();
            }
        }
    }
}
