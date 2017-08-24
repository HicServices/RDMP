using System;
using CachingEngine.PipelineExecution.Destinations;
using CachingEngine.PipelineExecution.Sources;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Factories
{
    public class CachingPipelineEngineFactory
    {
        public static DataFlowPipelineContext<ICacheChunk> Context;

        static CachingPipelineEngineFactory()
        {
            //create the context using the standard context factory
            var contextFactory = new DataFlowPipelineContextFactory<ICacheChunk>();
            Context = contextFactory.Create(PipelineUsage.None);

            //adjust context: we want a destination requirement of ICacheFileSystemDestination so that we can load from the cache using the pipeline endpoint as the source of the data load
            Context.MustHaveDestination = typeof(ICacheFileSystemDestination);//we want this freaky destination type
            Context.MustHaveSource = typeof (ICacheSource);
        }
        
        public IDataFlowPipelineEngine CreateCachingPipelineEngine(ICacheProgress cacheProgress, ICatalogueRepository repository, IDataLoadEventListener listener)
        {
            return CreateCachingPipelineEngineWithProvider(cacheProgress,repository,listener);
         
        }
        public IDataFlowPipelineEngine CreateRetryCachingPipelineEngine(ICacheProgress cacheProgress, ICatalogueRepository repository, IDataLoadEventListener listener)
        {
            return CreateCachingPipelineEngineWithProvider(cacheProgress,repository,listener,new FailedCacheFetchRequestProvider(cacheProgress));
        }

        public ICacheFileSystemDestination CreateDestinationOnly(ICacheProgress cacheProgress, IDataLoadEventListener listener)
        {
            ConfirmCacheProgressCompatibility(cacheProgress);

            ICatalogueRepository repo = (ICatalogueRepository)cacheProgress.Repository;

            var initObjects = GetInitializationObjects(cacheProgress, repo);
            
            // Create the pipeline engine factory and use it to stamp out a pipeline instance from the CacheProgress
            var factory = new DataFlowPipelineEngineFactory<ICacheChunk>(repo.MEF, Context);
            var destination = factory.CreateDestinationIfExists(cacheProgress.Pipeline);

            if(destination == null)
                throw new Exception( cacheProgress + " does not have a DestinationComponent in it's Pipeline");

            if(!(destination is ICacheFileSystemDestination))
                throw new NotSupportedException(cacheProgress + " pipeline destination is not an ICacheFileSystemDestination, it was " + cacheProgress.GetType().FullName);

            Context.PreInitialize(listener, destination, initObjects[0],initObjects[1],initObjects[2],initObjects[3]);

            return (ICacheFileSystemDestination) destination;

        }


        private void ConfirmCacheProgressCompatibility(ICacheProgress cacheProgress)
        {
            if(cacheProgress.Pipeline_ID == null)
                throw new Exception("CacheProgress " + cacheProgress + " does not have a Pipeline configured on it");
        }


        private IDataFlowPipelineEngine CreateCachingPipelineEngineWithProvider(ICacheProgress cacheProgress, ICatalogueRepository repository, IDataLoadEventListener listener, ICacheFetchRequestProvider providerIfAny = null)
        {
            ConfirmCacheProgressCompatibility(cacheProgress);

            object[] initObjects = GetInitializationObjects(cacheProgress, repository);

            // Create the pipeline engine factory and use it to stamp out a pipeline instance from the CacheProgress
            var factory = new DataFlowPipelineEngineFactory<ICacheChunk>(repository.MEF, Context);
            var instance = factory.Create(cacheProgress.Pipeline, listener);

            //initialize it
            instance.Initialize(initObjects[0], initObjects[1], initObjects[2], initObjects[3]);

            if (providerIfAny != null)
                initObjects[0] = providerIfAny;

            return instance;
        }

        /// <summary>
        /// Returns array [0] is a cache fetch request provider 
        /// [1] is a permission window if any
        /// [2] is hic project directory 
        /// [3] is a MEF (can actually be fetched from cacheProgress but this is neater I suppose having the argument explicitly)
        /// 
        /// The numbers are required so that it can be used as params to instance.Initialize
        /// </summary>
        /// <param name="cacheProgress"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        private object[] GetInitializationObjects(ICacheProgress cacheProgress, ICatalogueRepository repository)
        {
            object[] toReturn = new object[4];

            var loadProgress = cacheProgress.GetLoadProgress();

            var cacheFetchRequestFactory = new CacheFetchRequestFactory();
            var initialFetchRequest = cacheFetchRequestFactory.Create(cacheProgress, loadProgress);
            toReturn[0] = new CacheFetchRequestProvider(initialFetchRequest);

            toReturn[1] = initialFetchRequest.PermissionWindow;


            // Get the HICProjectDirectory for the engine initialization
            var loadMetadata = loadProgress.GetLoadMetadata();

            var hicProjectDirectory = new HICProjectDirectory(loadMetadata.LocationOfFlatFiles, false);
            toReturn[2] = hicProjectDirectory;

            toReturn[3] = repository.MEF;

            return toReturn;
        }

    }
}