using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryCaching.Aggregation;
using ReusableLibraryCode.Icons.IconProvision;
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
            }
            if(cic.RootCohortAggregateContainer_ID == null)
            {
                SetImpossible($"CohortIdentificationConfiguration {cic} has no root container");
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

            foreach(var ag in _cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively())
            {
                // lets just be brutal and delete all roles
                cacheManager.DeleteCacheEntryIfAny(ag, AggregateOperation.ExtractableAggregateResults);
                cacheManager.DeleteCacheEntryIfAny(ag, AggregateOperation.JoinableInceptionQuery);
            }
        }

    }
}
