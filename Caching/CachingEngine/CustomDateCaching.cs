using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CachingEngine
{
    /// <summary>
    /// Executes a caching configuration (ICacheProgress) for the specified arbitrary date/time range using either a SingleDayCacheFetchRequestProvider or a
    /// MultiDayCacheFetchRequestProvider.  Note that this uses a BackfillCacheFetchRequest which should mean that the actual progress head pointer (how far
    /// we think we have cached up to) does not change.  The reason we use a BackfillCacheFetchRequest is to allow us to run a given day/week again without
    /// reseting our entire progress back to that date. 
    /// </summary>
    public class CustomDateCaching
    {
        private readonly ICacheProgress _cacheProgress;
        private readonly ICatalogueRepository _catalogueRepository;

        public CustomDateCaching(ICacheProgress cacheProgress, ICatalogueRepository catalogueRepository)
        {
            _cacheProgress = cacheProgress;
            _catalogueRepository = catalogueRepository;
        }

        public Task Fetch(DateTime startDate, DateTime endDate, GracefulCancellationToken token, IDataLoadEventListener listener, bool ignorePermissionWindow = false)
        {
            var dateToRetrieve = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var initialFetchRequest = new BackfillCacheFetchRequest(_catalogueRepository, dateToRetrieve)
            {
                CacheProgress = _cacheProgress,
                ChunkPeriod = _cacheProgress.ChunkPeriod
            };

            var requestProvider = (startDate == endDate)
                ? (ICacheFetchRequestProvider) new SingleDayCacheFetchRequestProvider(initialFetchRequest)
                : new MultiDayCacheFetchRequestProvider(initialFetchRequest, endDate);
            
            var factory = new CachingPipelineUseCase(_cacheProgress, ignorePermissionWindow, requestProvider);

            var engine = factory.GetEngine(listener);
            
            return Task.Factory.StartNew(() => engine.ExecutePipeline(token));
        }
    }
}