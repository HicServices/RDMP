// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data.Cache;

/// <summary>
///     Records the progress of fetching and caching data from a remote source e.g. a Webservice or Imaging file host.
///     Each CacheProgress
///     is tied to a LoadProgress (which itself is tied to a LoadMetadata).
/// </summary>
public class CacheProgress : DatabaseEntity, ICacheProgress
{
    #region Database Properties

    private string _name;
    private int _loadProgressID;
    private int? _permissionWindowID;
    private DateTime? _cacheFillProgress;
    private string _cacheLagPeriod;
    private int? _pipelineID;
    private TimeSpan _chunkPeriod;
    private string _cacheLagPeriodLoadDelay;

    /// <inheritdoc />
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc />
    public int LoadProgress_ID
    {
        get => _loadProgressID;
        set => SetField(ref _loadProgressID, value);
    }

    /// <inheritdoc />
    public int? PermissionWindow_ID
    {
        get => _permissionWindowID;
        set => SetField(ref _permissionWindowID, value);
    }

    /// <inheritdoc />
    public DateTime? CacheFillProgress
    {
        get => _cacheFillProgress;
        set => SetField(ref _cacheFillProgress, value);
    }

    /// <inheritdoc />
    public string CacheLagPeriod
    {
        get => _cacheLagPeriod;
        set => SetField(ref _cacheLagPeriod, value);
    } // stored as string in DB, use GetCacheLagPeriod() to get as CacheLagPeriod

    /// <inheritdoc />
    public int? Pipeline_ID
    {
        get => _pipelineID;
        set => SetField(ref _pipelineID, value);
    }

    /// <inheritdoc />
    public TimeSpan ChunkPeriod
    {
        get => _chunkPeriod;
        set => SetField(ref _chunkPeriod, value);
    }

