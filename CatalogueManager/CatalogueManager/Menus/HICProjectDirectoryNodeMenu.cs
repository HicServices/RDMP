using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;

namespace CatalogueManager.Menus
{
    public class HICProjectDirectoryNodeMenu : RDMPToolStripMenuItem
    {
        public HICProjectDirectoryNodeMenu(IActivateItems activator, HICProjectDirectoryNode node) : base(activator, null)
        {
            Add(new ExecuteCommandOpenInExplorer(_activator, node.GetDirectoryInfoIfAny()));
            Add(new ExecuteCommandChooseHICProjectDirectory(_activator, node.LoadMetadata));
        }
    }
}