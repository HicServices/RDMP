using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsProjectSavedCohortsNode:RDMPCommandExecutionProposal<ProjectSavedCohortsNode>
    {
        public ProposeExecutionWhenTargetIsProjectSavedCohortsNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ProjectSavedCohortsNode target)
        {
            return false;
        }

        public override void Activate(ProjectSavedCohortsNode target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ProjectSavedCohortsNode target, InsertOption insertOption = InsertOption.Default)
        {
            //drop a cic on a Saved Cohorts Node to commit it to that project
            var cicCommand = cmd as CohortIdentificationConfigurationCommand;
            if (cicCommand != null)
                return
                    new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(ItemActivator).SetTarget(cicCommand.CohortIdentificationConfiguration).SetTarget(target.Project);
            
            //drop a file on the SavedCohorts node to commit it
            var fileCommand = cmd as FileCollectionCommand;
            if(fileCommand != null && fileCommand.Files.Length == 1)
                return new ExecuteCommandCreateNewCohortFromFile(ItemActivator,fileCommand.Files[0]).SetTarget(target.Project);

            //drop a Project Specific Catalogue onto it
            var catalogueCommand = cmd as CatalogueCommand;
            if (catalogueCommand != null)
                return new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator, catalogueCommand.Catalogue).SetTarget(target.Project);

            var columnCommand = cmd as ColumnCommand;

            if (columnCommand != null && columnCommand.Column is ExtractionInformation)
                return new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator,(ExtractionInformation) columnCommand.Column);

            return null;
        }
    }
}
