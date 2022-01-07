using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryCaching.Aggregation;
using ReusableLibraryCode.Icons.IconProvision;
using System;
using System.Drawing;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Cleares cached cohort set data stored in an <see cref="ExternalDatabaseServer"/> that is acting
    /// as a <see cref="QueryCachingPatcher"/>
    /// </summary>
    public class ExecuteCommandClearQueryCache : BasicCommandExecution
    {
        private CohortIdentificationConfiguration _cic;

        /// <summary>
        /// Clears all cache entries in the cache used by <paramref name="cic"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="cic"></param>
        public ExecuteCommandClearQueryCache(IBasicActivateItems activator, 
            [DemandsInitialization("The Cohort Builder query for which you want to invalidate all cache entries")]
            CohortIdentificationConfiguration cic) :base(activator)
        {
            _cic = cic;

            if(cic.QueryCachingServer_ID == null)
            {
                SetImpossible($"CohortIdentificationConfiguration {cic} does not have a query cache configured");
                return;
            }
            if(cic.RootCohortAggregateContainer_ID == null)
            {
                SetImpossible($"CohortIdentificationConfiguration {cic} has no root container");
                return;
            }

            if(GetCacheCount() == 0)
            {
                SetImpossible($"There are no cache entries for {cic}");
            }
        }
        public override Image GetImage(IIconProvider iconProvider)
        {
            var overlayProvider = new IconOverlayProvider();
            return overlayProvider.GetOverlayNoCache(CatalogueIcons.ExternalDatabaseServer_Cache, OverlayKind.Delete);
        }
        public override void Execute()
        {
            base.Execute();

            var cacheManager = new CachedAggregateConfigurationResultsManager(_cic.QueryCachingServer);
            int deleted = 0;

            foreach(var ag in _cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively())
            {
                // just incase they changed the role or something wierd we should nuke all it's roles
                deleted += cacheManager.DeleteCacheEntryIfAny(ag, AggregateOperation.IndexedExtractionIdentifierList) ? 1 : 0;
                deleted += cacheManager.DeleteCacheEntryIfAny(ag, AggregateOperation.JoinableInceptionQuery) ? 1:0;
            }
            foreach(var joinable in _cic.GetAllJoinables())
            {
                // just incase they changed the role or something wierd we should nuke all it's roles
                deleted += cacheManager.DeleteCacheEntryIfAny(joinable.AggregateConfiguration, AggregateOperation.IndexedExtractionIdentifierList) ? 1 : 0;
                deleted += cacheManager.DeleteCacheEntryIfAny(joinable.AggregateConfiguration, AggregateOperation.JoinableInceptionQuery) ? 1 : 0;
            }
            
            Show("Cache Entries Cleared", $"Deleted {deleted} cache entries");
        }


        private int GetCacheCount()
        {
            var cacheManager = new CachedAggregateConfigurationResultsManager(_cic.QueryCachingServer);
            int found = 0;

            foreach (var ag in _cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively())
            {
                // just incase they changed the role or something wierd we should nuke all it's roles
                found += cacheManager.GetLatestResultsTableUnsafe(ag, AggregateOperation.IndexedExtractionIdentifierList) != null ? 1 : 0;
                found += cacheManager.GetLatestResultsTableUnsafe(ag, AggregateOperation.JoinableInceptionQuery) != null ? 1 : 0;
            }
            foreach (var joinable in _cic.GetAllJoinables())
            {
                // just incase they changed the role or something wierd we should nuke all it's roles
                found += cacheManager.GetLatestResultsTableUnsafe(joinable.AggregateConfiguration, AggregateOperation.IndexedExtractionIdentifierList) != null ? 1 : 0;
                found += cacheManager.GetLatestResultsTableUnsafe(joinable.AggregateConfiguration, AggregateOperation.JoinableInceptionQuery) != null ? 1 : 0;
            }

            return found;
        }

    }
}
