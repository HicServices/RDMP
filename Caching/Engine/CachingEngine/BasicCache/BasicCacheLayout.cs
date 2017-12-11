using System;
using System.ComponentModel.Composition;
using System.IO;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;

namespace CachingEngine.BasicCache
{
    /// <summary>
    /// Specifies how files are laid out by date.  This is the default implementation in which the cache root directory (usually .\Data\Cache) is populated with folders
    /// yyyy-MM-dd which contains unzipped lists of files for that day.
    /// </summary>
    [Export (typeof(ICacheLayout))]
    public class BasicCacheLayout:CacheLayout
    {
        public BasicCacheLayout(DirectoryInfo rootCacheDirectory)
            : base(rootCacheDirectory, "yyyy-MM-dd", CacheArchiveType.None, CacheFileGranularity.Day, new NoSubdirectoriesCachePathResolver())
        {
            
        }
    }
}
