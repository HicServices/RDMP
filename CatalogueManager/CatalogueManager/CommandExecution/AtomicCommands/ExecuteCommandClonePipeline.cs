using System.Drawing;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandClonePipeline : BasicUICommandExecution, IAtomicCommand
    {
        private readonly Pipeline _pipeline;
        private readonly PipelineUseCase _useCase;

        public ExecuteCommandClonePipeline(IActivateItems activator, Pipeline pipeline, PipelineUseCase useCase)
            : base(activator)
        {
            _pipeline = pipeline;
            if (_pipeline == null)
                SetImpossible("You can only clone an existing pipeline"); 
            
            _useCase = useCase;
            if (_useCase == null)
                SetImpossible("Pipelines can only be created under an established use case");
        }

        public override void Execute()
        {
            base.Execute();

            var newPipe = _pipeline.Clone();
            var edit = new ExecuteCommandEditPipelineWithUseCase(Activator, newPipe, _useCase);
            edit.Execute();

        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Link);
        }
    }
}