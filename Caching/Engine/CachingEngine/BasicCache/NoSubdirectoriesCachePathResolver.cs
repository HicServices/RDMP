using System.IO;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CatalogueLibrary.Data.Cache;

namespace CachingEngine.BasicCache
{
    /// <summary>
    /// Basic case of ILoadCachePathResolver in which the path .\Data\Cache is what is returned unchanged i.e. no subdirectories
    /// </summary>
    public class NoSubdirectoriesCachePathResolver : ILoadCachePathResolver
    {
        public DirectoryInfo GetLoadCacheDirectory(DirectoryInfo cacheRootDirectory)
        {
            return cacheRootDirectory;
        }
    }
}