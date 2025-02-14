// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Changes the pivot dimension of an aggregate graph
/// </summary>
public class ExecuteCommandSetPivot : BasicCommandExecution
{
    private readonly AggregateConfiguration aggregate;
    private readonly string column;
    private readonly bool askAtRuntime;

    public ExecuteCommandSetPivot(IBasicActivateItems basicActivator, AggregateConfiguration aggregate) : base(
        basicActivator)
    {
        this.aggregate = aggregate;

        // don't let them try to set a pivot on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
        if (aggregate.IsCohortIdentificationAggregate)
        {
            SetImpossible(
                $"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have a pivot");
            return;
        }

        askAtRuntime = true;
    }


    [UseWithObjectConstructor]
    public ExecuteCommandSetPivot(IBasicActivateItems basicActivator, AggregateConfiguration aggregate, string column) :
        base(basicActivator)
    {
        this.aggregate = aggregate;
        this.column = column;

        if (!string.IsNullOrWhiteSpace(column))
        {
            // don't let them try to set a pivot on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
            if (aggregate.IsCohortIdentificationAggregate)
            {
                SetImpossible(
                    $"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have a pivot");
            }
        }
        else
        {
            if (aggregate.PivotOnDimensionID == null)
                SetImpossible($"AggregateConfiguration {aggregate} does not have a pivot to clear");
        }
    }

    public override void Execute()
    {
        base.Execute();

        if (string.IsNullOrWhiteSpace(column) && !askAtRuntime)
        {
            aggregate.PivotOnDimensionID = null;
            aggregate.SaveToDatabase();
        }
        else
        {
            var opts = AggregateBuilderOptionsFactory.Create(aggregate);
            AggregateDimension match = null;

            if (askAtRuntime)
            {
                var possible = aggregate.AggregateDimensions.Where(d => !d.IsDate()).ToArray();

                if (!possible.Any())
                    throw new Exception($"There are no AggregateDimensions in {aggregate} that can be used as a Pivot");

                match = (AggregateDimension)BasicActivator.SelectOne("Choose pivot dimension", possible);

                if (match == null) return;
            }
            else
            {
                match = aggregate.AggregateDimensions.FirstOrDefault(a => string.Equals(column, a.ToString()));
                if (match == null)
                    throw new Exception(
                        $"Could not find AggregateDimension {column} in Aggregate {aggregate} so could not set it as a pivot dimension.  Try adding the column to the aggregate first");
            }

            if (match.IsDate())
                throw new Exception(
                    $"AggregateDimension {match} is a Date so cannot set it as a Pivot for Aggregate {aggregate}");

            var enable = opts.ShouldBeEnabled(AggregateEditorSection.PIVOT, aggregate);

            if (!enable)
                throw new Exception(
                    $"Current state of Aggregate {aggregate} does not support having a Pivot Dimension");

            aggregate.PivotOnDimensionID = match.ID;
            aggregate.SaveToDatabase();
        }

        Publish(aggregate);
    }
}