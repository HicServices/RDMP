// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.QueryCaching.Aggregation.Arguments;

/// <summary>
///     Request to cache an AggregateConfiguration that is a 'patient index table' (See
///     JoinableCohortAggregateConfiguration).  This will include patient
///     identifier and some useful columns (e.g. 'prescription dates for methadone by patient id').  The resulting cached
///     DataTable will be joined against
///     patient identifier lists to answer questions such as 'who has been hospitalised (SMR01) within 6 months of a
///     prescription for methadone'.
///     <para>
///         When doing such a join on two large datasets you can end up with a query that will never complete without
///         intermediate caching.
///     </para>
///     <para>Serves as an input to CachedAggregateConfigurationResultsManager.</para>
/// </summary>
public class CacheCommitJoinableInceptionQuery : CacheCommitArguments
{
    public CacheCommitJoinableInceptionQuery(AggregateConfiguration configuration, string sql, DataTable results,
        DatabaseColumnRequest[] explicitTypes, int timeout)
        : base(AggregateOperation.JoinableInceptionQuery, configuration, sql, results, timeout, explicitTypes)
    {
    }

    public override void CommitTableDataCompleted(DiscoveredTable resultingTable)
    {
    }
}