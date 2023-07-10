// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMoveFilterIntoContainer : BasicCommandExecution
{
    private readonly FilterCombineable _filterCombineable;
    private readonly IContainer _targetContainer;

    [UseWithObjectConstructor]
    public ExecuteCommandMoveFilterIntoContainer(IBasicActivateItems activator, IFilter toMove, IContainer into)
        : this(activator,new FilterCombineable(toMove),into)
    {
    }

    public ExecuteCommandMoveFilterIntoContainer(IBasicActivateItems activator, FilterCombineable filterCombineable,
        IContainer targetContainer) : base(activator)
    {
        _filterCombineable = filterCombineable;
        _targetContainer = targetContainer;

        if (!filterCombineable.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
            SetImpossible("Filters can only be moved within their own container tree");

        if (targetContainer.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
    }

    public override void Execute()
    {
        base.Execute();

        _filterCombineable.Filter.FilterContainer_ID = _targetContainer.ID;
        ((DatabaseEntity)_filterCombineable.Filter).SaveToDatabase();
        Publish((DatabaseEntity)_targetContainer);
    }
}