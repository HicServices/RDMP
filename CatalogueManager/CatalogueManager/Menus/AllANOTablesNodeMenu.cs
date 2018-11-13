using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllANOTablesNodeMenu:RDMPContextMenuStrip
    {
        public AllANOTablesNodeMenu(RDMPContextMenuStripArgs args, AllANOTablesNode node): base(args, node)
        {
            Add(new ExecuteCommandCreateNewANOTable(_activator));
            
            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                typeof(ANOStore.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" });

            Add(new ExecuteCommandExportObjectsToFileUI(_activator,_activator.CoreChildProvider.AllANOTables));
        }
    }
}
