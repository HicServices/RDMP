using System.Windows.Forms;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class LoadMetadataScheduleNodeMenu : RDMPContextMenuStrip
    {
        public LoadMetadataScheduleNodeMenu(RDMPContextMenuStripArgs args, LoadMetadataScheduleNode schedulingNode): base(args, schedulingNode)
        {
            Add(new ExecuteCommandCreateNewLoadProgress(_activator, schedulingNode.LoadMetadata));
        }

    }
}