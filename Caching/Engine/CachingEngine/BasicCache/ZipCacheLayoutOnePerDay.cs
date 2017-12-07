using System.ComponentModel.Composition;
using System.IO;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CatalogueLibrary.Data.DataLoad;

namespace CachingEngine.BasicCache
{
    /// <summary>
    /// Alternative cache layout to BasicCacheLayout in which files are expected to be in a zip file instead of a directory (e.g. in .\Data\Cache\2001-01-01.zip, .\Data\Cache\2001-01-02.zip etc)
    /// </summary>
    public class ZipCacheLayoutOnePerDay : CacheLayout
    {
        public ZipCacheLayoutOnePerDay(DirectoryInfo rootCacheDirectory,ILoadCachePathResolver resolver)
            : base(rootCacheDirectory, "yyyy-MM-dd", CacheArchiveType.Zip, CacheFileGranularity.Day,resolver)
        {
            
        }
            
    }
}