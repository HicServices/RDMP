using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using MapsDirectlyToDatabaseTable;

namespace CachingEngine.Requests
{
    // todo: this is probably obsolete now? Investigate.
    public class BackfillCacheFetchRequest : ICacheFetchRequest
    {
        [NoMappingToDatabase]
        public IRepository Repository { get; set; }

        public DateTime Start { get; set; }
        public TimeSpan ChunkPeriod { get; set; }
        public IPermissionWindow PermissionWindow { get; set; }
        public ICacheProgress CacheProgress { get; set; }
        public bool IsRetry { get; private set; }

        public DateTime End
        {
            get { return Start.Add(ChunkPeriod); }
        }
        
        public BackfillCacheFetchRequest(IRepository repository,DateTime start)
        {
            Repository = repository;
            Start = start;
            IsRetry = false;
        }
        
        public void SaveCacheFillProgress(DateTime cacheFillProgress)
        {
            // do nothing, backfill should not affect the cache
        }

        public void RequestFailed(Exception e)
        {
            // do nothing, backfill should not affect the cache
        }

        public void RequestSucceeded()
        {
            // do nothing, backfill should not affect the cache
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