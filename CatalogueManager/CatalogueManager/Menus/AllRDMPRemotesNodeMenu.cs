using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    public class AllRDMPRemotesNodeMenu : RDMPContextMenuStrip
    {
        public AllRDMPRemotesNodeMenu(IActivateItems activator, AllRDMPRemotesNode node, RDMPCollectionCommonFunctionality collection)
            : base(activator, null, collection)
        {
            Add(new ExecuteCommandCreateNewRemoteRDMP(activator));
        }
    }
}