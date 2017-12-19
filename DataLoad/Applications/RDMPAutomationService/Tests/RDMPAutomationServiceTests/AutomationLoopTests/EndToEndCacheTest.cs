using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CachingEngine;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngineTests.Integration;
using DataLoadEngineTests.Integration.Cache;
using DataLoadEngineTests.Integration.PipelineTests;
using NUnit.Framework;
using RDMPAutomationService;
using RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndCacheTest : AutomationTests
    {

        private Catalogue _cata;
        private AutomationServiceSlot _slot;
        private LoadMetadata _lmd;
        private LoadProgress _lp;
        private CacheProgress _cp;

        private TestDataPipelineAssembler _testPipeline;
        private HICProjectDirectory _hicProjectDirectory;

        const int NumDaysToCache = 5;

        [SetUp]
        public void SetupDatabaseObjects()
        {
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

            var testDir = new DirectoryInfo(@".\EndToEndCacheTest");
            if (!testDir.Exists)
                testDir.Create();

            _slot = new AutomationServiceSlot(CatalogueRepository);
        
            _lmd = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch o' coconuts");
            _hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(testDir, true);
            _lmd.LocationOfFlatFiles = _hicProjectDirectory.RootPath.FullName;
            _lmd.SaveToDatabase();

            _cata = new Catalogue(CatalogueRepository, "EndToEndCacheTest");
            _cata.LoadMetadata_ID = _lmd.ID;
            _cata.SaveToDatabase();
        
            _lp = new LoadProgress(CatalogueRepository, _lmd);
            _cp = new CacheProgress(CatalogueRepository, _lp); 
            
            _lp.OriginDate = new DateTime(2001,1,1);
            _lp.SaveToDatabase();

            _testPipeline = new TestDataPipelineAssembler("EndToEndCacheTestPipeline",CatalogueRepository);
            _testPipeline.ConfigureCacheProgressToUseThePipeline(_cp);

            _cp.CacheFillProgress = DateTime.Now.AddDays(-NumDaysToCache);
            _cp.SaveToDatabase();

            _cp.SaveToDatabase();
        }


        [Test]
        public void FireItUpManually()
        {
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

            var cachingHost = new CachingHost(CatalogueRepository);
            var cpAsList = new ICacheProgress[] {_cp}.ToList();

            cachingHost.CacheProgressList = cpAsList;
            cachingHost.Start(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            // should be numDaysToCache days in cache
            Assert.AreEqual(NumDaysToCache, _hicProjectDirectory.Cache.GetFiles("*.csv").Count());

            // make sure each file is named as expected
            var cacheFiles = _hicProjectDirectory.Cache.GetFiles().Select(fi => fi.Name).ToArray();
            for (var i = -NumDaysToCache; i < 0; i++)
            {
                var filename = DateTime.Now.AddDays(i).ToString("yyyyMMdd") + ".csv"; 
                Assert.IsTrue(cacheFiles.Contains(filename), filename + " not found");
            }
        }

        [Test]
        [Timeout(60000)]
        public void RunEndToEndCacheTest()
        {
            _slot.CacheMaxConcurrentJobs = 2;//set the max to 2 which will trip up if there is dodgy locking logic that would allow 2 copies of the same task (shouldn't be allowed)
            _slot.DLEMaxConcurrentJobs = 0;
            _slot.DQEMaxConcurrentJobs = 0;
            _slot.SaveToDatabase();

            Assert.AreEqual(0, _hicProjectDirectory.Cache.GetFiles("*.csv").Count());

            var loop = new RDMPAutomationLoop(mockOptions, logAction);
            loop.Start();

            while (_hicProjectDirectory.Cache.GetFiles("*.csv").Count() < 5)
            {
                Thread.Sleep(1000);
            }

            int timeout = 10000;
            while (_slot.AutomationJobs.Length != 0 && timeout>0)
            {
                timeout -= 100;
                Thread.Sleep(100);
            }

            Assert.AreEqual(5, _hicProjectDirectory.Cache.GetFiles("*.csv").Count());
            Assert.AreEqual(0,_slot.AutomationJobs.Length);
            
            foreach (var ex in CatalogueRepository.GetAllObjects<AutomationServiceException>())
                Console.WriteLine("AutomationServiceException: " + ex.Exception);

            //Give the Thread time to complete even though the job length is zero the thread might be in race condition to complete with this check.
            while (!loop.AutomationDestination.CanStop() && timeout>0)
            {
                timeout -= 100;
                Thread.Sleep(100);
            }
            
            Assert.IsTrue(loop.AutomationDestination.CanStop());
            
            loop.Stop = true;
            
            Task.Delay(3000).Wait();
            Assert.IsFalse(loop.StillRunning);
        }

        [TearDown]
        public void DeleteDatabaseObjects()
        {
            _slot.Unlock();
            _slot.DeleteInDatabase();

            _cata.DeleteInDatabase();
            
            _testPipeline.Destroy();

            if (_cp != null)
                _cp.DeleteInDatabase();

            _lp.DeleteInDatabase();

            _lmd.DeleteInDatabase();
            
            _hicProjectDirectory.RootPath.Delete(true);
        }

    }
}
