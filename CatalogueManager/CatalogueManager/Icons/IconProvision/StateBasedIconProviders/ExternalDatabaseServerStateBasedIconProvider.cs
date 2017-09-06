using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExternalDatabaseServerStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private Bitmap _default;

        Dictionary<string,Bitmap> _assemblyToIconDictionary = new Dictionary<string, Bitmap>();
        private DatabaseTypeIconProvider _typeSpecificIconsProvider;

        public ExternalDatabaseServerStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _default = CatalogueIcons.ExternalDatabaseServer;

            _assemblyToIconDictionary.Add(typeof (DataQualityEngine.Database.Class1).Assembly.GetName().Name,CatalogueIcons.ExternalDatabaseServer_DQE);
            _assemblyToIconDictionary.Add(typeof(ANOStore.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_ANO);
            _assemblyToIconDictionary.Add(typeof(IdentifierDump.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_IdentifierDump);
            _assemblyToIconDictionary.Add(typeof(QueryCaching.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_Cache);
            _assemblyToIconDictionary.Add(typeof(HIC.Logging.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_Logging);

            _typeSpecificIconsProvider = new DatabaseTypeIconProvider();
        }

        public Bitmap GetIconForAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            if (_assemblyToIconDictionary.ContainsKey(assemblyName))
                return _assemblyToIconDictionary[assemblyName];

            throw new ArgumentOutOfRangeException("Did not know what icon to use for '" + assemblyName + "'.dll");
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var server = o as ExternalDatabaseServer;

            if (server == null)
                return null;

            var toReturn = _default;

            if (!string.IsNullOrWhiteSpace(server.CreatedByAssembly) && _assemblyToIconDictionary.ContainsKey(server.CreatedByAssembly))
                toReturn = _assemblyToIconDictionary[server.CreatedByAssembly];

            return _overlayProvider.GetOverlay(toReturn,_typeSpecificIconsProvider.GetOverlay(server.DatabaseType));
        }
    }
}