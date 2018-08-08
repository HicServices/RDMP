using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsStandardPipelineUseCaseNode :RDMPCommandExecutionProposal<StandardPipelineUseCaseNode>
    {
        public ProposeExecutionWhenTargetIsStandardPipelineUseCaseNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(StandardPipelineUseCaseNode target)
        {
            return false;
        }

        public override void Activate(StandardPipelineUseCaseNode target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, StandardPipelineUseCaseNode target, InsertOption insertOption = InsertOption.Default)
        {
            var sourcePipelineCommand = cmd as PipelineCommand;
            if(sourcePipelineCommand != null)
                return new ExecuteCommandEditPipelineWithUseCase(ItemActivator,sourcePipelineCommand.Pipeline, target.UseCase);

            return null;
        }
    }
}