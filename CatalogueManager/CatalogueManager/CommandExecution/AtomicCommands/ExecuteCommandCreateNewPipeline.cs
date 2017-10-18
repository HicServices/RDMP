using System;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewPipeline :BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly PipelineUser _user;
        private readonly IPipelineUseCase _useCase;

        public ExecuteCommandCreateNewPipeline(IActivateItems activator, PipelineUser user, IPipelineUseCase useCase)
        {
            _activator = activator;
            _user = user;
            _useCase = useCase;

            if (_user.Getter() != null)
                SetImpossible(_user.User + " already has a Pipeline configured");
        }

        public override void Execute()
        {
            base.Execute();

            _user.Setter(new Pipeline(_activator.RepositoryLocator.CatalogueRepository, "CachingPipeline_" + Guid.NewGuid()));
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_user.User));

            //now activate it so the user can edit it
            var cmd = new ExecuteCommandEditPipeline(_activator, _user, _useCase);

            if(!cmd.IsImpossible)
                cmd.Execute();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Add); 
        }
    }
}