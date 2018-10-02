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

        public ExecuteCommandClonePipeline(IActivateItems activator, Pipeline pipeline)
            : base(activator)
        {
            _pipeline = pipeline;
            if (_pipeline == null)
                SetImpossible("You can only clone an existing pipeline"); 
        }

        public override void Execute()
        {
            base.Execute();

            _pipeline.Clone();
            Publish(_pipeline);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Link);
        }
    }
}