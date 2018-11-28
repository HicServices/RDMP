using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllRDMPRemotesNodeMenu : RDMPContextMenuStrip
    {
        public AllRDMPRemotesNodeMenu(RDMPContextMenuStripArgs args, AllRDMPRemotesNode node)
            : base(args, node)
        {
            Add(new ExecuteCommandCreateNewRemoteRDMP(_activator));
        }
    }
}