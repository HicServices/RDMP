using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public interface IExternalDatabaseServerProvider
    {
        IExternalDatabaseServer GetExternalDatabaseServerWithID(int id);
    }
}