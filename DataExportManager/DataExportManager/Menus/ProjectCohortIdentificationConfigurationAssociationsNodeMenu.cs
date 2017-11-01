using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using DataExportManager.Collections.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    public class ProjectCohortIdentificationConfigurationAssociationsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectCohortIdentificationConfigurationAssociationsNodeMenu(IActivateItems activator, ProjectCohortIdentificationConfigurationAssociationsNode node): base(activator, null)
        {
            Add(
                new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(activator).SetTarget(
                    node.Project));
        }
    }
}
