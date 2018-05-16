using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllPermissionWindowsNodeMenu:RDMPContextMenuStrip
    {
        public AllPermissionWindowsNodeMenu(RDMPContextMenuStripArgs args, AllPermissionWindowsNode node): base(args, node)
        {
            Add(new ExecuteCommandCreateNewPermissionWindow(_activator));
        }
    }
}
