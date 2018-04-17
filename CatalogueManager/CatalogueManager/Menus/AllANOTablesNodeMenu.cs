using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllANOTablesNodeMenu:RDMPContextMenuStrip
    {
        public AllANOTablesNodeMenu(RDMPContextMenuStripArgs args, AllANOTablesNode node)
            : base(args, null)
        {
            Add(new ExecuteCommandCreateNewANOTable(_activator));
            
            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                typeof(ANOStore.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" });

            Add(new ExecuteCommandExportObjectsToFile(_activator,_activator.CoreChildProvider.AllANOTables));
        }
    }
}
