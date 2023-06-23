// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.UI.AggregationUIs.Advanced;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsAggregateConfiguration:RDMPCommandExecutionProposal<AggregateConfiguration>
{
    public ProposeExecutionWhenTargetIsAggregateConfiguration(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(AggregateConfiguration target)
    {
        return true;
    }

    public override void Activate(AggregateConfiguration target)
    {
        ItemActivator.Activate<AggregateEditorUI, AggregateConfiguration>(target);
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, AggregateConfiguration targetAggregateConfiguration, InsertOption insertOption = InsertOption.Default)
    {
        return cmd switch
        {
            ContainerCombineable cc => new ExecuteCommandImportFilterContainerTree(ItemActivator,
                targetAggregateConfiguration, cc.Container),
            //if it is an aggregate being dragged
            AggregateConfigurationCombineable sourceAggregateCommand =>
                !sourceAggregateCommand.Aggregate.Equals(targetAggregateConfiguration)
                    ? sourceAggregateCommand.ContainerIfAny != null && insertOption != InsertOption.Default
                        ? new ExecuteCommandReOrderAggregate(ItemActivator, sourceAggregateCommand,
                            targetAggregateConfiguration, insertOption)
                        : null
                    : null,
            CohortAggregateContainerCombineable sourceCohortAggregateContainerCommand =>
                sourceCohortAggregateContainerCommand.ParentContainerIfAny != null
                    ? insertOption != InsertOption.Default
                        ? new ExecuteCommandReOrderAggregateContainer(ItemActivator,
                            sourceCohortAggregateContainerCommand, targetAggregateConfiguration, insertOption)
                        : null
                    : null,
            ExtractionFilterParameterSetCombineable efps => new ExecuteCommandCreateNewFilter(ItemActivator,
                targetAggregateConfiguration)
            {
                BasedOn = efps.ParameterSet.ExtractionFilter, ParameterSet = efps.ParameterSet
            },
            _ => null
        };
    }
}