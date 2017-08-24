using System.ComponentModel.Composition;
using System.IO;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CatalogueLibrary.Data.DataLoad;

namespace CachingEngine.BasicCache
{
    [Export(typeof(ICacheLayout))]
    public class ZipCacheLayoutOnePerDay : CacheLayout
    {
        public ZipCacheLayoutOnePerDay(DirectoryInfo rootCacheDirectory,ILoadCachePathResolver resolver)
            : base(rootCacheDirectory, "yyyy-MM-dd", CacheArchiveType.Zip, CacheFileGranularity.Day,resolver)
        {
            
        }
            
    }
}