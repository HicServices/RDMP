using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewPipeline :BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly PipelineUser _user;

        public ExecuteCommandCreateNewPipeline(IActivateItems activator, PipelineUser user)
        {
            _activator = activator;
            _user = user;

            if (_user.Getter() != null)
                SetImpossible(_user.User + " already has a Pipeline configured");
        }

        public override void Execute()
        {
            base.Execute();

            _user.Setter(new Pipeline(_activator.RepositoryLocator.CatalogueRepository, "CachingPipeline_" + Guid.NewGuid()));
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_user.User));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Add); 
        }
    }
}