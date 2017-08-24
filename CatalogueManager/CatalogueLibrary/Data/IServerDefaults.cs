using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data
{
    public interface IServerDefaults
    {
        IExternalDatabaseServer GetDefaultFor(ServerDefaults.PermissableDefaults field);
        CatalogueRepository Repository { get; }
    }
}