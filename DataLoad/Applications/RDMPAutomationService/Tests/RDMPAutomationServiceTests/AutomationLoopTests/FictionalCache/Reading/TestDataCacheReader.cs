using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.DataProvider.FromCache;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache.Reading
{
    public class TestDataCacheReader : ICachedDataProvider
    {
        public ILoadProgress LoadProgress { get; set; }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            throw new NotImplementedException();
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public CacheArchiveType CacheArchiveType { get; set; }
        public string CacheDateFormat { get; set; }
        public Type CacheLayoutType { get; set; }
        public ILoadCachePathResolver CreateResolver(ILoadProgress loadProgress)
        {
            throw new NotImplementedException();
        }

    }
}
