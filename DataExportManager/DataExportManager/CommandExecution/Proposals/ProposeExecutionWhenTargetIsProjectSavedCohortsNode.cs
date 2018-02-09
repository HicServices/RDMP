using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsProjectSavedCohortsNode:RDMPCommandExecutionProposal<ProjectSavedCohortsNode>
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
                    new ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults(ItemActivator).SetTarget(cicCommand.CohortIdentificationConfiguration).SetTarget(target.Project);
            
            //drop a file on the SavedCohorts node to commit it
            var fileCommand = cmd as FileCollectionCommand;
            if(fileCommand != null && fileCommand.Files.Length == 1)
                return new ExecuteCommandImportFileAsNewCohort(ItemActivator,fileCommand.Files[0]).SetTarget(target.Project);

            return null;
        }
    }
}
