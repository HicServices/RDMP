using CatalogueLibrary.Nodes.CohortNodes;
using CatalogueManager.Menus;
using CohortManager.CommandExecution.AtomicCommands;

namespace CohortManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class AllFreeCohortIdentificationConfigurationsNodeMenu : RDMPContextMenuStrip
    {
        public AllFreeCohortIdentificationConfigurationsNodeMenu(RDMPContextMenuStripArgs args, AllFreeCohortIdentificationConfigurationsNode node)
            : base(args, node)
        {
            Add(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));
        }
    }
}