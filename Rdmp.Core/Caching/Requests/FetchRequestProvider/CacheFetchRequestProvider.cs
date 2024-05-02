// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Requests.FetchRequestProvider;

public class CacheFetchRequestProvider : ICacheFetchRequestProvider
{
    protected ICacheProgress CacheProgress { get; }
    public IPermissionWindow PermissionWindow { get; set; }

    private ICacheFetchRequest _initialRequest;
    public ICacheFetchRequest Current { get; private set; }

    /// <summary>
    ///     Sets up the class to generate <see cref="ICacheFetchRequest" /> for the given <see cref="ICacheProgress" />
    /// </summary>
    /// <param name="cacheProgress">
    ///     The cache which the request is for, this must have either an <see cref="ICacheProgress.CacheFillProgress" /> or its
    ///     <see cref="ILoadProgress" /> parent must have a populated <see cref="ILoadProgress.OriginDate" />
    /// </param>
    public CacheFetchRequestProvider(ICacheProgress cacheProgress)
    {
        CacheProgress = cacheProgress;
        PermissionWindow = CacheProgress.PermissionWindow;
    }

    /// <summary>
    ///     Creates a new <see cref="ICacheFetchRequest" /> with a valid start date but no End.  To hydrate end you should use
    ///     a <see cref="ICacheFetchRequestProvider" />
    /// </summary>
    /// <returns></returns>
    public virtual ICacheFetchRequest CreateInitialRequest()
    {
        var lp = CacheProgress.LoadProgress;

        // Figure out when to start loading from
        DateTime startDate;
        if (CacheProgress.CacheFillProgress.HasValue)
            startDate = CacheProgress.CacheFillProgress.Value;
        else if (lp.OriginDate.HasValue)
            startDate = lp.OriginDate.Value;
        else
            throw new Exception(
                "Don't know when to begin loading the cache from. Neither CacheProgress or LoadProgress has a relevant date.");

        var initialFetchRequest = new CacheFetchRequest(CacheProgress.Repository)
        {
            CacheProgress = CacheProgress,
            ChunkPeriod = CacheProgress.ChunkPeriod,
            PermissionWindow = PermissionWindow,
            Start = startDate
        };

        return initialFetchRequest;
    }

    /// <summary>
    ///     Returns the next <see cref="ICacheFetchRequest" /> based on the <see cref="Current" />
    /// </summary>
    /// <param name="listener"></param>
    /// <returns></returns>
    public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
    {
        return _initialRequest == null
            ? Current = _initialRequest = CreateInitialRequest()
            : Current = CreateNext();
    }

    private ICacheFetchRequest CreateNext()
    {
        var nextStart = Current.Start.Add(Current.ChunkPeriod);
        return new CacheFetchRequest(CacheProgress.Repository, nextStart)
        {
            CacheProgress = Current.CacheProgress,
            PermissionWindow = Current.PermissionWindow,
            ChunkPeriod = Current.ChunkPeriod
        };
    }
}