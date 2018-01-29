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
    /// <summary>
    /// Documents the relationship between the patching and database assemblies of the logging database.  This is a relational database in which events are logged
    /// in a hierarchical way (See HIC.Logging.LogManager)
    /// </summary>
    public class LoggingDatabasePatcher: IPatcher
    {
        public Assembly GetHostAssembly()
        {
            return typeof(LogManager).Assembly;
        }

        public Assembly GetDbAssembly()
        {
            return Assembly.Load("HIC.Logging.Database");
        }
    }
}