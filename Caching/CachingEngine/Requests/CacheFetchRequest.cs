using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CachingEngine.Requests
{
    /// <summary>
    /// Describes a normal caching request for a period of days/hours for an ICacheSource to request.  This includes start / end date which will be the next logical period
    /// of time to fetch to advance the head of an ICacheProgress (fetch the next date range and update the progress pointer).
    /// </summary>
    public class CacheFetchRequest : ICacheFetchRequest
    {
        [NoMappingToDatabase]
        public IRepository Repository { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get { return Start.Add(ChunkPeriod); } }
        public TimeSpan ChunkPeriod { get; set; }
        public IPermissionWindow PermissionWindow { get; set; }
        public ICacheProgress CacheProgress { get; set; }
        public ICacheFetchFailure PreviousFailure { get; set; }


        /// <summary>
        /// Is this CacheFetchRequest a retry of a previously failed fetch request?
        /// </summary>
        public bool IsRetry { get { return PreviousFailure != null; }}

        public CacheFetchRequest(IRepository repository, DateTime start)
        {
            Repository = repository;
            Start = start;
            PreviousFailure = null;
        }

        public CacheFetchRequest(IRepository repository): this(repository,DateTime.MinValue)
        {
        }

        /// <summary>
        /// Creates a CacheFetchRequest from a previous failure.
        /// </summary>
        /// <param name="cacheFetchFailure"></param>
        /// <param name="cacheProgress"></param>
        public CacheFetchRequest(ICacheFetchFailure cacheFetchFailure, ICacheProgress cacheProgress)
        {
            Start = cacheFetchFailure.FetchRequestStart;
            CacheProgress = cacheProgress;
            ChunkPeriod = cacheFetchFailure.FetchRequestEnd.Subtract(cacheFetchFailure.FetchRequestStart);
            PermissionWindow = cacheProgress.GetPermissionWindow();
            PreviousFailure = cacheFetchFailure;
        }

        public void SaveCacheFillProgress(DateTime cacheFillProgress)
        {
            CacheProgress.CacheFillProgress = cacheFillProgress;
            CacheProgress.SaveToDatabase();
        }

        public void RequestFailed(Exception e)
        {
            if (PreviousFailure != null)
            {
                PreviousFailure.LastAttempt = DateTime.Now;
                PreviousFailure.SaveToDatabase();
            }
            else
            {
                new CacheFetchFailure((ICatalogueRepository) Repository, CacheProgress,Start,End, e);
            }
        }

        public void RequestSucceeded()
        {
            if (IsRetry)
                PreviousFailure.Resolve();
            else
                SaveCacheFillProgress(End);
        }

        /// <summary>
        /// Factory method which creates the 'next' logical fetch request using the request's chunk period
        /// </summary>
        /// <returns></returns>
        public ICacheFetchRequest GetNext()
        {
            var nextStart = Start.Add(ChunkPeriod);
            return new CacheFetchRequest(Repository, nextStart)
            {
                CacheProgress = CacheProgress,
                PermissionWindow = PermissionWindow,
                ChunkPeriod = ChunkPeriod
            };
        }
    }
}