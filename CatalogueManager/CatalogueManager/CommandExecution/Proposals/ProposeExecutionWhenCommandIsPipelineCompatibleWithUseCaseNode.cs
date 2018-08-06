using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueManager.ItemActivation;
using CatalogueManager.PipelineUIs.Pipelines;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenCommandIsPipelineCompatibleWithUseCaseNode : RDMPCommandExecutionProposal<PipelineCompatibleWithUseCaseNode>
    {
        public ProposeExecutionWhenCommandIsPipelineCompatibleWithUseCaseNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(PipelineCompatibleWithUseCaseNode target)
        {
            return true;
        }

        public override void Activate(PipelineCompatibleWithUseCaseNode target)
        {
            //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
            var dialog = new ConfigurePipelineUI(target.Pipeline, target.UseCase, ItemActivator.RepositoryLocator.CatalogueRepository);
            dialog.ShowDialog();
        }
        public override ICommandExecution ProposeExecution(ICommand cmd, PipelineCompatibleWithUseCaseNode target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
