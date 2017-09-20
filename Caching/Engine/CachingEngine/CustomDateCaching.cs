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