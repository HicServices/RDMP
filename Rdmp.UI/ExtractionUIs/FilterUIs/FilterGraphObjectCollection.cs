// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.ExtractionUIs.FilterUIs;

/// <summary>
/// Builds a query to fetch data that matches a given <see cref="IFilter"/>
/// </summary>
public class FilterGraphObjectCollection : PersistableObjectCollection
{
    public FilterGraphObjectCollection()
    {
    }

    public FilterGraphObjectCollection(AggregateConfiguration graph, ConcreteFilter filter) : this()
    {
        if (graph.IsCohortIdentificationAggregate)
            throw new ArgumentException(
                $"Graph '{graph}' is a Cohort Identification Aggregate, this is not allowed.  Aggregat must be a graph aggregate");
        DatabaseObjects.Add(graph);
        DatabaseObjects.Add(filter);
    }

    public AggregateConfiguration GetGraph()
    {
        return (AggregateConfiguration)DatabaseObjects.Single(o => o is AggregateConfiguration);
    }

    public IFilter GetFilter()
    {
        return (IFilter)DatabaseObjects.Single(o => o is IFilter);
    }

    public void HandleRefreshObject(RefreshObjectEventArgs e)
    {
        foreach (var o in DatabaseObjects)
            if (o.Equals(e.Object))
                ((IRevertable)o).RevertToDatabaseState();
    }
}