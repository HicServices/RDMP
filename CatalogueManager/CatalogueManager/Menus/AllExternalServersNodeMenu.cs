using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.CommandExecution.AtomicCommands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AllExternalServersNodeMenu : RDMPContextMenuStrip
    {
        public AllExternalServersNodeMenu(RDMPContextMenuStripArgs args, AllExternalServersNode node) : base(args,null)
        {
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
                Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, kvp.Value, kvp.Key));


            AddAll<PluginAtomicCommand>();
        }
    }
}