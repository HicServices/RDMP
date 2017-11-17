using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    public class DataQualityEnginePatcher : IPatcher
    {
        private readonly IServerDefaults _serverDefaults;

        public DataQualityEnginePatcher(IServerDefaults serverDefaults)
        {
            _serverDefaults = serverDefaults;
        }

        public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
        {
            var dqe = _serverDefaults.GetDefaultFor(ServerDefaults.PermissableDefaults.DQE);

            hostAssembly = Assembly.Load("DataQualityEngine");
            dbAssembly = Assembly.Load("DataQualityEngine.Database");

            if (dqe == null)
                return null;

            return new[] {dqe};
        }
    }
}
