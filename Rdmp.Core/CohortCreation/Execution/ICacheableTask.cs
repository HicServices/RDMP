// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
///     Any ICompileable which can be cached once finished.  Typically any ICompileable in a CohortCompiler can be cached
///     unless it is composed of multiple discrete
///     sub queries (i.e. an AggregationContainerTask.)
/// </summary>
public interface ICacheableTask : ICompileable
{
    AggregateConfiguration GetAggregateConfiguration();
    CacheCommitArguments GetCacheArguments(string sql, DataTable results, DatabaseColumnRequest[] explicitTypes);
    void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager);

    bool IsCacheableWhenFinished();
    bool CanDeleteCache();
}