using System.IO;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CatalogueLibrary.Data.Cache;

namespace CachingEngine.BasicCache
{
    /// <summary>
    /// Translates a root cache directory (usually .\Data\Cache e.g. C:\temp\DemographyLoading\Data\Cache)
    /// </summary>
    public class NoSubdirectoriesCachePathResolver : ILoadCachePathResolver
    {
        public DirectoryInfo GetLoadCacheDirectory(DirectoryInfo cacheRootDirectory)
        {
            return cacheRootDirectory;
        }
    }
}