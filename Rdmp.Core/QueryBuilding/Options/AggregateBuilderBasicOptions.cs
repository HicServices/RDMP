// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.QueryBuilding.Options;

/// <summary>
///     Describes what parts of the GROUP BY statement are allowed for <see cref="AggregateConfiguration" /> that are
///     running in 'graph mode'
/// </summary>
public class AggregateBuilderBasicOptions : IAggregateBuilderOptions
{
    /// <inheritdoc />
    public string GetTitleTextPrefix(AggregateConfiguration aggregate)
    {
        return aggregate.IsExtractable ? "Extractable 'Group By' Aggregate:" : "'Group By' Aggregate:";
    }

    /// <inheritdoc />
    public IColumn[] GetAvailableSELECTColumns(AggregateConfiguration aggregate)
    {
        var existingDimensions = aggregate.AggregateDimensions.Select(d => d.ExtractionInformation).ToArray();

        return aggregate.Catalogue
            .GetAllExtractionInformation(ExtractionCategory.Any) //all columns of any extraction category
            .Except(existingDimensions) //except those that have already been added
            .Where(e => !e
                .IsExtractionIdentifier) //don't advertise IsExtractionIdentifier columns for use in basic aggregates
            .Cast<IColumn>()
            .ToArray();
    }

    /// <inheritdoc />
    public IColumn[] GetAvailableWHEREColumns(AggregateConfiguration aggregate)
    {
        //for this basic case the WHERE columns can be anything
        return aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Cast<IColumn>().ToArray();
    }

    /// <inheritdoc />
    public bool ShouldBeEnabled(AggregateEditorSection section, AggregateConfiguration aggregate)
    {
        return section switch
        {
            AggregateEditorSection.Extractable => CanMakeExtractable(aggregate),
            AggregateEditorSection.TOPX =>
                //can only Top X if we have a pivot (top x applies to the selection of the pivot values) or if we have nothing (no axis / pivot).  This rules out axis only queries
                aggregate.PivotOnDimensionID != null || aggregate.GetAxisIfAny() == null,
            AggregateEditorSection.PIVOT => aggregate.GetAxisIfAny() != null ||
                                            aggregate.AggregateDimensions.Length ==
                                            2 //can only pivot if there is an axis or exactly 2 dimensions (+ count)
            ,
            AggregateEditorSection.AXIS => true,
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable[] GetAvailableJoinables(AggregateConfiguration aggregate)
    {
        var availableTables = aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Select(e => e.ColumnInfo?.TableInfo)
            .Where(t => t != null)
            .Distinct();

        var implicitJoins =
            aggregate.ForcedJoins.Union(
                aggregate.AggregateDimensions.Select(d => d.ExtractionInformation.ColumnInfo.TableInfo).Distinct());

        //return all TableInfos that are not already force joined
        return availableTables.Except(implicitJoins).Cast<IMapsDirectlyToDatabaseTable>().ToArray();
    }

    private static bool CanMakeExtractable(AggregateConfiguration aggregate)
    {
        //if it has any extraction identifiers then it cannot be extractable!
        if (aggregate.AggregateDimensions.Any(d => d.IsExtractionIdentifier))
        {
            aggregate.IsExtractable = false;
            aggregate.SaveToDatabase();
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public ISqlParameter[] GetAllParameters(AggregateConfiguration aggregate)
    {
        return aggregate.GetAllParameters();
    }

    /// <inheritdoc />
    public CountColumnRequirement GetCountColumnRequirement(AggregateConfiguration aggregate)
    {
        return CountColumnRequirement.MustHaveOne;
    }
}