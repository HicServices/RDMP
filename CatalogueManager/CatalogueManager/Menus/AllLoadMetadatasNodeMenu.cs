using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllLoadMetadatasNodeMenu : RDMPContextMenuStrip
    {
        public AllLoadMetadatasNodeMenu(RDMPContextMenuStripArgs args, AllLoadMetadatasNode allLoadMetadatasNode) : base(args, allLoadMetadatasNode)
        {
            Add(new ExecuteCommandCreateNewLoadMetadata(_activator));
        }

    }
}