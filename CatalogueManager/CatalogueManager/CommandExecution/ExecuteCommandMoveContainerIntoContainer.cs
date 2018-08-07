using System;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveContainerIntoContainer : BasicUICommandExecution
    {
        private readonly ContainerCommand _containerCommand;
        private readonly IContainer _targetContainer;

        public ExecuteCommandMoveContainerIntoContainer(IActivateItems activator, ContainerCommand containerCommand, IContainer targetContainer) : base(activator)
        {
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
            Publish((DatabaseEntity) _targetContainer);
        }
    }
}