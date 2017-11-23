using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;

namespace CatalogueManager.Menus
{
    public class HICProjectDirectoryNodeMenu : RDMPContextMenuStrip
    {
        public HICProjectDirectoryNodeMenu(IActivateItems activator, HICProjectDirectoryNode node, RDMPCollectionCommonFunctionality collection) : base(activator, null, collection)
        {
            Add(new ExecuteCommandOpenInExplorer(_activator, node.GetDirectoryInfoIfAny()));
            Add(new ExecuteCommandChooseHICProjectDirectory(_activator, node.LoadMetadata));
        }
    }
}