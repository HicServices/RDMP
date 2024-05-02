// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Adds a new <see cref="AggregateDimension" /> to a <see cref="AggregateConfiguration" /> based on one of the
///     associated <see cref="Catalogue" /> <see cref="ExtractionInformation" />
/// </summary>
public sealed class ExecuteCommandAddDimension : BasicCommandExecution
{
    private readonly AggregateConfiguration _aggregate;
    private readonly string _column;
    private readonly bool _askAtRuntime;
    private const float DefaultWeight = 2.4f;

    public ExecuteCommandAddDimension(IBasicActivateItems basicActivator, AggregateConfiguration aggregate) : base(
        basicActivator)
    {
        Weight = DefaultWeight;
        _aggregate = aggregate;
        _askAtRuntime = true;
        ValidateCanAdd(aggregate);
    }

    [UseWithObjectConstructor]
    public ExecuteCommandAddDimension(IBasicActivateItems basicActivator, AggregateConfiguration aggregate,
        string column) : base(basicActivator)
    {
        Weight = DefaultWeight;
        _aggregate = aggregate;
        _column = column;

        if (!string.IsNullOrWhiteSpace(column))
        {
            // don't let them try to set a pivot on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
            if (aggregate.IsCohortIdentificationAggregate)
            {
                SetImpossible(
                    $"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have a pivot");
                return;
            }
        }
        else if (aggregate.PivotOnDimensionID == null)
        {
            SetImpossible($"AggregateConfiguration {aggregate} does not have a pivot to clear");
            return;
        }

        ValidateCanAdd(aggregate);
    }

    private void ValidateCanAdd(AggregateConfiguration aggregate)
    {
        if (aggregate.Catalogue.IsApiCall()) SetImpossible("API calls cannot have AggregateDimensions");

        if (aggregate.IsCohortIdentificationAggregate && !aggregate.IsJoinablePatientIndexTable())
            if (aggregate.AggregateDimensions.Any())
                SetImpossible(
                    "Cohort aggregates can only have a single dimension. Remove existing dimension to select another.");
    }

    public override void Execute()
    {
        base.Execute();

        var opts = AggregateBuilderOptionsFactory.Create(_aggregate);
        ExtractionInformation match = null;

        var possible = opts.GetAvailableSELECTColumns(_aggregate).OfType<ExtractionInformation>().ToArray();

        if (_askAtRuntime)
        {
            if (!possible.Any())
                throw new Exception(
                    $"There are no ExtractionInformation that can be added as new dimensions to {_aggregate}");

            match = (ExtractionInformation)BasicActivator.SelectOne("Choose dimension to add", possible);

            if (match == null) return;
        }
        else
        {
            match = possible.FirstOrDefault(a => string.Equals(_column, a.ToString()));
            if (match == null)
                throw new Exception(
                    $"Could not find ExtractionInformation {_column} in as an addable column to {_aggregate}");
        }

        var dim = new AggregateDimension(BasicActivator.RepositoryLocator.CatalogueRepository, match, _aggregate);
        dim.SaveToDatabase();

        Publish(_aggregate);
    }
}