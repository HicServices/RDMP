using System;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public class IdentifierDumpDatabasePatcher : IPatcher
    {
        private readonly CatalogueRepository _catalogueRepository;

        public IdentifierDumpDatabasePatcher(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
        {
            var uniqueServers =
                _catalogueRepository.GetAllObjects<TableInfo>() //get all tables
                    .Where(t=>t.IdentifierDumpServer_ID.HasValue) //which have a dump server
                    .Select(t2 => t2.IdentifierDumpServer_ID.Value) //get unique server IDs among them
                    .Distinct()
                    .Select(_catalogueRepository.GetObjectByID<ExternalDatabaseServer>)//Get the external database server object
                    .Cast<IExternalDatabaseServer>()
                    .ToArray();

            hostAssembly = Assembly.Load("IdentifierDump");
            dbAssembly = Assembly.Load("IdentifierDump.Database");

            return uniqueServers;
        }
    }
}