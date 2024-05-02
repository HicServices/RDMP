// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Cache;

/// <inheritdoc cref="ILoggedActivityRootObject" />
public interface ICacheProgress : INamed, ILoggedActivityRootObject
{
    /// <summary>
    ///     <para>
    ///         The amount of time that has to have passed beyond the <see cref="CacheLagPeriod" /> before a fetch will be
    ///         initiated.
    ///     </para>
    ///     <para>Stored as string in DB, use GetCacheLagPeriod() to get as CacheLagPeriod</para>
    /// </summary>
    string CacheLagPeriodLoadDelay { get; }

    /// <summary>
    ///     The LoadProgress which is responsible for loading data from this cache.
    /// </summary>
    int LoadProgress_ID { get; set; }

    /// <summary>
    ///     When we can run the cache (e.g. evenings only).  Also serves as the locking object for preventing multiple caching
    ///     engines/automation servers from executing
    ///     caching activities belonging to the same window.
    /// </summary>
    int? PermissionWindow_ID { get; set; }

    /// <summary>
    ///     How far through the caching activity are we? files up to this date should be on disk in archives
    /// </summary>
    DateTime? CacheFillProgress { get; set; }

    /// <summary>
    ///     The period of time e.g. 1m that is never fetched expressed as an offset from DateTime.Now.  This handles the case
    ///     where the cache source
    ///     is not real time i.e. we expect it to take at least 1 month for data to appear on the cache endpoint so don't make
    ///     requests for data that would
    ///     originate very recently.
    /// </summary>
    string CacheLagPeriod { get; set; }

    /// <summary>
    ///     The amount of time to request at a time from the cache source e.g. (fetch 6 hours of data at a time).
    /// </summary>
    TimeSpan ChunkPeriod { get; set; }

    /// <summary>
    ///     The pipeline configuration responsible for populating the cache.  This will be run repeatedly for each date range
    ///     fetched (See CachingEngine.Factories.CachingPipelineUseCase)
    /// </summary>
    int? Pipeline_ID { get; set; }

    /// <inheritdoc cref="Pipeline_ID" />
    IPipeline Pipeline { get; }

    /// <inheritdoc cref="ICacheProgress.PermissionWindow_ID" />
    IPermissionWindow PermissionWindow { get; }

    /// <summary>
    ///     Returns all failed cache requests documented.  This is an array of dates that were requested of the caching
    ///     endpoint but were no data was available due
    ///     to the endpoint reporting an Exception at the time it was requested.
    /// </summary>
    IEnumerable<ICacheFetchFailure> CacheFetchFailures { get; }

    /// <inheritdoc cref="LoadProgress_ID" />
    ILoadProgress LoadProgress { get; }

    /// <inheritdoc cref="ICacheProgress.CacheLagPeriod" />
    CacheLagPeriod GetCacheLagPeriod();

    /// <inheritdoc cref="ICacheProgress.CacheLagPeriod" />
    void SetCacheLagPeriod(CacheLagPeriod cacheLagPeriod);

    /// <inheritdoc cref="CacheLagPeriodLoadDelay" />
    CacheLagPeriod GetCacheLagPeriodLoadDelay();

    /// <inheritdoc cref="CacheLagPeriodLoadDelay" />
    void SetCacheLagPeriodLoadDelay(CacheLagPeriod cacheLagPeriod);

    /// <summary>
    ///     Returns a subset of CacheFetchFailures between the start and batch size.  Should return only unresolved
    ///     failures.
    ///     <para>
    ///         This differs from CacheFetchFailures since it ignores resolved failures and only returns 're-runnable'
    ///         ICacheFetchFailures
    ///     </para>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    IEnumerable<ICacheFetchFailure> FetchPage(int start, int batchSize);

    /// <summary>
    ///     Returns the maximum amount of time that could be loaded from this cache (based on the origin date of the dataset or
    ///     the max date of current data cached) and todays date (offset by lag period)
    ///     e.g. if the data has been cached to 2016-01-01 and todays date is 2016-01-05 then the TimeSpan will be +4 days.  A
    ///     Negative shortfall indicates that it has overcached ahead of the lag period or
    ///     it has cached future data (as in data from the future!).
    /// </summary>
    /// <returns></returns>
    TimeSpan GetShortfall();

    /// <summary>
    ///     Returns the unique logging name for <see cref="ArchivalDataLoadInfo" /> runs of this cache
    /// </summary>
    /// <returns></returns>
    string GetLoggingRunName();
}