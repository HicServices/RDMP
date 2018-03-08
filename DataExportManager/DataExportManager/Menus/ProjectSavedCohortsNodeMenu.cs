using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Menus.MenuItems;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;

namespace DataExportManager.Menus
{
    class ProjectSavedCohortsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectSavedCohortsNodeMenu(RDMPContextMenuStripArgs args, ProjectSavedCohortsNode savedCohortsNode): base(args, null)
        {
            Add(new ExecuteCommandImportFileAsNewCohort(_activator).SetTarget(savedCohortsNode.Project));
            Add(new ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults(_activator).SetTarget(savedCohortsNode.Project));
        }
    }
}
