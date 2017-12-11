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
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("IdentifierDump");
        }

        public Assembly GetDbAssembly()
        {
            return Assembly.Load("IdentifierDump.Database");
        }
    }
}