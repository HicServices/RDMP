using CachingEngine.Layouts;
using CatalogueLibrary.Data;

namespace DataLoadEngine.DataProvider.FromCache
{
    public interface ICachedDataProvider : IPluginDataProvider
    {
        ILoadProgress LoadProgress { get; set; }
    }
}