using System.ComponentModel.Composition;
using System.IO;

namespace CachingEngine.Layouts
{
    /// <summary>
    /// Translates a root cache directory (usually .\Data\Cache e.g. C:\temp\DemographyLoading\Data\Cache) into a subdirectory based on arbitrary logic. For example
    /// you might have CacheProgress that are tied to specific healthboards and you want subdirectories T and F in your Cache directory.
    /// </summary>
    [InheritedExport(typeof(ILoadCachePathResolver))]
    public interface ILoadCachePathResolver
    {
        DirectoryInfo GetLoadCacheDirectory(DirectoryInfo cacheRootDirectory);
    }
}