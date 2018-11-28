using System.Reflection;
using HIC.Logging;


namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Documents the relationship between the patching and database assemblies of the logging database.  This is a relational database in which events are logged
    /// in a hierarchical way (See HIC.Logging.LogManager)
    /// </summary>
    public class LoggingDatabasePatcher: IPatcher
    {
        /// <inheritdoc/>
        public Assembly GetHostAssembly()
        {
            return typeof(LogManager).Assembly;
        }

        /// <inheritdoc/>
        public Assembly GetDbAssembly()
        {
            return Assembly.Load("HIC.Logging.Database");
        }
    }
}