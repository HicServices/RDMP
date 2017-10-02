using System;
using System.Collections.Generic;
using System.Linq;
using CachingEngine.PipelineExecution.Destinations;
using CachingEngine.PipelineExecution.Sources;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Factories
{
    public class CachingPipelineUseCase:PipelineUseCase
    {
        private readonly ICacheProgress _cacheProgress;
        private readonly ICacheFetchRequestProvider _providerIfAny;
        private DataFlowPipelineContext<ICacheChunk> _context;
        private IPipeline _pipeline;
        private ICatalogueRepository _catalogueRepository;
        private PermissionWindow _permissionWindow;
        private HICProjectDirectory _hicProjectDirectory;

        /// <summary>
        /// Class for helping you to construct a caching pipeline engine instance with the correct context and initialization objects
        /// </summary>
        /// <param name="cacheProgress">The cache that will be run</param>
        /// <param name="ignorePermissionWindow">Set to true to ignore the CacheProgress.PermissionWindow (if any)</param>
        /// <param name="providerIfAny">The strategy for figuring out what dates to load the cache with e.g. failed cache fetches or new jobs from head of que?</param>
        public CachingPipelineUseCase(ICacheProgress cacheProgress,bool ignorePermissionWindow=false,ICacheFetchRequestProvider providerIfAny = null,bool throwIfNoPipeline = true)
        {
            _cacheProgress = cacheProgress;
            _providerIfAny = providerIfAny;

            //create the context using the standard context factory
            var contextFactory = new DataFlowPipelineContextFactory<ICacheChunk>();
            _context = contextFactory.Create(PipelineUsage.None);

            //adjust context: we want a destination requirement of ICacheFileSystemDestination so that we can load from the cache using the pipeline endpoint as the source of the data load
            _context.MustHaveDestination = typeof(ICacheFileSystemDestination);//we want this freaky destination type
            _context.MustHaveSource = typeof(ICacheSource);

            if(_providerIfAny == null)
            {
                var cacheFetchRequestFactory = new CacheFetchRequestFactory();
                var initialFetchRequest = cacheFetchRequestFactory.Create(_cacheProgress, _cacheProgress.LoadProgress);
                _providerIfAny = new CacheFetchRequestProvider(initialFetchRequest);
            }

            _pipeline = _cacheProgress.Pipeline;

            if (_pipeline == null && throwIfNoPipeline)
                throw new Exception("CacheProgress " + _cacheProgress + " does not have a Pipeline configured on it");

            _catalogueRepository = (ICatalogueRepository)_cacheProgress.Repository;

            if(!ignorePermissionWindow)
                _permissionWindow = cacheProgress.PermissionWindow??new PermissionWindow();

            // Get the HICProjectDirectory for the engine initialization
            var lmd = _cacheProgress.GetLoadProgress().GetLoadMetadata();
            _hicProjectDirectory = new HICProjectDirectory(lmd.LocationOfFlatFiles, false);
            
        }

        public ICacheFileSystemDestination CreateDestinationOnly( IDataLoadEventListener listener)
        {
            // get the current destination
            var destination = GetEngine(_pipeline, listener).DestinationObject;
            
            if(destination == null)
                throw new Exception(_cacheProgress + " does not have a DestinationComponent in it's Pipeline");

            if(!(destination is ICacheFileSystemDestination))
                throw new NotSupportedException(_cacheProgress + " pipeline destination is not an ICacheFileSystemDestination, it was " + _cacheProgress.GetType().FullName);
            
            return (ICacheFileSystemDestination) destination;
        }

        public IDataFlowPipelineEngine GetEngine(IDataLoadEventListener listener)
        {
            return GetEngine(_pipeline, listener);
        }
        
        public override object[] GetInitializationObjects(ICatalogueRepository repository)
        {
            return new object[]{_providerIfAny,
            _permissionWindow,
            _hicProjectDirectory,
            _catalogueRepository};
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }
    }

}