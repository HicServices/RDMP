using System;
using System.Collections.Generic;
using System.IO;
using CachingEngine.BasicCache;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibraryTests.Integration;
using DataLoadEngine.DataProvider.FromCache;
using DataLoadEngine.Job;
using DataLoadEngine.Job.Scheduling;
using HIC.Logging;
using LoadModules.Generic.DataProvider;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class CachedFileRetrieverTests : DatabaseTests
    {
        private ILoadProgress _lpMock;
        private ICacheProgress _cpMock;

        public CachedFileRetrieverTests()
        {
            _cpMock = MockRepository.GenerateMock<ICacheProgress>();

            _lpMock = MockRepository.GenerateMock<ILoadProgress>();
            _lpMock.Stub(cp => cp.GetCacheProgress()).Return(_cpMock);

            
        }



        [Test(Description = "RDMPDEV-185: Tests the scenario where the files in ForLoading do not match the files that are expected given the job specification. In this case the load process should not continue, otherwise the wrong data will be loaded.")]
        public void AttemptToLoadDataWithFilesInForLoading_DisagreementBetweenCacheAndForLoading()
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var tempDir = Directory.CreateDirectory(tempDirPath);

            try
            {
                // Different file in ForLoading than exists in cache
                var hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(tempDir, "CachedFileRetriever");
                var cachedFilePath = Path.Combine(hicProjectDirectory.Cache.FullName, "2016-01-02.zip");
                File.WriteAllText(cachedFilePath, "");
                File.WriteAllText(Path.Combine(hicProjectDirectory.ForLoading.FullName, "2016-01-01.zip"), "");

                // Set up retriever
                var cacheLayout = new ZipCacheLayoutOnePerDay(hicProjectDirectory.Cache,new NoSubdirectoriesCachePathResolver());
                
                var retriever = new TestCachedFileRetriever()
                {
                    ExtractFilesFromArchive = false,
                    LoadProgress = _lpMock,
                    Layout = cacheLayout
                };
                
                // Set up job
                var job = CreateTestJob(hicProjectDirectory); 
                job.DatesToRetrieve = new List<DateTime>
                {
                    new DateTime(2016, 01, 02)
                };

                // Should fail after determining that the files in ForLoading do not match the job specification
                var ex = Assert.Throws<InvalidOperationException>(() => retriever.Fetch(job, new GracefulCancellationToken()));
                Assert.IsTrue(ex.Message.StartsWith("The files in ForLoading do not match what this job expects to be loading from the cache."), ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                tempDir.Delete(true);
            }
        }

        [Test(Description = "RDMPDEV-185: Tests the scenario where the files in ForLoading match the files that are expected given the job specification, e.g. a load has after the cache has been populated and a subsequent load with *exactly the same parameters* has been triggered. In this case the load can proceed.")]
        public void AttemptToLoadDataWithFilesInForLoading_AgreementBetweenForLoadingAndCache()
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var tempDir = Directory.CreateDirectory(tempDirPath);

            try
            {
                // File in cache is the same file as in ForLoading (20160101.zip)
                var hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(tempDir, "CachedFileRetriever");
                var cachedFilePath = Path.Combine(hicProjectDirectory.Cache.FullName, "2016-01-01.zip");
                File.WriteAllText(cachedFilePath, "");
                File.WriteAllText(Path.Combine(hicProjectDirectory.ForLoading.FullName, "2016-01-01.zip"), "");


                // Set up retriever
                var cacheLayout = new ZipCacheLayoutOnePerDay(hicProjectDirectory.Cache, new NoSubdirectoriesCachePathResolver());

                var retriever = new TestCachedFileRetriever()
                {
                    ExtractFilesFromArchive = false,
                    LoadProgress = _lpMock,
                    Layout =  cacheLayout
                    
                };
                
                // Set up job
                var job = CreateTestJob(hicProjectDirectory); 
                job.DatesToRetrieve = new List<DateTime>
                {
                    new DateTime(2016, 01, 01)
                };

                // Should complete successfully, the file in ForLoading matches the job specification
                retriever.Fetch(job, new GracefulCancellationToken());

                // And ForLoading should still have the file in it (i.e. it hasn't mysteriously disappeared)
                Assert.IsTrue(File.Exists(Path.Combine(hicProjectDirectory.ForLoading.FullName, "2016-01-01.zip")));
            }
            finally
            {
                tempDir.Delete(true);
            }
        }

        [Test(Description = "RDMPDEV-185: Tests the default scenario where there are no files in ForLoading.")]
        public void AttemptToLoadDataWithoutFilesInForLoading()
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var tempDir = Directory.CreateDirectory(tempDirPath);

            try
            {
                // File in cache only, no files in ForLoading
                var hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(tempDir, "CachedFileRetriever");
                var cachedFilePath = Path.Combine(hicProjectDirectory.Cache.FullName, "2016-01-01.zip");
                File.WriteAllText(cachedFilePath, "");


                // Set up retriever
                var cacheLayout = new ZipCacheLayoutOnePerDay(hicProjectDirectory.Cache, new NoSubdirectoriesCachePathResolver());

                var retriever = new TestCachedFileRetriever()
                {
                    ExtractFilesFromArchive = false,
                    LoadProgress = _lpMock,
                    Layout = cacheLayout

                };

                // Set up job
                var job = CreateTestJob(hicProjectDirectory);
                job.DatesToRetrieve = new List<DateTime>
                {
                    new DateTime(2016, 01, 01)
                };

                // Should complete successfully, there are no files in ForLoading to worry about
                retriever.Fetch(job, new GracefulCancellationToken());

                // And the retriever should have copied the cached archive file into ForLoading
                Assert.IsTrue(File.Exists(Path.Combine(hicProjectDirectory.ForLoading.FullName, "2016-01-01.zip")));
            }
            finally
            {
                tempDir.Delete(true);
            }
        }

        private ScheduledDataLoadJob CreateTestJob(IHICProjectDirectory hicProjectDirectory)
        {
            var catalogue = MockRepository.GenerateStub<ICatalogue>();
            catalogue.Stub(c => c.GetTableInfoList(Arg<bool>.Is.Anything)).Return(new TableInfo[0]);
            catalogue.Stub(c => c.GetLookupTableInfoList()).Return(new TableInfo[0]);
            catalogue.Stub(c => c.LoggingDataTask).Return("TestLogging");

            var logManager = MockRepository.GenerateStub<ILogManager>();
            var loadMetadata = MockRepository.GenerateStub<ILoadMetadata>();
            loadMetadata.Stub(lm => lm.GetAllCatalogues()).Return(new[] { catalogue });

            var j =  new ScheduledDataLoadJob("Test job", logManager, loadMetadata, hicProjectDirectory, new ThrowImmediatelyDataLoadEventListener());
            j.LoadProgress = _lpMock;
            return j;
        }
    }

    internal class TestCachedFileRetriever : CachedFileRetriever
    {
        public ICacheLayout Layout;

        public override void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public override ExitCodeType Fetch(IDataLoadJob dataLoadJob, GracefulCancellationToken cancellationToken)
        {
            var scheduledJob = ConvertToScheduledJob(dataLoadJob);
            GetDataLoadWorkload(scheduledJob);
            ExtractJobs(scheduledJob);
            
            return ExitCodeType.Success;
        }

        protected override ICacheLayout CreateCacheLayout(ICacheProgress cacheProgress, IDataLoadEventListener listener)
        {
            return Layout;
        }
    }
}