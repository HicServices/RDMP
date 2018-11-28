using System.Reflection;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Documents the relationship between the patching and database assemblies of the Identifier Dump Database (this db stores identifiable data that is 
    /// dropped out of loads in the DLE i.e. dilution and column dropping to produce a semi anonymous dataset for data analysts to work on).
    /// </summary>
    public class IdentifierDumpDatabasePatcher : IPatcher
    {
        /// <inheritdoc/>
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("IdentifierDump");
        }

        /// <inheritdoc/>
        public Assembly GetDbAssembly()
        {
            return Assembly.Load("IdentifierDump.Database");
        }
    }
}