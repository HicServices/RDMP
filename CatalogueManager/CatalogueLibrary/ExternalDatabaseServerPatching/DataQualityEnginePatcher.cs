using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Documents the relationship between the patching and database assemblies of the DQE
    /// </summary>
    public class DataQualityEnginePatcher : IPatcher
    {
        public Assembly GetHostAssembly()
        {
            return Assembly.Load("DataQualityEngine");
        }

        public Assembly GetDbAssembly()
        {
            return Assembly.Load("DataQualityEngine.Database");
        }
    }
}
