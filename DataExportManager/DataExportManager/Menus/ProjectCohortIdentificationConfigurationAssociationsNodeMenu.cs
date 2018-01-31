using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using DataExportManager.Collections.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    class ProjectCohortIdentificationConfigurationAssociationsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectCohortIdentificationConfigurationAssociationsNodeMenu(RDMPContextMenuStripArgs args, ProjectCohortIdentificationConfigurationAssociationsNode node)
            : base(args, null)
        {
            Add(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(node.Project));

        }
    }
}
