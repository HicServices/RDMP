using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Microsoft.SqlServer.Management.Smo;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public class LoggingDatabasePatcher: IPatcher
    {
        private readonly CatalogueRepository _catalogueRepository;

        public LoggingDatabasePatcher(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
        {
            // Determine which databases are to be updated
            var serverIDsToCheck = new List<int>();

            var catalogues = _catalogueRepository.GetAllCatalogues();
            var defaults = new ServerDefaults(_catalogueRepository);

            // Add the default servers
            var defaultLiveServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
            if (defaultLiveServer != null)
                serverIDsToCheck.Add(defaultLiveServer.ID);

            var defaultTestServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.TestLoggingServer_ID);
            if (defaultTestServer != null)
                serverIDsToCheck.Add(defaultTestServer.ID);

            // Harvest the logging server IDs from all the catalogues
            serverIDsToCheck.AddRange(catalogues.Where(c => c.LiveLoggingServer_ID.HasValue).Select(c => c.LiveLoggingServer_ID.Value));
            serverIDsToCheck.AddRange(catalogues.Where(c => c.TestLoggingServer_ID.HasValue).Select(c => c.TestLoggingServer_ID.Value));
            
            //distinct them
            serverIDsToCheck = serverIDsToCheck.Distinct().ToList();

            hostAssembly = typeof (LogManager).Assembly;
            dbAssembly = Assembly.Load("HIC.Logging.Database");

            //now check and patch each in turn
            return _catalogueRepository.GetAllObjectsInIDList<ExternalDatabaseServer>(serverIDsToCheck).ToArray();
        }
    }
}