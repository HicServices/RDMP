using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExternalDatabaseServerStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _default;

        Dictionary<string,Bitmap> _assemblyToIconDictionary = new Dictionary<string, Bitmap>();

        public ExternalDatabaseServerStateBasedIconProvider()
        {
            _default = CatalogueIcons.ExternalDatabaseServer;

            _assemblyToIconDictionary.Add(typeof (DataQualityEngine.Database.Class1).Assembly.GetName().Name,CatalogueIcons.ExternalDatabaseServer_DQE);
            _assemblyToIconDictionary.Add(typeof(ANOStore.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_ANO);
            _assemblyToIconDictionary.Add(typeof(IdentifierDump.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_IdentifierDump);
            _assemblyToIconDictionary.Add(typeof(QueryCaching.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_Cache);
            _assemblyToIconDictionary.Add(typeof(HIC.Logging.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_Logging);
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var server = o as ExternalDatabaseServer;

            if (server == null)
                return null;
            
            if (string.IsNullOrWhiteSpace(server.CreatedByAssembly))
                return _default;

            if (_assemblyToIconDictionary.ContainsKey(server.CreatedByAssembly))
                return _assemblyToIconDictionary[server.CreatedByAssembly];

            return _default;
        }
    }
}