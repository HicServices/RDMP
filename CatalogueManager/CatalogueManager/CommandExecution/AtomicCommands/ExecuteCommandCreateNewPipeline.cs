using System.Drawing;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewPipeline : BasicUICommandExecution,IAtomicCommand
    {
        private readonly PipelineUseCase _useCase;

        public ExecuteCommandCreateNewPipeline(IActivateItems activator, PipelineUseCase useCase) : base(activator)
        {
            _useCase = useCase;

            if(_useCase == null)
                SetImpossible("Pipelines can only be created under an established use case");
        }

        public override void Execute()
        {
            base.Execute();

            var newPipe = new Pipeline(Activator.RepositoryLocator.CatalogueRepository);
            var edit = new ExecuteCommandEditPipelineWithUseCase(Activator, newPipe, _useCase);
            edit.Execute();

        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Add);
        }
    }
}