// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Threading.Tasks;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Caching.Requests.FetchRequestProvider;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching;

/// <summary>
///     Executes a caching configuration (ICacheProgress) for the specified arbitrary date/time range using either a
///     SingleDayCacheFetchRequestProvider or a
///     MultiDayCacheFetchRequestProvider.  Note that this uses a <see cref="BackfillCacheFetchRequest" /> which should
///     mean that the actual progress head pointer (how far
///     we think we have cached up to) does not change.  The reason we use a  <see cref="BackfillCacheFetchRequest" /> is
///     to allow us to run a given day/week again without
///     resetting our entire progress back to that date.
/// </summary>
public class CustomDateCaching
{
    private readonly ICacheProgress _cacheProgress;
    private readonly ICatalogueRepository _catalogueRepository;

    public CustomDateCaching(ICacheProgress cacheProgress, ICatalogueRepository catalogueRepository)
    {
        _cacheProgress = cacheProgress;
        _catalogueRepository = catalogueRepository;
    }

    public Task Fetch(DateTime startDate, DateTime endDate, GracefulCancellationToken token,
        IDataLoadEventListener listener, bool ignorePermissionWindow = false)
    {
        var dateToRetrieve = new DateTime(startDate.Year, startDate.Month, startDate.Day);
        var initialFetchRequest = new BackfillCacheFetchRequest(_catalogueRepository, dateToRetrieve)
        {
            CacheProgress = _cacheProgress,
            ChunkPeriod = _cacheProgress.ChunkPeriod
        };

        var requestProvider = startDate == endDate
            ? (ICacheFetchRequestProvider)new SingleDayCacheFetchRequestProvider(initialFetchRequest)
            : new MultiDayCacheFetchRequestProvider(initialFetchRequest, endDate);

        var factory = new CachingPipelineUseCase(_cacheProgress, ignorePermissionWindow, requestProvider);

        var engine = factory.GetEngine(listener);

        return Task.Factory.StartNew(() => engine.ExecutePipeline(token));
    }
}