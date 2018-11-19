using System.Reflection;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Records that the host assembly ANOStore stores types for the database assembly 
    /// </summary>
    public class ANOStoreDatabasePatcher:IPatcher
    {
        /// <inheritdoc/>
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("ANOStore");
        }

        /// <inheritdoc/>
        public Assembly GetDbAssembly()
        {
            return Assembly.Load("ANOStore.Database");
        }
    }
}