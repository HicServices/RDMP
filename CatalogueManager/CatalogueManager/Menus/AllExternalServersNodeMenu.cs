using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AllExternalServersNodeMenu : ContextMenuStrip
    {
        private readonly IActivateItems _activator;

        public AllExternalServersNodeMenu(IActivateItems activator)
        {
            var overlayProvider = new IconOverlayProvider();
            var iconProvider = new ExternalDatabaseServerStateBasedIconProvider(overlayProvider);

            var assemblyDictionary = new Dictionary<ServerDefaults.PermissableDefaults, Assembly>();
            
            Items.Add(new ToolStripMenuItem("Create New External Server Reference",activator.CoreIconProvider.GetImage(RDMPConcept.ExternalDatabaseServer,OverlayKind.Add),CreateNewBlankServer));
            Items.Add(new ToolStripSeparator());

            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.DQE, typeof(DataQualityEngine.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID, typeof(QueryCaching.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, typeof(HIC.Logging.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID, typeof(IdentifierDump.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.ANOStore, typeof(ANOStore.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.CohortIdentificationQueryCachingServer_ID, typeof(QueryCaching.Database.Class1).Assembly);

            _activator = activator;

            foreach (KeyValuePair<ServerDefaults.PermissableDefaults, Assembly> kvp in assemblyDictionary)
            {
                string name = GetHumanReadableNameFromPermissableDefault(kvp.Key);

                var defaultToSet = kvp.Key;
                var databaseAssembly = kvp.Value;
                
                var basicIcon = iconProvider.GetIconForAssembly(kvp.Value);
                var addIcon = overlayProvider.GetOverlayNoCache(basicIcon, OverlayKind.Add);

                Items.Add(new ToolStripMenuItem("Create New '" + name + "' Server...",addIcon, (s, e) => CreateNewExternalServer(defaultToSet, databaseAssembly)));
            }
               
        }

        private void CreateNewBlankServer(object sender, EventArgs e)
        {
            var newServer = new ExternalDatabaseServer(_activator.RepositoryLocator.CatalogueRepository, "New ExternalDatabaseServer " + Guid.NewGuid());
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(newServer));
        }

        private string GetHumanReadableNameFromPermissableDefault(ServerDefaults.PermissableDefaults def)
        {
            return UsefulStuff.PascalCaseStringToHumanReadable(def.ToString().Replace("_ID", "").Replace("Live", "").Replace("ANO","Anonymisation"));
        }

        private void CreateNewExternalServer(ServerDefaults.PermissableDefaults defaultToSet, Assembly databaseAssembly)
        {
            //user wants to create a new server e.g. a new Logging server

            //do we already have a default server for this?
            var defaults = new ServerDefaults(_activator.RepositoryLocator.CatalogueRepository);
            var existingDefault = defaults.GetDefaultFor(defaultToSet);


            //create the new server
            var newServer = CreatePlatformDatabase.CreateNewExternalServer(
                _activator.RepositoryLocator.CatalogueRepository,

                //if we already have an existing default of this type then don't set the default yet
                existingDefault  != null? ServerDefaults.PermissableDefaults.None : defaultToSet,
                databaseAssembly);

            //user cancelled creating a server
            if (newServer == null)
                return;

            if (existingDefault != null)
            {
                if(MessageBox.Show(
                    "Would you like the new default '" + defaultToSet +
                    "' server to the newly created server? The current default is '" + existingDefault + "'",
                    "Overwrite Default", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    defaults.SetDefault(defaultToSet,newServer);
            }

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(newServer));
        }

    }
}