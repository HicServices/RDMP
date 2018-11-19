using System.Reflection;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Documents the relationship between the patching and database assemblies of the DQE
    /// </summary>
    public class DataQualityEnginePatcher : IPatcher
    {
        /// <inheritdoc/>
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("DataQualityEngine");
        }

        /// <inheritdoc/>
        public Assembly GetDbAssembly()
        {
            return Assembly.Load("DataQualityEngine.Database");
        }
    }
}
