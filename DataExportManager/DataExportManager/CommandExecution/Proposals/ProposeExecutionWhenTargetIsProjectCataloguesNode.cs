using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Providers.Nodes;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsProjectCataloguesNode : RDMPCommandExecutionProposal<ProjectCataloguesNode>
    {
        private ProposeExecutionWhenTargetIsProject _projectFunctionality;

        public ProposeExecutionWhenTargetIsProjectCataloguesNode(IActivateItems itemActivator) : base(itemActivator)
        {
            _projectFunctionality = new ProposeExecutionWhenTargetIsProject(itemActivator);
        }

        public override bool CanActivate(ProjectCataloguesNode target)
        {
            return false;
        }

        public override void Activate(ProjectCataloguesNode target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ProjectCataloguesNode target, InsertOption insertOption = InsertOption.Default)
        {
            //use the same drop options as Project except for this one

            if (cmd is CohortIdentificationConfigurationCommand)
                return null;

            
            return _projectFunctionality.ProposeExecution(cmd, target.Project, insertOption);
        }
    }
}