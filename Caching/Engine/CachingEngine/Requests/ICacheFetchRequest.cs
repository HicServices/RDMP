using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using MapsDirectlyToDatabaseTable;

namespace CachingEngine.Requests
{
    public interface ICacheFetchRequest
    {
        IRepository Repository { get; set; }

        void SaveCacheFillProgress(DateTime cacheFillProgress);
        
        DateTime Start { get; set; }
        DateTime End { get; }
        TimeSpan ChunkPeriod { get; set; }
        IPermissionWindow PermissionWindow { get; set; }
        ICacheProgress CacheProgress { get; set; }
        bool IsRetry { get; }

        void RequestFailed(Exception e);
        void RequestSucceeded();
        ICacheFetchRequest GetNext();
    }
}