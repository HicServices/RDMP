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
    public class JobDateGenerationStrategyFactory
    {
        private readonly Type _typeToCreate;
        private readonly HICDatabaseConfiguration _dbConfiguration;

        public JobDateGenerationStrategyFactory(Type typeToCreate, HICDatabaseConfiguration dbConfiguration)
        {
            _typeToCreate = typeToCreate;
            _dbConfiguration = dbConfiguration;
        }

        /// <summary>
        /// Always respects the LoadProgress dates and crashes if there arent any load progresses associated with the given load metadata
        /// Uses SingleScheduleCacheDateTrackingStrategy if there is a cache associated with any of the load progresses otherwise uses SingleScheduleConsecutiveDateStrategy (meaning for example each day for the next 5 days)
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="databaseLoadConfiguration"></param>
        public JobDateGenerationStrategyFactory(ILoadProgressSelectionStrategy strategy, HICDatabaseConfiguration databaseLoadConfiguration)
            :
            this(
            strategy.GetAllLoadProgresses(false).Any(p => p.GetCacheProgress() != null)//if any of the strategies you plan to use (without locking btw) have a cache progress
            ?typeof(SingleScheduleCacheDateTrackingStrategy)//then we should use a cache progress based strategy
            :typeof(SingleScheduleConsecutiveDateStrategy),databaseLoadConfiguration)//otherwise we should probably use consecutive days strategy
        {
            // Left for reference: this contract is not valid because GetAllLoadProgresses is not a Pure function
            // Contract.Requires<ArgumentException>(strategy.GetAllLoadProgresses(false).Any(),"There were no LoadProgress objects associated with the given strategy (see your LoadMetadata for more information) so why are you asking for job date creation/allocation logic");
        }

        public IJobDateGenerationStrategy Create(ILoadProgress loadProgress, IDataLoadEventListener listener)
        {
            if (_typeToCreate == typeof(SingleScheduleConsecutiveDateStrategy))
                return new SingleScheduleConsecutiveDateStrategy(loadProgress);

            var loadMetadata = loadProgress.GetLoadMetadata();
            
            if (_typeToCreate == typeof(SingleScheduleCacheDateTrackingStrategy))
                return new SingleScheduleCacheDateTrackingStrategy(CreateCacheLayout(loadProgress, loadMetadata, listener), loadProgress,listener);

            throw new Exception("Factory has been configured to supply an unknown type");
        }

        private ICacheLayout CreateCacheLayout(ILoadProgress loadProgress, ILoadMetadata metadata, IDataLoadEventListener listener)
        {
            AssertThatThereIsACacheDataProvider(metadata, metadata.ProcessTasks);

            var cp = loadProgress.GetCacheProgress();

            var factory = new CachingPipelineUseCase(cp);
            var destination = factory.CreateDestinationOnly(new ThrowImmediatelyDataLoadEventListener());
            
            return destination.CreateCacheLayout();
        }

        private void AssertThatThereIsACacheDataProvider(ILoadMetadata metadata, IEnumerable<ProcessTask> processTasks)
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