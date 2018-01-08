using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;

namespace RDMPAutomationService.Logic.DLE
{
    /// <summary>
    /// Identifies data loads (LoadMetadatas) which are due to run (and not already executing/crashed/locked etc).  Used by DLEAutomationSource to 
    /// identify when a new AutomatedDLELoad can be started.
    /// </summary>
    public class DLERunFinder
    {
        private readonly ICatalogueRepository _catalogueRepository;

        public DLERunFinder(ICatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }
        public LoadPeriodically SuggestLoad()
        {
            var lockedCatalogues = _catalogueRepository.GetAllAutomationLockedCatalogues();

            //refresh list of push jobs
            return _catalogueRepository.GetAllObjects<LoadPeriodically>().FirstOrDefault(p => p.IsLoadDue(lockedCatalogues));
        }

        public LoadProgress SuggestLoadBecauseCacheAvailable()
        {
            var cacheProgresses = _catalogueRepository.GetAllObjects<CacheProgress>();
            var lockedCatalogues = _catalogueRepository.GetAllAutomationLockedCatalogues();
            
            foreach (CacheProgress cp in cacheProgresses)
            {
               var dtCache = cp.CacheFillProgress;
               var loadProgress = cp.LoadProgress;
               

                //LoadProgress isn't allowed to run through automation anyway
                if(!loadProgress.AllowAutomation)
                    continue;

                //It's already locked (either running or hard crashed)
                if(loadProgress.LockedBecauseRunning)
                    continue;

                //Permission window is locked 
                if(cp.PermissionWindow_ID != null)
                    if(cp.PermissionWindow.LockedBecauseRunning)
                        continue;

               //Cache has never been loaded
               if(dtCache == null)
                   continue;

                var dtLoadProgress = loadProgress.DataLoadProgress;
                
                //Never loaded the LoadProgress... probably don't start now in automation, that's a bad idea.  If user wants to load it they can run load checks which will suggest to the user they set it to the OriginDate anyway
                if(dtLoadProgress == null)
                    continue;

                int daysToLoad = loadProgress.DefaultNumberOfDaysToLoadEachTime;

                if (daysToLoad == 0)
                    continue;

                if (dtLoadProgress.Value.AddDays(daysToLoad) <= dtCache)
                    if(!LocksPreventLoading(loadProgress,lockedCatalogues))
                        return loadProgress;
            }

            //No tasks are ready to go
            return null;
        }

        private bool LocksPreventLoading(LoadProgress cacheProgress, Catalogue[] lockedCatalogues)
        {
            var loadCatalogues = cacheProgress.LoadMetadata.GetAllCatalogues();

            //if any of the catalogues that participate in the load are locked in another automation task (could be DQE or cache download even!)
            return loadCatalogues.Any(lockedCatalogues.Contains);
        }
    }
}
