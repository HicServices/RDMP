using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public class QueryCachingDatabasePatcher:IPatcher
    {
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("QueryCaching");
        }

        public Assembly GetDbAssembly()
        {
            return Assembly.Load("QueryCaching.Database");
        }
    }
}