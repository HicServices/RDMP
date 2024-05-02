// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Caching.Requests;

/// <summary>
///     ICacheFetchRequest representing an 'out of order' fetch for a specific time/date range.  This will not affect the
///     head of an ICacheProgress (how far we think we
///     have loaded) and is intended to back fill gaps in a cache that is already populated.
/// </summary>
public class BackfillCacheFetchRequest : ICacheFetchRequest
{
    [NoMappingToDatabase] public IRepository Repository { get; set; }

    public DateTime Start { get; set; }
    public TimeSpan ChunkPeriod { get; set; }
    public IPermissionWindow PermissionWindow { get; set; }
    public ICacheProgress CacheProgress { get; set; }
    public bool IsRetry { get; }

    public DateTime End => Start.Add(ChunkPeriod);

    public BackfillCacheFetchRequest(IRepository repository, DateTime start)
    {
        Repository = repository;
        Start = start;
        IsRetry = false;
    }

    public void SaveCacheFillProgress(DateTime cacheFillProgress)
    {
        // do nothing, backfill should not affect the cache
    }

    public void RequestFailed(Exception e)
    {
        // do nothing, backfill should not affect the cache
    }

    public void RequestSucceeded()
    {
        // do nothing, backfill should not affect the cache
    }

    /// <summary>
    ///     Factory method which creates the 'next' logical fetch request using the request's chunk period
    /// </summary>
    /// <returns></returns>
    public ICacheFetchRequest GetNext()
    {
        var nextStart = Start.Add(ChunkPeriod);
        return new BackfillCacheFetchRequest(Repository, nextStart)
        {
            CacheProgress = CacheProgress,
            PermissionWindow = PermissionWindow,
            ChunkPeriod = ChunkPeriod
        };
    }
}