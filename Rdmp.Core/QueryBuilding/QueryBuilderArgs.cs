// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort.Joinables;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Input class for <see cref="CohortQueryBuilderHelper" /> that assists in building the Sql query for a single cohort
///     set in a <see cref="AggregateConfiguration" />
///     This can include arbitrary hacks like replacing the patient identifier with * and applying TopX etc (see base class
///     <see cref="QueryBuilderCustomArgs" />).
/// </summary>
public class QueryBuilderArgs : QueryBuilderCustomArgs
{
    public JoinableCohortAggregateConfigurationUse JoinIfAny { get; }
    public AggregateConfiguration JoinedTo { get; }
    public CohortQueryBuilderDependencySql JoinSql { get; }
    public ISqlParameter[] Globals { get; }

    /// <summary>
    ///     Creates basic arguments for an <see cref="AggregateConfiguration" /> that does not have a join to a patient index
    ///     table
    /// </summary>
    public QueryBuilderArgs(QueryBuilderCustomArgs customisations, ISqlParameter[] globals)
    {
        customisations?.Populate(this);
        Globals = globals ?? Array.Empty<ISqlParameter>();
    }

    /// <summary>
    ///     Creates arguments for an <see cref="AggregateConfiguration" /> which has a JOIN to a patient index table.  All
    ///     arguments must be provided
    /// </summary>
    /// <param name="join">The join usage relationship object (includes join direction etc)</param>
    /// <param name="joinedTo">
    ///     The patient index to which the join is made to (e.g.
    ///     <see cref="JoinableCohortAggregateConfiguration.AggregateConfiguration" />)
    /// </param>
    /// <param name="joinSql">The full SQL of the join</param>
    /// <param name="customisations"></param>
    /// <param name="globals"></param>
    public QueryBuilderArgs(JoinableCohortAggregateConfigurationUse join, AggregateConfiguration joinedTo,
        CohortQueryBuilderDependencySql joinSql, QueryBuilderCustomArgs customisations,
        ISqlParameter[] globals) : this(customisations, globals)
    {
        JoinIfAny = join;
        JoinedTo = joinedTo;
        JoinSql = joinSql;

        if (JoinIfAny == null != (JoinedTo == null) || JoinIfAny == null != (JoinSql == null))
            throw new Exception("You must provide all arguments or no arguments");

        if (JoinedTo != null)
            if (!JoinedTo.IsCohortIdentificationAggregate || !JoinedTo.IsJoinablePatientIndexTable())
                throw new ArgumentException($"JoinedTo ({JoinedTo}) was not a patient index table", nameof(joinedTo));
    }
}