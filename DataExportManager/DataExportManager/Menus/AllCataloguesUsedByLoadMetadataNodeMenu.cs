using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Menus;

namespace DataExportManager.Menus
{
    class AllCataloguesUsedByLoadMetadataNodeMenu:RDMPContextMenuStrip
    {
        public AllCataloguesUsedByLoadMetadataNodeMenu(RDMPContextMenuStripArgs args, AllCataloguesUsedByLoadMetadataNode node) : base(args, node)
        {
            Add(new ExecuteCommandAssociateCatalogueWithLoadMetadata(_activator, node.LoadMetadata));
        }
    }
}
