// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsFilterContainer : RDMPCommandExecutionProposal<IContainer>
{
    public ProposeExecutionWhenTargetIsFilterContainer(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(IContainer target) => false;

    public override void Activate(IContainer target)
    {
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, IContainer targetContainer,
        InsertOption insertOption = InsertOption.Default)
    {
        //drag a filter into a container
        if (cmd is FilterCombineable sourceFilterCommand)
        {
            //if filter is already in the target container
            if (sourceFilterCommand.ImmediateContainerIfAny?.Equals(targetContainer) ?? false)
                return null;

            //if the target container is one that is part of the filters tree then it's a move
            if (sourceFilterCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
                return new ExecuteCommandMoveFilterIntoContainer(ItemActivator, sourceFilterCommand, targetContainer);

            //otherwise it's an import

            //so instead let's let them create a new copy (possibly including changing the type e.g. importing a master
            //filter into a data export AND/OR container
            return new ExecuteCommandCreateNewFilter(ItemActivator, targetContainer, sourceFilterCommand.Filter);
        }

        //drag a container into another container
        if (cmd is ContainerCombineable sourceContainerCommand)
        {
            //if the source and target are the same container
            if (sourceContainerCommand.Container.Equals(targetContainer))
                return null;

            //is it a movement within the current container tree
            return sourceContainerCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer)
                ? new ExecuteCommandMoveContainerIntoContainer(ItemActivator, sourceContainerCommand, targetContainer)
                : new ExecuteCommandImportFilterContainerTree(ItemActivator, targetContainer,
                    sourceContainerCommand.Container);
        }

        return null;
    }
}