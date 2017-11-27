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
        public LoadMetadataScheduleNodeMenu(RDMPContextMenuStripArgs args, LoadMetadataScheduleNode schedulingNode) : base(args,null)
        {
            Add(new ExecuteCommandCreateNewLoadProgress(_activator, schedulingNode.LoadMetadata));
            Add(new ExecuteCommandCreateNewLoadPeriodically(_activator, schedulingNode.LoadMetadata));
        }

    }
}