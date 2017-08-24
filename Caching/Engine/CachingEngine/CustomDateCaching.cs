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
        private readonly IHICProjectDirectory _hicProjectDirectory;

        public CustomDateCaching(ICacheProgress cacheProgress, ICatalogueRepository catalogueRepository, IHICProjectDirectory hicProjectDirectory)
        {
            _cacheProgress = cacheProgress;
            _catalogueRepository = catalogueRepository;
            _hicProjectDirectory = hicProjectDirectory;
        }

        public Task Fetch(DateTime startDate, DateTime endDate, GracefulCancellationToken token, IDataLoadEventListener listener, IPermissionWindow permissionWindowOverride = null)
        {
            var permissionWindow = permissionWindowOverride ?? _cacheProgress.GetPermissionWindow();

            // todo: in general need better semantics around null PermissionWindows
            permissionWindow = permissionWindow ?? new PermissionWindow();

            var factory = new CachingPipelineEngineFactory();
            
            var engine = factory.CreateCachingPipelineEngine(_cacheProgress,(ICatalogueRepository) _cacheProgress.Repository,listener);
            var dateToRetrieve = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var initialFetchRequest = new BackfillCacheFetchRequest(_catalogueRepository, dateToRetrieve)
            {
                CacheProgress = _cacheProgress,
                ChunkPeriod = _cacheProgress.ChunkPeriod,
                PermissionWindow = permissionWindow
            };

            var requestProvider = (startDate == endDate)
                ? (ICacheFetchRequestProvider) new SingleDayCacheFetchRequestProvider(initialFetchRequest)
                : new MultiDayCacheFetchRequestProvider(initialFetchRequest, endDate);

            engine.Initialize(requestProvider, permissionWindow, _hicProjectDirectory, _catalogueRepository.MEF);

            return Task.Factory.StartNew(() => engine.ExecutePipeline(token));
        }
    }
}