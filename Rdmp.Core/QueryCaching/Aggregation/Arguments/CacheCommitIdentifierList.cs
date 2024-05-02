// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.QueryCaching.Aggregation.Arguments;

/// <summary>
///     Request to cache an AggregateConfiguration that is a cohort identifier list subquery from a
///     CohortIdentificationConfiguration (it is a query that
///     identifies patients fitting certain criteria e.g. 'patients with HBA1c biochemistry results > 50').
///     <para>Serves as an input to CachedAggregateConfigurationResultsManager.</para>
/// </summary>
public class CacheCommitIdentifierList : CacheCommitArguments
{
    private readonly DatabaseColumnRequest _identifierColumn;

    public CacheCommitIdentifierList(AggregateConfiguration configuration, string sql, DataTable results,
        DatabaseColumnRequest identifierColumn, int timeout)
        : base(AggregateOperation.IndexedExtractionIdentifierList, configuration, sql, results, timeout,
            new[] { identifierColumn })
    {
        //advise them if they are trying to cache an identifier list but the DataTable has more than 1 column
        if (results.Columns.Count != 1)
            throw new NotSupportedException(
                $"The DataTable did not have exactly 1 column (it had {results.Columns.Count} columns).  This makes it incompatible with committing to the Cache as an IdentifierList");

        //advise them if they are trying to cache a cache query itself!
        if (sql.Trim().StartsWith(CachedAggregateConfigurationResultsManager.CachingPrefix))
            throw new NotSupportedException(
                $"Sql for the query started with '{CachedAggregateConfigurationResultsManager.CachingPrefix}' which implies you ran some SQL code to fetch some stuff from the cache and then committed it back into the cache (obliterating the record of what the originally executed query was).  This is referred to as Inception Caching and isn't allowed.  Note to developers: this happens if user caches a query then runs the query again (fetching it from the cache) and somehow tries to commit the cache fetch request back into the cache as an overwrite");

        //throw away nulls
        foreach (var r in results.Rows.Cast<DataRow>().ToArray())
            if (r[0] == null || r[0] == DBNull.Value)
                results.Rows.Remove(r);

        _identifierColumn = identifierColumn ??
                            throw new Exception(
                                "You must specify the data type of the identifier column, identifierColumn was null");
        _identifierColumn.AllowNulls = false;
        _identifierColumn.ColumnName = results.Columns[0].ColumnName;
    }

    public override void CommitTableDataCompleted(DiscoveredTable resultingTable)
    {
        //if user has an explicit type to use for the column (probably a good idea to have all extraction idetntifiers of the same data type
        var col = resultingTable.DiscoverColumn(_identifierColumn.ColumnName);

        CreateIndex(resultingTable, col, Configuration.ToString());
    }


    private static void CreateIndex(DiscoveredTable table, DiscoveredColumn onColumn, string configurationName)
    {
        try
        {
            table.CreatePrimaryKey(onColumn);
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Failed to create unique primary key on the results of AggregateConfiguration {configurationName}", e);
        }
    }
}