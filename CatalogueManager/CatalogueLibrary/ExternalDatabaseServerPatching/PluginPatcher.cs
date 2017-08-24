using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public abstract class PluginPatcher : IPluginPatcher
    {
        protected readonly CatalogueRepository CatalogueRepository;

        protected PluginPatcher(CatalogueRepository catalogueRepository)
        {
            CatalogueRepository = catalogueRepository;
        }

        public abstract IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly);
    }
}