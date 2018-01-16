using System;
using System.Data.SqlClient;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DataProvider.FromCache;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.Job.Scheduling.Exceptions;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using DataLoadEngineTests.Integration.Cache;
using DataLoadEngineTests.Integration.PipelineTests;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class JobDateGenerationStrategyFactoryTestsIntegration:DatabaseTests
    {
        private CacheProgress _cp;
        private LoadProgress _lp;
        private LoadMetadata _lmd;
        private DiscoveredServer _server;
        private JobDateGenerationStrategyFactory _factory;

        [SetUp]
        public void up()
        {
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

            _lmd = new LoadMetadata(CatalogueRepository, "JobDateGenerationStrategyFactoryTestsIntegration");
            _lp = new LoadProgress(CatalogueRepository, _lmd);

            _lp.DataLoadProgress = new DateTime(2001, 1, 1);
            _lp.SaveToDatabase();

            _cp = new CacheProgress(CatalogueRepository, _lp);


            _server = new DiscoveredServer(new SqlConnectionStringBuilder("server=localhost;initial catalog=fish"));
            _factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(_lp));
        }

        [Test]
        public void CacheProvider_None()
        {
            var ex = Assert.Throws<CacheDataProviderFindingException>(() => _factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener()));
            Assert.IsTrue(ex.Message.StartsWith("LoadMetadata JobDateGenerationStrategyFactoryTestsIntegration does not have ANY process tasks of type ProcessTaskType.DataProvider"));
        }


        [Test]
        public void CacheProvider_NonCachingOne()
        {
            var pt = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            pt.Path = typeof (LoadModules.Generic.DataProvider.DoNothingDataProvider).FullName;
            pt.ProcessTaskType = ProcessTaskType.DataProvider;
            pt.Name = "DoNothing";
            pt.SaveToDatabase();
            
            var ex = Assert.Throws<CacheDataProviderFindingException>(() => _factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener()));
            Assert.IsTrue(ex.Message.StartsWith("LoadMetadata JobDateGenerationStrategyFactoryTestsIntegration has some DataProviders tasks but none of them wrap classes that implement ICachedDataProvider"));
        }


        [Test]
        public void CacheProvider_TwoCachingOnes()
        {
            var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            pt1.Path = typeof(TestCachedFileRetriever).FullName;
            pt1.ProcessTaskType = ProcessTaskType.DataProvider;
            pt1.Name = "Cache1";
            pt1.SaveToDatabase();

            var pt2 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            pt2.Path = typeof(TestCachedFileRetriever).FullName;
            pt2.ProcessTaskType = ProcessTaskType.DataProvider;
            pt2.Name = "Cache2";
            pt2.SaveToDatabase();

            var ex = Assert.Throws<CacheDataProviderFindingException>(() => _factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener()));
            Assert.AreEqual("LoadMetadata JobDateGenerationStrategyFactoryTestsIntegration has multiple cache DataProviders tasks (Cache1,Cache2), you are only allowed 1",ex.Message);
        }

        [Test]
        public void CacheProvider_NoPipeline()
        {
            var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            pt1.Path = typeof(TestCachedFileRetriever).FullName;
            pt1.ProcessTaskType = ProcessTaskType.DataProvider;
            pt1.Name = "Cache1";
            pt1.SaveToDatabase();

            _cp.CacheFillProgress = new DateTime(1999, 1, 1);
            _cp.SaveToDatabase();

            pt1.CreateArgumentsForClassIfNotExists<TestCachedFileRetriever>();

            var dir = new DirectoryInfo("delme");

            var projDir = HICProjectDirectory.CreateDirectoryStructure(dir, true);
            _lmd.LocationOfFlatFiles = projDir.RootPath.FullName;
            _lmd.SaveToDatabase();
            try
            {
                var ex = Assert.Throws<Exception>(() => _factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener()));
                Assert.AreEqual("CacheProgress Cache Progress "+_cp.ID+" does not have a Pipeline configured on it", ex.Message);
            }
            finally
            {
                dir.Delete(true);
            }
        }

        [Test]
        public void CacheProvider_NoCacheProgress()
        {
            var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            pt1.Path = typeof(BasicCacheDataProvider).FullName;
            pt1.ProcessTaskType = ProcessTaskType.DataProvider;
            pt1.Name = "Cache1";
            pt1.SaveToDatabase();

            var dir = new DirectoryInfo("delme");

            var projDir = HICProjectDirectory.CreateDirectoryStructure(dir, true);
            _lmd.LocationOfFlatFiles = projDir.RootPath.FullName;
            _lmd.SaveToDatabase();

            var pipeAssembler = new TestDataPipelineAssembler("CacheProvider_Normal", CatalogueRepository);
            pipeAssembler.ConfigureCacheProgressToUseThePipeline(_cp);

            try
            {
                var ex = Assert.Throws<Exception>(()=>_factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener()));
                Assert.AreEqual("Don't know when to begin loading the cache from. Neither CacheProgress or LoadProgress has a relevant date.",ex.Message);
            }
            finally
            {
                _cp.Pipeline_ID = null;
                pipeAssembler.Destroy();
                dir.Delete(true);
            }
        }
        [Test]
        public void CacheProvider_Normal()
        {
            var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            pt1.Path = typeof(BasicCacheDataProvider).FullName;
            pt1.ProcessTaskType = ProcessTaskType.DataProvider;
            pt1.Name = "Cache1";
            pt1.SaveToDatabase();

            _cp.CacheFillProgress = new DateTime(2010, 1, 1);
            _cp.SaveToDatabase();

            var dir = new DirectoryInfo("delme");

            //delete remnants
            if (dir.Exists)
                dir.Delete(true);

            var projDir = HICProjectDirectory.CreateDirectoryStructure(dir, true);
            _lmd.LocationOfFlatFiles = projDir.RootPath.FullName;
            _lmd.SaveToDatabase();
            
            var pipeAssembler = new TestDataPipelineAssembler("CacheProvider_Normal", CatalogueRepository);
            pipeAssembler.ConfigureCacheProgressToUseThePipeline(_cp);

            try
            {
                var strategy = _factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener());
                Assert.AreEqual(typeof(SingleScheduleCacheDateTrackingStrategy), strategy.GetType());
                
                var dates = strategy.GetDates(10, false);
                Assert.AreEqual(0,dates.Count); //zero dates to load because no files in cache

                File.WriteAllText(Path.Combine(projDir.Cache.FullName, "2001-01-02.zip"),"bobbobbobyobyobyobbzzztproprietarybitztreamzippy");
                File.WriteAllText(Path.Combine(projDir.Cache.FullName, "2001-01-03.zip"), "bobbobbobyobyobyobbzzztproprietarybitztreamzippy");
                File.WriteAllText(Path.Combine(projDir.Cache.FullName, "2001-01-05.zip"), "bobbobbobyobyobyobbzzztproprietarybitztreamzippy");
                
                strategy = _factory.Create(_lp,new ThrowImmediatelyDataLoadEventListener());
                Assert.AreEqual(typeof(SingleScheduleCacheDateTrackingStrategy), strategy.GetType());
                dates = strategy.GetDates(10, false);
                Assert.AreEqual(3, dates.Count); //zero dates to load because no files in cache


            }
            finally
            {
                _cp.Pipeline_ID = null;
                pipeAssembler.Destroy();
                dir.Delete(true);
            }
        }

        [TearDown]
        public void down()
        {
            _cp.DeleteInDatabase();
            _lp.DeleteInDatabase();
            _lmd.DeleteInDatabase();
        }
    }
}

