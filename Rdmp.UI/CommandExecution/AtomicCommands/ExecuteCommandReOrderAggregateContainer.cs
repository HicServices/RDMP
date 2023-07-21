// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandReOrderAggregateContainer : BasicUICommandExecution
{
    private readonly InsertOption _insertOption;
    private readonly CohortAggregateContainerCombineable _sourceCohortAggregateContainerCombineable;
    private readonly IOrderable _targetOrderable;

    private readonly CohortAggregateContainer _targetParent;

    public ExecuteCommandReOrderAggregateContainer(IActivateItems activator,
        CohortAggregateContainerCombineable sourceCohortAggregateContainerCombineable,
        CohortAggregateContainer targetCohortAggregateContainer, InsertOption insertOption) : this(activator,
        targetCohortAggregateContainer, insertOption)
    {
        _sourceCohortAggregateContainerCombineable = sourceCohortAggregateContainerCombineable;

        _targetParent = targetCohortAggregateContainer.GetParentContainerIfAny();

        //reorder is only possible within a container
        if (!_sourceCohortAggregateContainerCombineable.ParentContainerIfAny.Equals(_targetParent))
            SetImpossible("First move containers to share the same parent container");

        if (_insertOption == InsertOption.Default)
            SetImpossible("Insert must be above/below");

        if (_targetParent.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
    }

    public ExecuteCommandReOrderAggregateContainer(IActivateItems activator,
        CohortAggregateContainerCombineable sourceCohortAggregateContainerCombineable,
        AggregateConfiguration targetAggregate, InsertOption insertOption) : this(activator, targetAggregate,
        insertOption)
    {
        _sourceCohortAggregateContainerCombineable = sourceCohortAggregateContainerCombineable;
        _targetParent = targetAggregate.GetCohortAggregateContainerIfAny();

        //if they do not share the same parent container
        if (!_sourceCohortAggregateContainerCombineable.ParentContainerIfAny.Equals(_targetParent))
            SetImpossible("First move objects into the same parent container");
    }

    private ExecuteCommandReOrderAggregateContainer(IActivateItems activator, IOrderable orderable,
        InsertOption insertOption) : base(activator)
    {
        _targetOrderable = orderable;
        _insertOption = insertOption;
    }

    public override void Execute()
    {
        base.Execute();

        var source = _sourceCohortAggregateContainerCombineable.AggregateContainer;

        var order = _targetOrderable.Order;

        _targetParent.CreateInsertionPointAtOrder(source, order, _insertOption == InsertOption.InsertAbove);
        source.Order = order;
        source.SaveToDatabase();

        //refresh the parent container
        Publish(_sourceCohortAggregateContainerCombineable.ParentContainerIfAny);
    }
}