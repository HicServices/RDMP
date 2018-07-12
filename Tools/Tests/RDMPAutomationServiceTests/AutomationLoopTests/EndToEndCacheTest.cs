using System;
using System.IO;
using System.Linq;
using CachingEngine;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngineTests.Integration;
using DataLoadEngineTests.Integration.Cache;
using DataLoadEngineTests.Integration.PipelineTests;
using NUnit.Framework;
using RDMPAutomationService.Options;
using RDMPAutomationService.Runners;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndCacheTest : DatabaseTests
    {

        private Catalogue _cata;
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
            Assert.AreEqual(0, _hicProjectDirectory.Cache.GetFiles("*.csv").Count());

            var auto = new CacheRunner(new CacheOptions(){  CacheProgress= _cp.ID, Command = CommandLineActivity.run });
            auto.Run(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(), new ThrowImmediatelyCheckNotifier(), new GracefulCancellationToken());
        }

        [TearDown]
        public void DeleteDatabaseObjects()
        {
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
