using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllServersNodeMenu : RDMPContextMenuStrip
    {
        public AllServersNodeMenu(RDMPContextMenuStripArgs args, AllServersNode o) : base(args, o)
        {
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, false));
            Add(new ExecuteCommandBulkImportTableInfos(_activator));
        }
    }
}