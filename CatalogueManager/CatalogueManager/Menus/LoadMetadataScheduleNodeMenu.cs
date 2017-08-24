using System.Windows.Forms;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class LoadMetadataScheduleNodeMenu : ContextMenuStrip
    {
        public LoadMetadataScheduleNodeMenu(IActivateItems activator, LoadMetadataScheduleNode schedulingNode)
        {
            var factory = new AtomicCommandUIFactory(activator.CoreIconProvider);
            Items.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewLoadProgress(activator, schedulingNode.LoadMetadata)));
            Items.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewLoadPeriodically(activator, schedulingNode.LoadMetadata)));
        }

    }
}