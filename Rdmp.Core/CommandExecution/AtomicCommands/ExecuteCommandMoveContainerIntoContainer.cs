// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandMoveContainerIntoContainer : BasicCommandExecution
    {
        private readonly ContainerCombineable _containerCombineable;
        private readonly IContainer _targetContainer;

        [UseWithObjectConstructor]
        public ExecuteCommandMoveContainerIntoContainer(IBasicActivateItems activator, IContainer toMove, IContainer into) 
            : this(activator,new ContainerCombineable(toMove),into)
        {

        }
        public ExecuteCommandMoveContainerIntoContainer(IBasicActivateItems activator, ContainerCombineable containerCombineable, IContainer targetContainer) : base(activator)
        {
            _containerCombineable = containerCombineable;
            _targetContainer = targetContainer;

            if(containerCombineable.AllSubContainersRecursive.Contains(targetContainer))
                SetImpossible("You cannot move a container (AND/OR) into one of its own subcontainers");

            if(targetContainer.ShouldBeReadOnly(out string reason))
                SetImpossible(reason);
        }

        public override void Execute()
        {
            base.Execute();

            _containerCombineable.Container.MakeIntoAnOrphan();
            _targetContainer.AddChild(_containerCombineable.Container);
            Publish((DatabaseEntity) _targetContainer);
        }
    }
}