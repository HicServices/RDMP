// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CohortCommitting.Pipeline.Sources;

/// <summary>
///     Pipeline source component which executes an AggregateConfiguration query in a CohortIdentificationConfiguration
///     which has the role of
///     'Patient Index Table' (JoinableCohortAggregateConfiguration).  A 'Patient Index Table' is what researchers call any
///     table with information
///     about patients (e.g. a table containing every prescription date for a given drug) in which the data (not patient
///     identifiers) is directly
///     used to identify their cohort (e.g. cohort query is 'everyone who has been hospitalised with code X within 6 months
///     of having a prescription
///     of drug Y - in this case the patient index table is 'the prescribed dates of drug Y').
///     <para>
///         Since 'Patient Index Tables' always contain a superset of the final identifiers this component will add an
///         additional filter to the query
///         to restrict rows returned only to those patients in your final cohort list (you must already have a committed
///         final cohort list to use this
///         component).  This prevents you saving a snapshot of 1,000,000 prescription dates when your final cohort of
///         patients only own 500 of those
///         records (because the cohort identification configuration includes further set operations that reduce the
///         patient count beyond the prescribed drug Y).
///     </para>
///     <para>
///         The purpose of all this is usually to ship a table ('Patient Index Table') which was used to build the
///         researchers cohort into the saved cohorts
///         database so it can be linked and extracted (as custom data) along with all the normal datasets that make up the
///         researchers extract.
///     </para>
/// </summary>
public class PatientIndexTableSource : AggregateConfigurationTableSource, IPipelineRequirement<ExtractableCohort>
{
    private ExtractableCohort _extractableCohort;

    protected override string GetSQL()
    {
        var builder = new CohortQueryBuilder(AggregateConfiguration,
            CohortIdentificationConfigurationIfAny.GetAllParameters(), null);

        var sql = builder.SQL;

        var extractionIdentifier = AggregateConfiguration.AggregateDimensions.Single(d => d.IsExtractionIdentifier);

        //IMPORTANT: We are using impromptu SQL instead of a Spontaneous container / CustomLine because we want the CohortQueryBuilder to decide to use
        //the cached table data (if any).  If it senses we are monkeying with the query it will run it verbatim which will be very slow.

        var whereString = AggregateConfiguration.RootFilterContainer_ID != null ? "AND " : "WHERE ";

        var impromptuSql =
            $"{whereString}{extractionIdentifier.SelectSQL} IN (SELECT {_extractableCohort.GetPrivateIdentifier()} FROM {_extractableCohort.ExternalCohortTable.TableName} WHERE {_extractableCohort.WhereSQL()})";

        //if there is a group by then we must insert the AND patient in cohort bit before the group by but after any WHERE containers
        var insertionPoint = sql.IndexOf("group by", 0, StringComparison.CurrentCultureIgnoreCase);

        //if there isn't a group by
        return insertionPoint == -1
            ? $"{sql}{Environment.NewLine}{impromptuSql}"
            :
            //there is a group by
            $"{sql[..insertionPoint]}{Environment.NewLine}{impromptuSql}{Environment.NewLine}{sql[insertionPoint..]}";
    }

    public void PreInitialize(ExtractableCohort value, IDataLoadEventListener listener)
    {
        _extractableCohort = value;
    }

    public override void PreInitialize(AggregateConfiguration value, IDataLoadEventListener listener)
    {
        base.PreInitialize(value, listener);

        if (CohortIdentificationConfigurationIfAny == null)
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Configuration {AggregateConfiguration} is not a valid input because it does not have a CohortIdentificationConfiguration never mind a JoinableCohortAggregateConfiguration.  Maybe it isn't a patient index table?"));
    }
}