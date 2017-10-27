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
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewPipeline :BasicUICommandExecution, IAtomicCommand
    {
        private readonly PipelineUser _user;
        private readonly IPipelineUseCase _useCase;

        public ExecuteCommandCreateNewPipeline(IActivateItems activator, PipelineUser user, IPipelineUseCase useCase) : base(activator)
        {
            _user = user;
            _useCase = useCase;

            if (_user.Getter() != null)
                SetImpossible(_user.User + " already has a Pipeline configured");
        }

        public override void Execute()
        {
            base.Execute();

            _user.Setter(new Pipeline(Activator.RepositoryLocator.CatalogueRepository, "CachingPipeline_" + Guid.NewGuid()));
            Publish(_user.User);

            //now activate it so the user can edit it
            var cmd = new ExecuteCommandEditPipeline(Activator, _user, _useCase);

            if(!cmd.IsImpossible)
                cmd.Execute();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Add); 
        }
    }
}