    /// <inheritdoc />
    public string CacheLagPeriodLoadDelay
    {
        get => _cacheLagPeriodLoadDelay;
        set => SetField(ref _cacheLagPeriodLoadDelay, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc />
    [NoMappingToDatabase]
    public IEnumerable<ICacheFetchFailure> CacheFetchFailures =>
        Repository.GetAllObjectsWithParent<CacheFetchFailure>(this);

    /// <inheritdoc cref="ICacheProgress.LoadProgress_ID" />
    [NoMappingToDatabase]
    public ILoadProgress LoadProgress => Repository.GetObjectByID<LoadProgress>(LoadProgress_ID);

    /// <inheritdoc cref="ICacheProgress.Pipeline_ID" />
    [NoMappingToDatabase]
    public IPipeline Pipeline => Pipeline_ID == null ? null : Repository.GetObjectByID<Pipeline>((int)Pipeline_ID);

    /// <inheritdoc cref="ICacheProgress.PermissionWindow_ID" />
    [NoMappingToDatabase]
    public IPermissionWindow PermissionWindow =>
        PermissionWindow_ID == null
            ? null
            : Repository.GetObjectByID<PermissionWindow>((int)PermissionWindow_ID);

    #endregion

    /// <inheritdoc cref="ICacheProgress.CacheLagPeriod" />
    public CacheLagPeriod GetCacheLagPeriod()
    {
        return string.IsNullOrWhiteSpace(CacheLagPeriod) ? null : new CacheLagPeriod(CacheLagPeriod);
    }

    /// <inheritdoc cref="ICacheProgress.CacheLagPeriodLoadDelay" />
    public CacheLagPeriod GetCacheLagPeriodLoadDelay()
    {
        return string.IsNullOrWhiteSpace(CacheLagPeriodLoadDelay)
            ? Cache.CacheLagPeriod.Zero
            : new CacheLagPeriod(CacheLagPeriodLoadDelay);
    }

    /// <inheritdoc cref="ICacheProgress.CacheLagPeriod" />
    public void SetCacheLagPeriod(CacheLagPeriod cacheLagPeriod)
    {
        CacheLagPeriod = cacheLagPeriod == null ? "" : cacheLagPeriod.ToString();
    }

    /// <inheritdoc cref="ICacheProgress.CacheLagPeriodLoadDelay" />
    public void SetCacheLagPeriodLoadDelay(CacheLagPeriod cacheLagLoadDelayPeriod)
    {
        CacheLagPeriodLoadDelay = cacheLagLoadDelayPeriod == null ? "" : cacheLagLoadDelayPeriod.ToString();
    }

    public CacheProgress()
    {
    }

    /// <summary>
    ///     Defines that the given <see cref="LoadProgress" /> is a DLE data load that is driven by reading data from a cache.
    ///     The instance created can be used
    ///     to describe which pipeline should be run to fill that cache, the period that has been fetched from the remote
    ///     endpoint so far etc.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="loadProgress"></param>
    public CacheProgress(ICatalogueRepository repository, ILoadProgress loadProgress)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "LoadProgress_ID", loadProgress.ID },
            { "Name", $"New CacheProgress {Guid.NewGuid()}" }
        });
    }

    internal CacheProgress(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        LoadProgress_ID = int.Parse(r["LoadProgress_ID"].ToString());
        PermissionWindow_ID = ObjectToNullableInt(r["PermissionWindow_ID"]);
        CacheFillProgress = ObjectToNullableDateTime(r["CacheFillProgress"]);
        CacheLagPeriod = r["CacheLagPeriod"] as string;
        CacheLagPeriodLoadDelay = r["CacheLagPeriodLoadDelay"] as string;
        Pipeline_ID = ObjectToNullableInt(r["Pipeline_ID"]);
        ChunkPeriod = (TimeSpan)r["ChunkPeriod"];
        Name = r["Name"].ToString();
    }

    /// <inheritdoc />
    public IEnumerable<ICacheFetchFailure> FetchPage(int start, int batchSize)
    {
        var toReturnIds = new List<int>();

        using (var conn = ((CatalogueRepository)Repository).GetConnection())
        {
            using var cmd =
                DatabaseCommandHelper.GetCommand($@"SELECT ID FROM CacheFetchFailure 
WHERE CacheProgress_ID = @CacheProgressID AND ResolvedOn IS NULL
ORDER BY FetchRequestStart
OFFSET {start} ROWS
FETCH NEXT {batchSize} ROWS ONLY", conn.Connection, conn.Transaction);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@CacheProgressID", cmd, ID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                toReturnIds.Add(Convert.ToInt32(reader["ID"]));
        }

        return Repository.GetAllObjectsInIDList<CacheFetchFailure>(toReturnIds);
    }

    /// <inheritdoc />
    public TimeSpan GetShortfall()
    {
        var lag = GetCacheLagPeriod();
        var lastExpectedCacheDate = lag?.CalculateStartOfLagPeriodFrom(DateTime.Today) ?? DateTime.Today;

        TimeSpan shortfall;
        if (CacheFillProgress != null)
        {
            shortfall = lastExpectedCacheDate.Subtract(CacheFillProgress.Value);
        }
        else
        {
            var load = LoadProgress;
            if (load.DataLoadProgress.HasValue)
                shortfall = lastExpectedCacheDate.Subtract(load.DataLoadProgress.Value);
            else if (load.OriginDate.HasValue)
                shortfall = lastExpectedCacheDate.Subtract(load.OriginDate.Value);
            else
                throw new NotSupportedException("Unknown (no cache progress or load origin date)");
        }

        return shortfall;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Returns all the Catalogues in this caches LoadMetadata OR if it has a PermissionWindow then ALL the unique
    ///     catalogues across ALL the LoadMetadatas of ANY cache that uses the same permission window
    /// </summary>
    /// <returns></returns>
    public Catalogue[] GetAllCataloguesMaximisingOnPermissionWindow()
    {
        return PermissionWindow_ID == null
            ? LoadProgress.LoadMetadata.GetAllCatalogues().Cast<Catalogue>().Distinct().ToArray()
            : PermissionWindow.CacheProgresses.SelectMany(p => p.LoadProgress.LoadMetadata.GetAllCatalogues())
                .Cast<Catalogue>().Distinct().ToArray();
    }

    /// <summary>
    ///     Returns
    /// </summary>
    /// <returns></returns>
    public DiscoveredServer GetDistinctLoggingDatabase()
    {
        return CatalogueRepository.GetDefaultLogManager()?.Server ??
               throw new Exception("No default logging server configured");
    }

    public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
    {
        serverChosen = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        return GetDistinctLoggingDatabase();
    }

    /// <summary>
    ///     Returns the unique logging name for <see cref="ArchivalDataLoadInfo" /> runs of this cache
    /// </summary>
    /// <returns></returns>
    public string GetLoggingRunName()
    {
        return $"Caching {Name}";
    }


    public string GetDistinctLoggingTask()
    {
        return "caching";
    }

    public IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs)
    {
        var name = GetLoggingRunName();

        return runs.Where(r => r.Description?.Equals(name, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }
}