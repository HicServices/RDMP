using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.PipelineUIs.Pipelines;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsPipelineCompatibleWithUseCaseNode : RDMPCommandExecutionProposal<PipelineCompatibleWithUseCaseNode>
    {
        public ProposeExecutionWhenTargetIsPipelineCompatibleWithUseCaseNode(IActivateItems itemActivator): base(itemActivator)
        {
        }

        public override bool CanActivate(PipelineCompatibleWithUseCaseNode target)
        {
            return true;
        }

        public override void Activate(PipelineCompatibleWithUseCaseNode target)
        {
            var cmd = new ExecuteCommandEditPipelineWithUseCase(ItemActivator,target.Pipeline, target.UseCase);
            cmd.Execute();
        }
        public override ICommandExecution ProposeExecution(ICommand cmd, PipelineCompatibleWithUseCaseNode target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
