using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    class AllRDMPRemotesNodeMenu : RDMPContextMenuStrip
    {
        public AllRDMPRemotesNodeMenu(RDMPContextMenuStripArgs args, AllRDMPRemotesNode node)
            : base(args, null)
        {
            Add(new ExecuteCommandCreateNewRemoteRDMP(_activator));
        }
    }
}