using System;
using System.ComponentModel.Composition;
using System.IO;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;

namespace CachingEngine.BasicCache
{
    [Export (typeof(ICacheLayout))]
    public class BasicCacheLayout:CacheLayout
    {
        public BasicCacheLayout(DirectoryInfo rootCacheDirectory)
            : base(rootCacheDirectory, "yyyy-MM-dd", CacheArchiveType.None, CacheFileGranularity.Day, new NoSubdirectoriesCachePathResolver())
        {
            
        }
    }
}
