using CatalogueLibrary.Data;

namespace CatalogueLibrary.Repositories.Managers
{
    public interface IPluginManager
    {
        Plugin[] GetCompatiblePlugins();
    }
}