using CatalogueManager.Menus;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    class ProjectCohortIdentificationConfigurationAssociationsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectCohortIdentificationConfigurationAssociationsNodeMenu(RDMPContextMenuStripArgs args, ProjectCohortIdentificationConfigurationAssociationsNode node)
            : base(args, node)
        {
            Add(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(node.Project));

        }
    }
}
