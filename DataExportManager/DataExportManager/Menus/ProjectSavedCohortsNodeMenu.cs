using CatalogueManager.Menus;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;

namespace DataExportManager.Menus
{
    class ProjectSavedCohortsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectSavedCohortsNodeMenu(RDMPContextMenuStripArgs args, ProjectSavedCohortsNode savedCohortsNode): base(args, null)
        {
            Add(new ExecuteCommandCreateNewCohortFromFile(_activator).SetTarget(savedCohortsNode.Project));
            Add(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator).SetTarget(savedCohortsNode.Project));
            Add(new ExecuteCommandCreateNewCohortFromCatalogue(_activator).SetTarget(savedCohortsNode.Project));
        }
    }
}
