using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CachingEngine;
using CachingEngine.BasicCache;
using CachingEngine.Factories;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CachingEngine.PipelineExecution.Sources;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace CachingEngineTests.Integration
{
    [Category("Integration")]
    public class CustomDateCachingTests : DatabaseTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void FetchMultipleDays_Success(bool singleDay)
        {
            var mef = RepositoryLocator.CatalogueRepository.MEF;
            mef.AddTypeToCatalogForTesting(typeof(TestCacheSource));
            mef.AddTypeToCatalogForTesting(typeof(TestCacheDestination));

            // Create a pipeline that will record the cache chunks
            var pipeline = MockRepository.GenerateStub<IPipeline>();
            var sourceComponent = MockRepository.GenerateStub<IPipelineComponent>();
            sourceComponent.Class = "CachingEngineTests.Integration.TestCacheSource";
            sourceComponent.Stub(c => c.GetClassAsSystemType()).Return(typeof (TestCacheSource));
            sourceComponent.Stub(c => c.GetAllArguments()).Return(new IArgument[0]);

            var destinationComponent = MockRepository.GenerateStub<IPipelineComponent>();
            destinationComponent.Class = "CachingEngineTests.Integration.TestCacheDestination";
            destinationComponent.Stub(c => c.GetClassAsSystemType()).Return(typeof (TestCacheDestination));
            destinationComponent.Stub(c => c.GetAllArguments()).Return(new IArgument[0]);

            pipeline.Repository = CatalogueRepository;
            pipeline.Stub(p => p.Source).Return(sourceComponent);
            pipeline.Stub(p => p.Destination).Return(destinationComponent);
            pipeline.Stub(p => p.PipelineComponents).Return(Enumerable.Empty<IPipelineComponent>().OrderBy(p => p));

            var projDir = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo("delme"),true);

            var lmd = MockRepository.GenerateStub<ILoadMetadata>();
            lmd.LocationOfFlatFiles = projDir.RootPath.FullName;

            var loadProgress = MockRepository.GenerateStub<ILoadProgress>();
            loadProgress.OriginDate = new DateTime(2001,01,01);
            loadProgress.Expect(m => m.LoadMetadata).Return(lmd);

            var cacheProgress = MockRepository.GenerateStub<ICacheProgress>();
            cacheProgress.Pipeline_ID = -123;
            cacheProgress.Stub(c => c.Pipeline).Return(pipeline);
            cacheProgress.ChunkPeriod = new TimeSpan(1, 0, 0, 0);
            cacheProgress.LoadProgress_ID = -1;
            cacheProgress.Repository = CatalogueRepository;
            cacheProgress.Expect(m => m.LoadProgress).Return(loadProgress);
            cacheProgress.CacheFillProgress = new DateTime(2020, 1, 1);

            var caching = new CustomDateCaching(cacheProgress, RepositoryLocator.CatalogueRepository);
            var startDate = new DateTime(2016, 1, 1);
            var endDate = singleDay? new DateTime(2016, 1, 1): new DateTime(2016, 1, 3);

            var listener = new LoggingListener();
            var task = caching.Fetch(startDate, endDate, new GracefulCancellationToken(), listener);
            task.Wait();

            var dateNotifications = listener.Notifications.Where(n => n.Message.StartsWith("!!"))
                .Select(n => n.Message.TrimStart('!'))
                .ToArray();

            //should not have been updated because this is a backfill request
            Assert.AreEqual(new DateTime(2020,1,1),cacheProgress.CacheFillProgress);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(dateNotifications.Contains(startDate.ToString("g")));
            Assert.IsTrue(dateNotifications.Contains(endDate.ToString("g")));
            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);

            projDir.RootPath.Delete(true);
        }
    }

    public class LoggingListener : IDataLoadEventListener
    {
        public List<NotifyEventArgs> Notifications { get; private set; }

        public LoggingListener()
        {
            Notifications = new List<NotifyEventArgs>();
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            Notifications.Add(e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
        }
    }

    public class TestCacheChunk : ICacheChunk
    {
        public ICacheFetchRequest Request { get; private set; }

        public TestCacheChunk(DateTime fetchDate)
        {
            Request = new CacheFetchRequest(null,fetchDate){ChunkPeriod = new TimeSpan(1,0,0)};
        }

        
    }

    public class TestCacheSource : CacheSource<TestCacheChunk>
    {
        public override void DoGetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            Chunk = new TestCacheChunk(Request.Start);
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "!!" + Chunk.Request.Start.ToString("g")));
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public override void Abort(IDataLoadEventListener listener)
        {
        }

        public override TestCacheChunk TryGetPreview()
        {
            throw new NotImplementedException();
        }

        public override void Check(ICheckNotifier notifier)
        {
        }
    }

    public class TestCacheDestination : IPluginDataFlowComponent<ICacheChunk>, IDataFlowDestination<ICacheChunk>, ICacheFileSystemDestination 
    {
        public TestCacheChunk ProcessPipelineData(TestCacheChunk toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            return toProcess;
        }

        public ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener,
            GracefulCancellationToken cancellationToken)
        {
            return ProcessPipelineData((TestCacheChunk) toProcess, listener, cancellationToken);
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Abort(IDataLoadEventListener listener)
        {
        }

        public void Check(ICheckNotifier notifier)
        {
        }

        private IHICProjectDirectory project;
        public void PreInitialize(IHICProjectDirectory value, IDataLoadEventListener listener)
        {
            project = value;
        }

        public ICacheLayout CreateCacheLayout()
        {
            return new BasicCacheLayout(project.Cache);
        }

    }
}