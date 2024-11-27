// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;
using System;
using System.Linq;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandReorderFilter : BasicUICommandExecution
{
    private readonly ConcreteFilter _source;
    private readonly ConcreteFilter _target;
    private readonly InsertOption _insertOption;

    public ExecuteCommandReorderFilter(IActivateItems activator, ConcreteFilter source, ConcreteFilter destination, InsertOption insertOption) : base(activator)
    {
        _source = source;
        _target = destination;
        _insertOption = insertOption;
        if (_source.FilterContainer_ID is null || _target.FilterContainer_ID is null)
        {
            SetImpossible("Both filters must exist within some container in order to be orderable");
        }
        if (_source.FilterContainer_ID != _target.FilterContainer_ID)
        {
            SetImpossible("Cannot reorder filters as they do not share a parent");
        }
    }

    public override void Execute()
    {
        var order = _target.Order;

        var filters = _target.FilterContainer.GetFilters().Where(f => f is ConcreteFilter).Select(f => (ConcreteFilter)f).ToArray();
        Array.Sort(
           filters,
            delegate (ConcreteFilter a, ConcreteFilter b) { return a.Order.CompareTo(b.Order); }
        );
        if (!filters.All(c => c.Order != order))
        {
            foreach (var orderable in filters)
            {
                if (orderable.Order < order)
                    orderable.Order--;
                else if (orderable.Order > order)
                    orderable.Order++;
                else //collision on order
                    orderable.Order += _insertOption == InsertOption.InsertAbove ? 1 : -1;
                ((ISaveable)orderable).SaveToDatabase();
            }
        }
        _source.Order = order;
        _source.SaveToDatabase();
        Publish(_target.FilterContainer);
    }
}
