using System;
using System.Linq;
using CachingEngine.Layouts;
using CatalogueLibrary.Data;
using DataLoadEngine.Factories;
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

            var factory = new CacheLayoutFactory();

            if (_typeToCreate == typeof(SingleScheduleCacheDateTrackingStrategy))
                return new SingleScheduleCacheDateTrackingStrategy(factory.CreateCacheLayout(loadProgress, loadMetadata), loadProgress,listener);

            throw new Exception("Factory has been configured to supply an unknown type");
        }
    }
}