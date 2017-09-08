using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
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
            _assemblyToIconDictionary.Add(typeof (IdentifierDump.Database.Class1).Assembly.GetName().Name, CatalogueIcons.ExternalDatabaseServer_IdentifierDump);
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
            var dumpServerUsage = o as IdentifierDumpServerUsageNode;

            if (dumpServerUsage != null)
                server = dumpServerUsage.IdentifierDumpServer;

            //if its not a server we aren't responsible for providing an icon for it
            if (server == null)
                return null;

            //the untyped server icon (e.g. user creates a reference to a server that he is going to use but isn't created/managed by a .Datbase assembly)
            var toReturn = _default;

            //if it is a .Database assembly managed database then use the appropriate icon instead (ANO, LOG, IDD etc)
            if (!string.IsNullOrWhiteSpace(server.CreatedByAssembly) && _assemblyToIconDictionary.ContainsKey(server.CreatedByAssembly))
                toReturn = _assemblyToIconDictionary[server.CreatedByAssembly];
                
            //add the database type overlay
            toReturn = _overlayProvider.GetOverlay(toReturn, _typeSpecificIconsProvider.GetOverlay(server.DatabaseType));

            if (dumpServerUsage != null)
                toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Link);

            return toReturn;
        }
    }
}