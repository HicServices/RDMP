using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using CachingEngine.Factories;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// This returns dates by examining cache filenames whilst honouring the Load and Cache Progress information
    /// </summary>
    public class SingleScheduleCacheDateTrackingStrategy : IJobDateGenerationStrategy
    {
        private readonly Queue<DateTime> _availableDates = new Queue<DateTime>();
        private readonly DateTime _lastDateForLoading;

        public SingleScheduleCacheDateTrackingStrategy(ICacheLayout cacheLayout, ILoadProgress loadProgress)
        {
            // no null check needed as the contract ensures that both DataLoadProgress and OriginDate can't simultaneously be null
            var lastAssignedLoadDate = loadProgress.DataLoadProgress == null ? loadProgress.OriginDate.Value : loadProgress.DataLoadProgress.Value;

            // This is all the dates in the cache, but we want to start from _lastAssignedLoadDate
            // todo: must be efficient, revisit
            _availableDates = cacheLayout.GetSortedDateQueue();
            if (_availableDates.Any())
            {
                while (_availableDates.Peek() <= lastAssignedLoadDate)
                    _availableDates.Dequeue();
            }

            _lastDateForLoading = CalculateLastLoadDate(loadProgress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadProgress"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Caching has not been configured correctly or the caching process has not begun</exception>
        public DateTime CalculateLastLoadDate(ILoadProgress loadProgress)
        {
            // Compute the last cache date from the CacheFillProgress date
            // CacheFillProgress is the date up to which caching has been performed, and is therefore the date from which caching will next begin.
            var cacheProgress = loadProgress.GetCacheProgress();
            if (cacheProgress == null)
                throw new InvalidOperationException("Could not retrieve the CacheProgress from LoadProgress " + loadProgress.ID + " (ensure caching is configured on this load before using this strategy)");
            if (cacheProgress.CacheFillProgress == null)
                throw new InvalidOperationException("Caching has not begun for this CacheProgress (" + cacheProgress.ID + "), so there is nothing to load and this strategy should not be used.");

            // We don't want to load partially filled cache files, so use the CacheFileGranularity to calculate the latest file we can safely load
            // CacheFileGranularity is a caching pipeline component argument, so need to get the runtime pipeline
            var cachedFileRetriever = GetCacheDestinationPipelineComponent(cacheProgress);
            var layout = cachedFileRetriever.CreateCacheLayout();

            return CalculateLastLoadDate(layout.CacheFileGranularity, cacheProgress.CacheFillProgress.Value);
        }

        /// <summary>
        /// Retrieves the destination component from the caching pipeline associated with the ICacheProgress object. The destination component is required to be an ICacheFileSystemDestination.
        /// </summary>
        /// <param name="cacheProgress"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Caching pipeline is not configured properly/doesn't exist</exception>
        private static ICacheFileSystemDestination GetCacheDestinationPipelineComponent(ICacheProgress cacheProgress)
        {
            if (cacheProgress.Pipeline_ID == null)
                throw new InvalidOperationException("This CacheProgress does not have a caching pipeline, please configure one.");

            var factory = new CachingPipelineEngineFactory(cacheProgress);
            ICacheFileSystemDestination destination;
            
            try
            {
                destination = factory.CreateDestinationOnly(new ThrowImmediatelyDataLoadEventListener());
            }
            catch (Exception e)
            {
                throw new Exception("We identified that your cache uses pipeline "+cacheProgress.Pipeline+ " but we could not instantiate the Pipeline's Destination instance, make sure the pipeline is intact in PipelineDiagramUI.  See inner exception for details",e );
            }

            return destination;
        }

        public static DateTime CalculateLastLoadDate(CacheFileGranularity cacheFileGranularity, DateTime nextDateToBeCached)
        {
            switch (cacheFileGranularity)
            {
                case CacheFileGranularity.Hour:
                    var priorDate = nextDateToBeCached.AddHours(-1);
                    return priorDate.Date.AddHours(priorDate.Hour); // to get rid of any minutes
                case CacheFileGranularity.Day:
                    // Get the beginning of the day prior to nextDateToBeCached
                    return nextDateToBeCached.AddDays(-1).Date;
                default:
                    throw new ArgumentOutOfRangeException("cacheFileGranularity", cacheFileGranularity, "CacheFileGranularity must either be Hour or Day.");
            }
        }

        // todo: figure out how allowLoadingFutureDates fits into this
        public List<DateTime> GetDates(int batchSize, bool allowLoadingFutureDates)
        {
            var dates = new List<DateTime>();

            for (var i = 0; i < batchSize; i++)
            {
                if (!_availableDates.Any())
                    break;

                var nextDate = _availableDates.Dequeue();
                if (nextDate > _lastDateForLoading)
                    break;

                dates.Add(nextDate);
            }

            return dates;
        }

        public int GetTotalNumberOfJobs(int batchSize, bool allowLoadingFutureDates)
        {
            var totalNumberOfValidDates = _availableDates.Count(time => time <= _lastDateForLoading);
            var totalNumberOfJobs = totalNumberOfValidDates/batchSize;
            if (totalNumberOfValidDates%batchSize > 0) ++totalNumberOfJobs;
            return totalNumberOfJobs;
        }
    }
}