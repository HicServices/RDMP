using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ServerDefaults
    /// </summary>
    public interface IServerDefaults
    {
        IExternalDatabaseServer GetDefaultFor(ServerDefaults.PermissableDefaults field);
        CatalogueRepository Repository { get; }
    }
}