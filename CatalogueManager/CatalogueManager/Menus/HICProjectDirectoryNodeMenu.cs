using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class HICProjectDirectoryNodeMenu : RDMPContextMenuStrip
    {
        public HICProjectDirectoryNodeMenu(RDMPContextMenuStripArgs args,HICProjectDirectoryNode node) : base(args, node)
        {
            Add(new ExecuteCommandOpenInExplorer(_activator, node.GetDirectoryInfoIfAny()));
            Add(new ExecuteCommandChooseHICProjectDirectory(_activator, node.LoadMetadata));
        }
    }
}