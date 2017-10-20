using System;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveContainerIntoContainer : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly ContainerCommand _containerCommand;
        private readonly IContainer _targetContainer;

        public ExecuteCommandMoveContainerIntoContainer(IActivateItems activator, ContainerCommand containerCommand, IContainer targetContainer)
        {
            _activator = activator;
            _containerCommand = containerCommand;
            _targetContainer = targetContainer;

            if(containerCommand.AllSubContainersRecursive.Contains(targetContainer))
                SetImpossible("You cannot move a container (AND/OR) into one of it's own subcontainers");
        }

        public override void Execute()
        {
            base.Execute();

            _containerCommand.Container.MakeIntoAnOrphan();
            _targetContainer.AddChild(_containerCommand.Container);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs((DatabaseEntity) _targetContainer));
        }
    }
}