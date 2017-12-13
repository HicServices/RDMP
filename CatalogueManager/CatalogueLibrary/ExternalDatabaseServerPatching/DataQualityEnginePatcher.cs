using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
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
