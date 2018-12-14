using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    class HICProjectDirectoryNodeMenu : RDMPContextMenuStrip
    {
        public HICProjectDirectoryNodeMenu(RDMPContextMenuStripArgs args,HICProjectDirectoryNode node) : base(args, node)
        {
            ReBrandActivateAs("Open In Explorer",RDMPConcept.CatalogueFolder);
            Add(new ExecuteCommandChooseHICProjectDirectory(_activator, node.LoadMetadata));
        }
    }
}