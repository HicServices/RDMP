// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Clears cached cohort set data stored in an <see cref="ExternalDatabaseServer" /> that is acting
///     as a <see cref="QueryCachingPatcher" />
/// </summary>
public sealed class ExecuteCommandClearQueryCache : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;

    /// <summary>
    ///     Clears all cache entries in the cache used by <paramref name="cic" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="cic"></param>
    public ExecuteCommandClearQueryCache(IBasicActivateItems activator,
        [DemandsInitialization("The Cohort Builder query for which you want to invalidate all cache entries")]
        CohortIdentificationConfiguration cic) : base(activator)
    {
        _cic = cic;

        if (cic.QueryCachingServer_ID == null)
        {
            SetImpossible($"CohortIdentificationConfiguration {cic} does not have a query cache configured");
            return;
        }

        if (cic.RootCohortAggregateContainer_ID == null)
        {
            SetImpossible($"CohortIdentificationConfiguration {cic} has no root container");
            return;
        }

        if (GetCacheCount() == 0) SetImpossible($"There are no cache entries for {cic}");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return IconOverlayProvider.GetOverlayNoCache(Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer_Cache),
            OverlayKind.Delete);
    }

    public override void Execute()
    {
        base.Execute();

        var cacheManager = new CachedAggregateConfigurationResultsManager(_cic.QueryCachingServer);
        var deleted = 0;

        foreach (var ag in _cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively())
        {
            // just in case they changed the role or something weird we should nuke all its roles
            deleted += cacheManager.DeleteCacheEntryIfAny(ag, AggregateOperation.IndexedExtractionIdentifierList)
                ? 1
                : 0;
            deleted += cacheManager.DeleteCacheEntryIfAny(ag, AggregateOperation.JoinableInceptionQuery) ? 1 : 0;
        }

        foreach (var joinable in _cic.GetAllJoinables())
        {
            // just in case they changed the role or something weird we should nuke all its roles
            deleted += cacheManager.DeleteCacheEntryIfAny(joinable.AggregateConfiguration,
                AggregateOperation.IndexedExtractionIdentifierList)
                ? 1
                : 0;
            deleted += cacheManager.DeleteCacheEntryIfAny(joinable.AggregateConfiguration,
                AggregateOperation.JoinableInceptionQuery)
                ? 1
                : 0;
        }

        Show("Cache Entries Cleared", $"Deleted {deleted} cache entries");
    }


    private int GetCacheCount()
    {
        var cacheManager = new CachedAggregateConfigurationResultsManager(_cic.QueryCachingServer);
        var found = 0;

        foreach (var ag in _cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively())
        {
            // just incase they changed the role or something wierd we should nuke all its roles
            found += cacheManager.GetLatestResultsTableUnsafe(ag, AggregateOperation.IndexedExtractionIdentifierList) !=
                     null
                ? 1
                : 0;
            found += cacheManager.GetLatestResultsTableUnsafe(ag, AggregateOperation.JoinableInceptionQuery) != null
                ? 1
                : 0;
        }

        foreach (var joinable in _cic.GetAllJoinables())
        {
            // just incase they changed the role or something wierd we should nuke all its roles
            found += cacheManager.GetLatestResultsTableUnsafe(joinable.AggregateConfiguration,
                AggregateOperation.IndexedExtractionIdentifierList) != null
                ? 1
                : 0;
            found += cacheManager.GetLatestResultsTableUnsafe(joinable.AggregateConfiguration,
                AggregateOperation.JoinableInceptionQuery) != null
                ? 1
                : 0;
        }

        return found;
    }
}