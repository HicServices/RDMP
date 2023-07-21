﻿// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Sets the <see cref="AggregateContinuousDateAxis"/> on a graph <see cref="AggregateConfiguration"/>
/// </summary>
public class ExecuteCommandSetAxis : BasicCommandExecution
{
    private readonly AggregateConfiguration aggregate;
    private readonly string column;
    private readonly bool askAtRuntime;

    public ExecuteCommandSetAxis(IBasicActivateItems basicActivator, AggregateConfiguration aggregate) : base(basicActivator)
    {
        this.aggregate = aggregate;

        // don't let them try to set a axis on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
        if (aggregate.IsCohortIdentificationAggregate)
        {
            SetImpossible($"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have an axis");
            return;
        }

        if (aggregate.GetAxisIfAny() != null)
        {
            SetImpossible($"AggregateConfiguration {aggregate} already has an axis");
            return;
        }

        askAtRuntime = true;
    }


    [UseWithObjectConstructor]
    public ExecuteCommandSetAxis(IBasicActivateItems basicActivator, AggregateConfiguration aggregate, string column) : base(basicActivator)
    {
        this.aggregate = aggregate;
        this.column = column;

        if (!string.IsNullOrWhiteSpace(column))
        {
            // don't let them try to set an axis on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
            if (aggregate.IsCohortIdentificationAggregate)
            {
                SetImpossible($"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have an axis");
                return;
            }

            if (aggregate.GetAxisIfAny() != null)
            {
                SetImpossible($"AggregateConfiguration {aggregate} already has an axis");
                return;
            }
        }
        else
        {
            if (aggregate.GetAxisIfAny() == null)
            {
                SetImpossible($"AggregateConfiguration {aggregate} does not have an axis to clear");
            }
        }

    }
    public override void Execute()
    {
        base.Execute();

        if (string.IsNullOrWhiteSpace(column) && !askAtRuntime)
        {
            aggregate.GetAxisIfAny()?.DeleteInDatabase();
        }
        else
        {
            if (aggregate.GetAxisIfAny() != null)
            {
                throw new Exception($"Aggregate {aggregate} already has an axis");
            }

            var opts = AggregateBuilderOptionsFactory.Create(aggregate);
            AggregateDimension match = null;

            if (askAtRuntime)
            {
                var possible = aggregate.AggregateDimensions.Where(d => d.IsDate()).ToArray();

                if (!possible.Any())
                {
                    throw new Exception($"There are no AggregateDimensions in {aggregate} that can be used as an axis (Dimensions must be Date Type)");
                }

                match = (AggregateDimension)BasicActivator.SelectOne("Choose axis dimension", possible);

                if (match == null)
                {
                    return;
                }
            }
            else
            {
                match = aggregate.AggregateDimensions.FirstOrDefault(a => string.Equals(column, a.ToString()));
                if (match == null)
                {
                    throw new Exception($"Could not find AggregateDimension {column} in Aggregate {aggregate} so could not set it as an axis dimension.  Try adding the column to the aggregate first");
                }
            }

            if (!match.IsDate())
            {
                throw new Exception($"AggregateDimension {match} is not a Date so cannot set it as an axis for Aggregate {aggregate}");
            }

            var enable = opts.ShouldBeEnabled(AggregateEditorSection.AXIS, aggregate);

            if (!enable)
            {
                throw new Exception($"Current state of Aggregate {aggregate} does not support having an axis Dimension");
            }

            var axis = new AggregateContinuousDateAxis(BasicActivator.RepositoryLocator.CatalogueRepository, match);
            axis.SaveToDatabase();
        }

        Publish(aggregate);
    }

}