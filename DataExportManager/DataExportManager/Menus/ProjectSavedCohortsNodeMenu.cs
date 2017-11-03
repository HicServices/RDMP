using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Menus.MenuItems;
using DataExportManager.Collections.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;

namespace DataExportManager.Menus
{
    public class ProjectSavedCohortsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectSavedCohortsNodeMenu(IActivateItems activator, ProjectSavedCohortsNode savedCohortsNode): base(activator, null)
        {
            Add(new ExecuteCommandImportFileAsNewCohort(activator).SetTarget(savedCohortsNode.Project));
            Add(new ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults(activator).SetTarget(savedCohortsNode.Project));
        }
    }
}
