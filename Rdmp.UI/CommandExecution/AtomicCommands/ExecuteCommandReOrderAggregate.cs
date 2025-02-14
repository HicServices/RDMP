// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandReOrderAggregate : BasicUICommandExecution
{
    private readonly AggregateConfigurationCombineable _sourceAggregateCommand;
    private CohortAggregateContainer _parentContainer;

    private IOrderable _targetOrder;
    private readonly InsertOption _insertOption;

    public ExecuteCommandReOrderAggregate(IActivateItems activator,
        AggregateConfigurationCombineable sourceAggregateCommand, AggregateConfiguration targetAggregateConfiguration,
        InsertOption insertOption) : this(activator, targetAggregateConfiguration, insertOption)
    {
        _sourceAggregateCommand = sourceAggregateCommand;
        _parentContainer = targetAggregateConfiguration.GetCohortAggregateContainerIfAny();

        if (_parentContainer == null)
        {
            SetImpossible(
                $"Target Aggregate {targetAggregateConfiguration} is not part of any cohort identification set containers");
            return;
        }

        if (!sourceAggregateCommand.ContainerIfAny.Equals(_parentContainer))
        {
            SetImpossible(
                "Cannot ReOrder, you should first move both objects into the same container (UNION / EXCEPT / INTERSECT)");
        }
    }

    public ExecuteCommandReOrderAggregate(IActivateItems activator,
        AggregateConfigurationCombineable sourceAggregateCommand,
        CohortAggregateContainer targetCohortAggregateContainer, InsertOption insertOption) : this(activator,
        targetCohortAggregateContainer, insertOption)
    {
        _sourceAggregateCommand = sourceAggregateCommand;
        _parentContainer = targetCohortAggregateContainer.GetParentContainerIfAny();

        if (!sourceAggregateCommand.ContainerIfAny.Equals(_parentContainer))
        {
            SetImpossible(
                "Cannot ReOrder, you should first move both objects into the same container (UNION / EXCEPT / INTERSECT)");
        }
    }

    private ExecuteCommandReOrderAggregate(IActivateItems activator, IOrderable target, InsertOption insertOption) :
        base(activator)
    {
        _targetOrder = target;
        _insertOption = insertOption;

        if (target is IMightBeReadOnly ro && ro.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
    }

    public override void Execute()
    {
        base.Execute();

        var source = _sourceAggregateCommand.Aggregate;
        _sourceAggregateCommand.ContainerIfAny.RemoveChild(source);

        var targetOrder = _targetOrder.Order;

        _parentContainer.CreateInsertionPointAtOrder(source, targetOrder, _insertOption == InsertOption.InsertAbove);
        _parentContainer.AddChild(source, targetOrder);

        //refresh the parent container
        Publish(_parentContainer);
    }
}