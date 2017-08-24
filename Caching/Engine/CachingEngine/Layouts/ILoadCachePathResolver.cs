using System.ComponentModel.Composition;
using System.IO;

namespace CachingEngine.Layouts
{
    // to get paths for dataset-specific instances
    [InheritedExport(typeof(ILoadCachePathResolver))]
    public interface ILoadCachePathResolver
    {
        DirectoryInfo GetLoadCacheDirectory(DirectoryInfo cacheRootDirectory);
    }
}