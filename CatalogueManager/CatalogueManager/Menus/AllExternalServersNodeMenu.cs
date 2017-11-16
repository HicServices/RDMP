using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AllExternalServersNodeMenu : RDMPContextMenuStrip
    {
        public AllExternalServersNodeMenu(IActivateItems activator, AllExternalServersNode node) : base(activator,null)
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

            var factory = new AtomicCommandUIFactory(activator.CoreIconProvider);

            var types = activator.RepositoryLocator.CatalogueRepository.MEF
                .GetTypes<IAtomicCommand>().Where(t =>
                    typeof (PluginDatabaseAtomicCommand).IsAssignableFrom(t));

            foreach (var type in types)
            {
                var instance = new ObjectConstructor().Construct(type,activator.RepositoryLocator);
                Items.Add(factory.CreateMenuItem((IAtomicCommand)instance));
            }
               
        }

        private void CreateNewBlankServer(object sender, EventArgs e)
        {
            var newServer = new ExternalDatabaseServer(_activator.RepositoryLocator.CatalogueRepository, "New ExternalDatabaseServer " + Guid.NewGuid());
            Publish(newServer);
        }

        private string GetHumanReadableNameFromPermissableDefault(ServerDefaults.PermissableDefaults def)
        {
            return UsefulStuff.PascalCaseStringToHumanReadable(def.ToString().Replace("_ID", "").Replace("Live", "").Replace("ANO","Anonymisation"));
        }

        private void CreateNewExternalServer(ServerDefaults.PermissableDefaults defaultToSet, Assembly databaseAssembly)
        {
            new ExecuteCommandCreateNewExternalDatabaseServer(_activator, databaseAssembly, defaultToSet).Execute();
        }

    }
}