// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding.Parameters;

namespace Rdmp.Core.QueryBuilding.Options;

/// <summary>
///     Describes what parts of the GROUP BY statement are allowed for <see cref="AggregateConfiguration" /> that are
///     running as a 'cohort set'
/// </summary>
public class AggregateBuilderCohortOptions : IAggregateBuilderOptions
{
    private readonly ISqlParameter[] _globals;

    /// <summary>
    ///     Creates an <see cref="IAggregateBuilderOptions" /> for use with <see cref="AggregateConfiguration" /> which are
    ///     <see cref="AggregateConfiguration.IsCohortIdentificationAggregate" />
    /// </summary>
    /// <param name="globals">Global parameters found in the scope of <see cref="AggregateConfiguration" /> you intend to use</param>
    public AggregateBuilderCohortOptions(ISqlParameter[] globals)
    {
        _globals = globals;
    }

    /// <inheritdoc />
    public string GetTitleTextPrefix(AggregateConfiguration aggregate)
    {
        return aggregate.IsJoinablePatientIndexTable()
            ? "Patient Index Table:"
            : "Cohort Identification Set:";
    }

    /// <inheritdoc />
    public IColumn[] GetAvailableSELECTColumns(AggregateConfiguration aggregate)
    {
        //get the existing dimensions
        var alreadyExisting = aggregate.AggregateDimensions.ToArray();

        //get novel ExtractionInformations from the catalogue for which there are not already any Dimensions
        var candidates = aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Where(e => alreadyExisting.All(d => d.ExtractionInformation_ID != e.ID)).ToArray();

        //patient index tables can have any columns
        if (aggregate.IsJoinablePatientIndexTable())
            return candidates;

        //otherwise only return the patient identifier column(s) - for example Marriages dataset would have Partner1Identifier and Partner2Identifier
        return candidates.Where(c => c.IsExtractionIdentifier).ToArray();
    }

    /// <inheritdoc />
    public IColumn[] GetAvailableWHEREColumns(AggregateConfiguration aggregate)
    {
        var toReturn = new List<IColumn>();

        toReturn.AddRange(aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any));

        //for each joined PatientIdentifier table
        foreach (var usedJoinable in aggregate.PatientIndexJoinablesUsed)
        {
            var tableAlias = usedJoinable.GetJoinTableAlias();
            var hackedDimensions = usedJoinable.JoinableCohortAggregateConfiguration.AggregateConfiguration
                .AggregateDimensions.Cast<IColumn>().ToArray();

            //change the SelectSQL to the table alias of the joinable used (see CohortQueryBuilder.AddJoinablesToBuilder)
            foreach (var dimension in hackedDimensions)
                dimension.SelectSQL = $"{tableAlias}.{dimension.GetRuntimeName()}";

            toReturn.AddRange(hackedDimensions);
        }

        return toReturn.ToArray();
    }

    /// <inheritdoc />
    public bool ShouldBeEnabled(AggregateEditorSection section, AggregateConfiguration aggregate)
    {
        return section switch
        {
            AggregateEditorSection.Extractable => false,
            AggregateEditorSection.TOPX => true,
            AggregateEditorSection.PIVOT => false,
            AggregateEditorSection.AXIS => false,
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable[] GetAvailableJoinables(AggregateConfiguration aggregate)
    {
        var existingForcedJoinTables = aggregate.ForcedJoins;

        var existingDimensions = aggregate.AggregateDimensions;
        var existingTablesAlreadyReferenced = existingDimensions.Select(d => d.ColumnInfo.TableInfo).Distinct();

        var availableTableInfos = aggregate.Catalogue.GetTableInfoList(true);

        var toReturn = new List<IMapsDirectlyToDatabaseTable>();

        //They can add TableInfos that have not been referenced yet by the columns or already been configured as an explicit force join
        toReturn.AddRange(availableTableInfos.Except(existingTablesAlreadyReferenced.Union(existingForcedJoinTables)));

        //if it is a patient index table itself then that's all folks
        if (aggregate.IsJoinablePatientIndexTable())
            return toReturn.ToArray();

        //it's not a patient index table itself so it can reference other patient index tables in the configuration
        var config = aggregate.GetCohortIdentificationConfigurationIfAny() ?? throw new NotSupportedException(
            $"Aggregate {aggregate} did not return its CohortIdentificationConfiguration correctly, did someone delete the configuration or Orphan this AggregateConfiguration while you weren't looking?");

        //find those that are already referenced
        var existingJoinables = aggregate.PatientIndexJoinablesUsed.Select(u => u.JoinableCohortAggregateConfiguration);

        //return also these which are available for use but not yet linked in
        toReturn.AddRange(config.GetAllJoinables().Except(existingJoinables));

        return toReturn.ToArray();
    }

    /// <inheritdoc />
    public ISqlParameter[] GetAllParameters(AggregateConfiguration aggregate)
    {
        var parameterManager = new ParameterManager();
        foreach (var p in _globals)
            parameterManager.AddGlobalParameter(p);

        parameterManager.AddParametersFor(aggregate, ParameterLevel.QueryLevel);

        return parameterManager.GetFinalResolvedParametersList().ToArray();
    }

    /// <inheritdoc />
    public CountColumnRequirement GetCountColumnRequirement(AggregateConfiguration aggregate)
    {
        return aggregate.IsJoinablePatientIndexTable()
            ? CountColumnRequirement.CanOptionallyHaveOne
            : CountColumnRequirement.CannotHaveOne;
    }
}