using System.Collections.Generic;
using System.Linq;
using CachingEngine.Factories;
using CachingEngine.Layouts;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataProvider.FromCache;
using DataLoadEngine.Job.Scheduling.Exceptions;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Factories
{
    /// <summary>
    /// Creates <see cref="ICacheLayout"/> instances based on the <see cref="ICachedDataProvider"/>s declared in the load <see cref="ILoadMetadata"/>.  There
    /// can be multiple <see cref="ILoadProgress"/> in a load (e.g. Tayside / Fife) so you will also need to provide which <see cref="ILoadProgress"/> you are 
    /// trying to execute.
    /// </summary>
    public class CacheLayoutFactory
    {
        public ICacheLayout CreateCacheLayout(ILoadProgress loadProgress, ILoadMetadata metadata)
        {
            AssertThatThereIsACacheDataProvider(metadata, metadata.ProcessTasks.Where(p=>!p.IsDisabled));

            var cp = loadProgress.CacheProgress;

            var factory = new CachingPipelineUseCase(cp);
            var destination = factory.CreateDestinationOnly(new ThrowImmediatelyDataLoadEventListener());

            return destination.CreateCacheLayout();
        }

        private void AssertThatThereIsACacheDataProvider(ILoadMetadata metadata, IEnumerable<IProcessTask> processTasks)
        {
            const string whatWeExpected = @"(we expected one that was a MEF class implementing ICachedDataProvider since you are trying to execute a cache based data load)";

            List<ProcessTask> incompatibleProviders = new List<ProcessTask>();
            List<ProcessTask> compatibleProviders = new List<ProcessTask>();


            foreach (ProcessTask task in processTasks)
            {
                //it's not a DataProvider
                if (!task.ProcessTaskType.Equals(ProcessTaskType.DataProvider))
                    continue;

                var type = ((CatalogueRepository)task.Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(task.Path);

                if (typeof(ICachedDataProvider).IsAssignableFrom(type))
                    compatibleProviders.Add(task);
                else
                    incompatibleProviders.Add(task);
            }

            if (!incompatibleProviders.Any() && !compatibleProviders.Any())
                throw new CacheDataProviderFindingException("LoadMetadata " + metadata + " does not have ANY process tasks of type ProcessTaskType.DataProvider " + whatWeExpected);

            if (!compatibleProviders.Any())
                throw new CacheDataProviderFindingException("LoadMetadata " + metadata + " has some DataProviders tasks but none of them wrap classes that implement ICachedDataProvider " + whatWeExpected + " FYI the data providers in your load wrap the following classes:" + string.Join(",", incompatibleProviders.Select(t => t.Path)));

            if (compatibleProviders.Count > 1)
                throw new CacheDataProviderFindingException("LoadMetadata " + metadata + " has multiple cache DataProviders tasks (" + string.Join(",", compatibleProviders.Select(p => p.ToString())) + "), you are only allowed 1");

        }
    }
}
