using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.Refreshing;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AllExternalServersNodeMenu : RDMPContextMenuStrip
    {
        public AllExternalServersNodeMenu(RDMPContextMenuStripArgs args, AllExternalServersNode node) : base(args,null)
        {
            var overlayProvider = new IconOverlayProvider();
            var iconProvider = new ExternalDatabaseServerStateBasedIconProvider(overlayProvider);

            var assemblyDictionary = new Dictionary<ServerDefaults.PermissableDefaults, Assembly>();

            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null,ServerDefaults.PermissableDefaults.None));

            Items.Add(new ToolStripSeparator());

            //Add(new ExecuteCommandConfigureDefaultServers());

            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.DQE, typeof(DataQualityEngine.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID, typeof(QueryCaching.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, typeof(HIC.Logging.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID, typeof(IdentifierDump.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.ANOStore, typeof(ANOStore.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.CohortIdentificationQueryCachingServer_ID, typeof(QueryCaching.Database.Class1).Assembly);

            foreach (KeyValuePair<ServerDefaults.PermissableDefaults, Assembly> kvp in assemblyDictionary)
            {
                string name = GetHumanReadableNameFromPermissableDefault(kvp.Key);

                var defaultToSet = kvp.Key;
                var databaseAssembly = kvp.Value;
                
                var basicIcon = iconProvider.GetIconForAssembly(kvp.Value);
                var addIcon = overlayProvider.GetOverlayNoCache(basicIcon, OverlayKind.Add);

                Items.Add(new ToolStripMenuItem("Create New '" + name + "' Server...",addIcon, (s, e) => CreateNewExternalServer(defaultToSet, databaseAssembly)));
            }

            var types = _activator.RepositoryLocator.CatalogueRepository.MEF
                .GetTypes<IAtomicCommand>().Where(t =>
                    typeof (PluginDatabaseAtomicCommand).IsAssignableFrom(t));

            foreach (var type in types)
            {
                var instance = new ObjectConstructor().Construct(type,_activator.RepositoryLocator);
                Add((IAtomicCommand)instance);
            }
               
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