using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public interface IPatcher
    {
        IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly);
    }
}