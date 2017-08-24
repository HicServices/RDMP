using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public class QueryCachingDatabasePatcher:IPatcher
    {
        private CatalogueRepository _catalogueRepository;

        public QueryCachingDatabasePatcher(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
        {
            var queryCachingDatabases = _catalogueRepository.GetAllObjects<ExternalDatabaseServer>()
                .Where(s => s.CreatedByAssembly == "QueryCaching.Database").ToArray();

            hostAssembly = Assembly.Load("QueryCaching");
            dbAssembly = Assembly.Load("QueryCaching.Database");
            
            return queryCachingDatabases;
        }
    }
}