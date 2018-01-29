using CachingEngine.Layouts;
using CatalogueLibrary.Data;

namespace DataLoadEngine.DataProvider.FromCache
{
    /// <summary>
    /// MEF discoverable plugin implementation of IDataProvider intended to read from the ILoadProgress cache directory (e.g. and unzip into 
    /// HICProjectDirectoy.ForLoading) during data loading.  This is only required if you want to be able to change the way you interact with
    /// your cache (e.g. if you have a proprietary archive format).  In general you should try to ensure any caches you create are compatible with
    /// BasicCacheDataProvider and just use that instead.
    /// </summary>
    public interface ICachedDataProvider : IPluginDataProvider
    {
        ILoadProgress LoadProgress { get; set; }
    }
}