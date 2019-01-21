using CatalogueLibrary.Nodes.CohortNodes;
using CatalogueManager.Menus;
using CohortManager.CommandExecution.AtomicCommands;

namespace CohortManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class AllProjectCohortIdentificationConfigurationsNodeMenu : RDMPContextMenuStrip
    {
        public AllProjectCohortIdentificationConfigurationsNodeMenu(RDMPContextMenuStripArgs args, AllProjectCohortIdentificationConfigurationsNode node): base(args, node)
        {
            Add(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));
        }
    }
}