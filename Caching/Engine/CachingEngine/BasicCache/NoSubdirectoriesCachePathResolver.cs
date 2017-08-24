using System.IO;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CatalogueLibrary.Data.Cache;

namespace CachingEngine.BasicCache
{
    public class NoSubdirectoriesCachePathResolver : ILoadCachePathResolver
    {
        public DirectoryInfo GetLoadCacheDirectory(DirectoryInfo cacheRootDirectory)
        {
            return cacheRootDirectory;
        }
    }
}