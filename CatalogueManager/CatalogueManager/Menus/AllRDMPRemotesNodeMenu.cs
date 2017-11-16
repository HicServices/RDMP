using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    public class AllRDMPRemotesNodeMenu : RDMPContextMenuStrip
    {
        public AllRDMPRemotesNodeMenu(IActivateItems activator, AllRDMPRemotesNode node)
            : base(activator, null)
        {
            Add(new ExecuteCommandCreateNewRemoteRDMP(activator));
        }
    }
}