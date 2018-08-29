using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using CachingEngine.Factories;
using CachingEngine.Layouts;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DataProvider;
using DataLoadEngine.DataProvider.FromCache;
using DataLoadEngine.Job.Scheduling.Exceptions;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// Decides the date generation strategy (e.g. pick next X days from the head of the LoadProgress or base dates loaded on next files available in cache)
    /// </summary>
    public class JobDateGenerationStrategyFactory
    {
        private readonly Type _typeToCreate;
        
        /// <summary>
        /// Always respects the LoadProgress dates and crashes if there arent any load progresses associated with the given load metadata
        /// Uses SingleScheduleCacheDateTrackingStrategy if there is a cache associated with any of the load progresses otherwise uses SingleScheduleConsecutiveDateStrategy (meaning for example each day for the next 5 days)
        /// </summary>
        /// <param name="strategy"></param>
        public JobDateGenerationStrategyFactory(ILoadProgressSelectionStrategy strategy)

        {
            _typeToCreate =
                strategy.GetAllLoadProgresses().Any(p => p.CacheProgress != null)//if any of the strategies you plan to use (without locking btw) have a cache progress
                ? typeof (SingleScheduleCacheDateTrackingStrategy) //then we should use a cache progress based strategy
                : typeof (SingleScheduleConsecutiveDateStrategy);//otherwise we should probably use consecutive days strategy;
        }

        public IJobDateGenerationStrategy Create(ILoadProgress loadProgress, IDataLoadEventListener listener)
        {
            if (_typeToCreate == typeof(SingleScheduleConsecutiveDateStrategy))
                return new SingleScheduleConsecutiveDateStrategy(loadProgress);

            var loadMetadata = loadProgress.LoadMetadata;
            
            if (_typeToCreate == typeof(SingleScheduleCacheDateTrackingStrategy))
                return new SingleScheduleCacheDateTrackingStrategy(CreateCacheLayout(loadProgress, loadMetadata, listener), loadProgress,listener);

            throw new Exception("Factory has been configured to supply an unknown type");
        }

        private ICacheLayout CreateCacheLayout(ILoadProgress loadProgress, ILoadMetadata metadata, IDataLoadEventListener listener)
        {
            AssertThatThereIsACacheDataProvider(metadata, metadata.ProcessTasks);

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
                if(!task.ProcessTaskType.Equals(ProcessTaskType.DataProvider))
                    continue;

                var type = ((CatalogueRepository) task.Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(task.Path);

                if (typeof (ICachedDataProvider).IsAssignableFrom(type))
                    compatibleProviders.Add(task);
                else
                    incompatibleProviders.Add(task);
            }
            
            if (!incompatibleProviders.Any() && !compatibleProviders.Any())
                throw new CacheDataProviderFindingException("LoadMetadata " + metadata + " does not have ANY process tasks of type ProcessTaskType.DataProvider " + whatWeExpected);

            if(!compatibleProviders.Any())
                throw new CacheDataProviderFindingException("LoadMetadata " + metadata + " has some DataProviders tasks but none of them wrap classes that implement ICachedDataProvider " + whatWeExpected + " FYI the data providers in your load wrap the following classes:" + string.Join(",",incompatibleProviders.Select(t=>t.Path)));

            if(compatibleProviders.Count > 1)
                throw new CacheDataProviderFindingException("LoadMetadata " + metadata + " has multiple cache DataProviders tasks (" + string.Join(",",compatibleProviders.Select(p=>p.ToString())) + "), you are only allowed 1");

        }
    }
}