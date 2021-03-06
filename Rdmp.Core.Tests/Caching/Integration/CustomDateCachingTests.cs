// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Caching;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Caching.Pipeline.Sources;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.Caching.Integration
{
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
                        var sourceComponent = Mock.Of<IPipelineComponent>(x=>
                x.Class == "CachingEngineTests.Integration.TestCacheSource" &&
                x.GetClassAsSystemType()==typeof (TestCacheSource) &&
                x.GetAllArguments()==new IArgument[0]);

            var destinationComponent = Mock.Of<IPipelineComponent>(x=>
                x.Class == "CachingEngineTests.Integration.TestCacheDestination" &&
                x.GetClassAsSystemType()==typeof (TestCacheDestination) &&
                x.GetAllArguments()==new IArgument[0]);

            var pipeline = Mock.Of<IPipeline>(p=>
                p.Repository == CatalogueRepository &&
                p.Source==sourceComponent &&
                p.Destination==destinationComponent &&
                p.Repository == CatalogueRepository &&
                p.PipelineComponents==Enumerable.Empty<IPipelineComponent>().OrderBy(o => o).ToList());
            
            var projDir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"delme",true);

            var lmd = Mock.Of<ILoadMetadata>();
            lmd.LocationOfFlatFiles = projDir.RootPath.FullName;

            var loadProgress = Mock.Of<ILoadProgress>(l=>
            l.OriginDate == new DateTime(2001,01,01) &&
            l.LoadMetadata==lmd);

            var cacheProgress = Mock.Of<ICacheProgress>(c=>

            c.Pipeline_ID == -123 &&
            c.Pipeline==pipeline &&
            c.ChunkPeriod == new TimeSpan(1, 0, 0, 0) &&
            c.LoadProgress_ID == -1 &&
            c.Repository == CatalogueRepository &&
            c.LoadProgress ==loadProgress &&
            c.CacheFillProgress == new DateTime(2020, 1, 1));

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
        public override TestCacheChunk DoGetChunk(ICacheFetchRequest request, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var c = new TestCacheChunk(Request.Start);
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "!!" + request.Start.ToString("g")));
            return c;
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

        private ILoadDirectory project;
        public void PreInitialize(ILoadDirectory value, IDataLoadEventListener listener)
        {
            project = value;
        }

        public ICacheLayout CreateCacheLayout()
        {
            return new BasicCacheLayout(project.Cache);
        }

    }
}