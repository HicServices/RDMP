using System;
using System.Drawing;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.ItemActivation;
using CatalogueManager.PipelineUIs.Pipelines;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandEditPipelineWithUseCase : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Pipeline _pipeline;
        private readonly PipelineUseCase _useCase;

        public ExecuteCommandEditPipelineWithUseCase(IActivateItems itemActivator,Pipeline pipeline, PipelineUseCase useCase):base(itemActivator)
        {
            _pipeline = pipeline;
            _useCase = useCase;
        }

        public override void Execute()
        {
            base.Execute();

            //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
            var dialog = new ConfigurePipelineUI(_pipeline, _useCase, Activator.RepositoryLocator.CatalogueRepository);
            dialog.ShowDialog();
            
            Publish(_pipeline);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}