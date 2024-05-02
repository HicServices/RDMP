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

public abstract class CacheableTask : Compileable, ICacheableTask
{
    protected CacheableTask(CohortCompiler compiler) : base(compiler)
    {
    }

    public abstract AggregateConfiguration GetAggregateConfiguration();

    public abstract CacheCommitArguments GetCacheArguments(string sql, DataTable results,
        DatabaseColumnRequest[] explicitTypes);

    public abstract void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager);

    public bool IsCacheableWhenFinished()
    {
        if (!_compiler.Tasks.ContainsKey(this))
            return false;

        var execution = _compiler.Tasks[this];
        return execution != null && execution.SubQueries > execution.SubqueriesCached;
    }

    public bool CanDeleteCache()
    {
        return _compiler.Tasks[this].SubqueriesCached > 0;
    }
}