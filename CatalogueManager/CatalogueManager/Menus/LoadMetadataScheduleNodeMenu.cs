using System.Windows.Forms;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class LoadMetadataScheduleNodeMenu : RDMPContextMenuStrip
    {
        public LoadMetadataScheduleNodeMenu(IActivateItems activator, LoadMetadataScheduleNode schedulingNode) : base(activator,null)
        {
            Add(new ExecuteCommandCreateNewLoadProgress(activator, schedulingNode.LoadMetadata));
            Add(new ExecuteCommandCreateNewLoadPeriodically(activator, schedulingNode.LoadMetadata));
        }

    }
}