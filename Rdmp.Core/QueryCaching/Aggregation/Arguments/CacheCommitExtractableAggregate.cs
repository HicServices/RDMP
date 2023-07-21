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
///     Request to cache an AggregateConfiguration that results in a DataTable suitable for producing a useful graph (e.g.
///     'number of records per year in
///     Biochemistry by healthboard').  Should not contain patient identifiers.
///     <para>Serves as an input to CachedAggregateConfigurationResultsManager.</para>
/// </summary>
public class CacheCommitExtractableAggregate : CacheCommitArguments
{
    public CacheCommitExtractableAggregate(AggregateConfiguration configuration, string sql, DataTable results,
        int timeout)
        : base(AggregateOperation.ExtractableAggregateResults, configuration, sql, results, timeout)
    {
        if (results.Columns.Count == 0)
            throw new ArgumentException(
                $"The DataTable that you claimed was an {Operation} had zero columns and therefore cannot be cached");

        var suspectDimensions =
            configuration.AggregateDimensions
                .Where(d => d.IsExtractionIdentifier || d.HashOnDataRelease)
                .Select(d => d.GetRuntimeName())
                .ToArray();
        if (suspectDimensions.Any())
            throw new NotSupportedException(
                $"Aggregate {configuration} contains dimensions marked as IsExtractionIdentifier or HashOnDataRelease ({string.Join(",", suspectDimensions)}) so the aggregate cannot be cached.  This would/could result in private patient identifiers appearing on your website!");

        if (!configuration.IsExtractable)
            throw new NotSupportedException(
                $"Aggregate {configuration} is not marked as IsExtractable therefore cannot be cached for publication on website");
    }

    public override void CommitTableDataCompleted(DiscoveredTable resultingTable)
    {
        //no need to do anything here we don't need index or anything else
    }
}