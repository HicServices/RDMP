// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Caching.Requests;

/// <summary>
///     Describes a normal caching request for a period of days/hours for an ICacheSource to request.  This includes start
///     / end date which will be the next logical period
///     of time to fetch to advance the head of an ICacheProgress (fetch the next date range and update the progress
///     pointer).
/// </summary>
public class CacheFetchRequest : ICacheFetchRequest
{
    [NoMappingToDatabase] public IRepository Repository { get; set; }

    public DateTime Start { get; set; }
    public DateTime End => Start.Add(ChunkPeriod);
    public TimeSpan ChunkPeriod { get; set; }
    public IPermissionWindow PermissionWindow { get; set; }
    public ICacheProgress CacheProgress { get; set; }
    public ICacheFetchFailure PreviousFailure { get; set; }


    /// <summary>
    ///     Is this CacheFetchRequest a retry of a previously failed fetch request?
    /// </summary>
    public bool IsRetry => PreviousFailure != null;

    public CacheFetchRequest(IRepository repository, DateTime start)
    {
        Repository = repository;
        Start = start;
        PreviousFailure = null;
    }

    public CacheFetchRequest(IRepository repository) : this(repository, DateTime.MinValue)
    {
    }

    /// <summary>
    ///     Creates a CacheFetchRequest from a previous failure.
    /// </summary>
    /// <param name="cacheFetchFailure"></param>
    /// <param name="cacheProgress"></param>
    public CacheFetchRequest(ICacheFetchFailure cacheFetchFailure, ICacheProgress cacheProgress)
    {
        Start = cacheFetchFailure.FetchRequestStart;
        CacheProgress = cacheProgress;
        ChunkPeriod = cacheFetchFailure.FetchRequestEnd.Subtract(cacheFetchFailure.FetchRequestStart);
        PermissionWindow = cacheProgress.PermissionWindow;
        PreviousFailure = cacheFetchFailure;
    }

    public void SaveCacheFillProgress(DateTime cacheFillProgress)
    {
        CacheProgress.CacheFillProgress = cacheFillProgress;
        CacheProgress.SaveToDatabase();
    }

    public void RequestFailed(Exception e)
    {
        if (PreviousFailure != null)
        {
            PreviousFailure.LastAttempt = DateTime.Now;
            PreviousFailure.SaveToDatabase();
        }
        else
        {
            _ = new CacheFetchFailure((ICatalogueRepository)Repository, CacheProgress, Start, End, e);
        }
    }

    public void RequestSucceeded()
    {
        if (IsRetry)
            PreviousFailure.Resolve();
        else
            SaveCacheFillProgress(End);
    }

    /// <summary>
    ///     Factory method which creates the 'next' logical fetch request using the request's chunk period
    /// </summary>
    /// <returns></returns>
    public ICacheFetchRequest GetNext()
    {
        var nextStart = Start.Add(ChunkPeriod);
        return new CacheFetchRequest(Repository, nextStart)
        {
            CacheProgress = CacheProgress,
            PermissionWindow = PermissionWindow,
            ChunkPeriod = ChunkPeriod
        };
    }
}