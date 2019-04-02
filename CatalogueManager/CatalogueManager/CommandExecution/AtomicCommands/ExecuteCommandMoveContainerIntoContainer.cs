// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Data;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.CommandExecution.AtomicCommands
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