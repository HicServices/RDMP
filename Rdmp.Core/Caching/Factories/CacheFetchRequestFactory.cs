// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Cache;

namespace Rdmp.Core.Caching.Factories
{
    /// <summary>
    /// Gets the next reasonable date range to fetch from an ICacheProgress.  This is usually CacheFillProgress + ChunkPeriod.  Does not take into account
    /// CacheLagPeriod / PermissionWindow Locking etc.
    /// </summary>
    public class CacheFetchRequestFactory
    {
        public ICacheFetchRequest Create(ICacheProgress cacheProgress, ILoadProgress loadProgress, IPermissionWindow permissionWindow)
        {
            // Figure out when to start loading from
            DateTime startDate;
            if (cacheProgress.CacheFillProgress.HasValue)
                startDate = cacheProgress.CacheFillProgress.Value;
            else if (loadProgress.OriginDate.HasValue)
                startDate = loadProgress.OriginDate.Value;
            else
                throw new Exception("Don't know when to begin loading the cache from. Neither CacheProgress or LoadProgress has a relevant date.");

            var initialFetchRequest = new CacheFetchRequest(loadProgress.Repository)
            {
                CacheProgress = cacheProgress,
                ChunkPeriod = cacheProgress.ChunkPeriod,
                PermissionWindow = permissionWindow,
                Start = startDate
            };

            return initialFetchRequest;
        }
    }
}