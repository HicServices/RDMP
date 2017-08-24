using System;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public class ANOStoreDatabasePatcher:IPatcher
    {
        private readonly IRepository _catalogueRepository;
        

        public ANOStoreDatabasePatcher(IRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
        {
            var uniqueServers =
                _catalogueRepository.GetAllObjects<ANOTable>() //get all the ANOTables
                    .Select(a => a.Server_ID) //get unique server IDs among ANOTables
                    .Distinct()
                    .Select(_catalogueRepository.GetObjectByID<ExternalDatabaseServer>);//Get the external database server object

            var d = new ServerDefaults((CatalogueRepository)_catalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.ANOStore);

            if(d != null)
                uniqueServers = uniqueServers.Union(new[] { (ExternalDatabaseServer)d });
            
            hostAssembly = Assembly.Load("ANOStore");
            dbAssembly = Assembly.Load("ANOStore.Database");
            
            return  uniqueServers.ToArray();
        }
    }
}