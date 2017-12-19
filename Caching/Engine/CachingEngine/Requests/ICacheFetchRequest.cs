using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using MapsDirectlyToDatabaseTable;

namespace CachingEngine.Requests
{
    /// <summary>
    /// An instruction for an ICacheSource to request a specific date/time range of data.  The ICacheFetchRequest will also be available in the ICacheChunk which is the T 
    /// flow object of a caching pipeline (See CachingPipelineUseCase) this means that the destination can ensure that the data read goes into the correct sections of the 
    /// file system.
    /// </summary>
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