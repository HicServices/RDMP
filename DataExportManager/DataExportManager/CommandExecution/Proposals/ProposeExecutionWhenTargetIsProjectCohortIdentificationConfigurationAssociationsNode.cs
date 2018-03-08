using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsProjectCohortIdentificationConfigurationAssociationsNode : RDMPCommandExecutionProposal<ProjectCohortIdentificationConfigurationAssociationsNode>
    {
        public ProposeExecutionWhenTargetIsProjectCohortIdentificationConfigurationAssociationsNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ProjectCohortIdentificationConfigurationAssociationsNode target)
        {
            return false;
        }

        public override void Activate(ProjectCohortIdentificationConfigurationAssociationsNode target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ProjectCohortIdentificationConfigurationAssociationsNode target,
            InsertOption insertOption = InsertOption.Default)
        {
            var cicCommand = cmd as CohortIdentificationConfigurationCommand;
            if (cicCommand != null)
            {
                return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(ItemActivator).SetTarget(cicCommand.CohortIdentificationConfiguration).SetTarget(target.Project);
            }

            return null;
        }
    }
}
