using System;
using System.Collections.Generic;
using System.Linq;
using CachingEngine;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace RDMPAutomationService.Logic.Cache
{
    /// <summary>
    /// Identifies CacheProgresses which are not already executing/crashed/locked and are available to run (current time is within the CacheProgress PermissionWindow
    /// if any).  
    /// 
    /// Used by CacheAutomationSource to identify when a new AutomatedCacheRun can be started.
    /// </summary>
    public class CacheRunFinder
    {
        private readonly ICatalogueRepository _catalogueRepository;
        private List<PermissionWindow> _permissionWindowsAlreadyUnderway;
        private List<CacheProgress> _cacheProgressesAlreadyUnderway;

        public CacheRunFinder(ICatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public CacheProgress SuggestCacheProgress()
        {
            RefreshOngoingJobs();

            var caches = _catalogueRepository.GetAllObjects<CacheProgress>().ToArray();
            
            if (!caches.Any())
                return null;

            var automationLockedCatalogues = _catalogueRepository.GetAllAutomationLockedCatalogues();

            //if there is no logging server then we can't do automated cache runs
            var defaults = new ServerDefaults((CatalogueRepository) _catalogueRepository);
            if (defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID) == null)
                return null;
            
            Dictionary<CacheProgress,Catalogue[]> cacheCatalogues 
                = caches.ToDictionary(
                cache => cache, 
                cache => cache.GetAllCataloguesMaximisingOnPermissionWindow());

            List<CacheProgress> toDiscard = new List<CacheProgress>();

            foreach (KeyValuePair<CacheProgress, Catalogue[]> kvp in cacheCatalogues)
            {
                var memory = new ToMemoryCheckNotifier();
                new CachingPreExecutionChecker(kvp.Key).Check(memory);
                
                //if it fails pre load checks
                if(memory.GetWorst() == CheckResult.Fail)
                    toDiscard.Add(kvp.Key);
                else
                if(!kvp.Value.Any())//if there are no catalogues
                    toDiscard.Add(kvp.Key);
                else if (IsAlreadyUnderway(kvp.Key))
                    toDiscard.Add(kvp.Key);
                else if (kvp.Value.Any(automationLockedCatalogues.Contains)) //if there are locked catalogues
                    toDiscard.Add(kvp.Key);
                else if (kvp.Key.PermissionWindow_ID != null && kvp.Key.PermissionWindow.LockedBecauseRunning)
                    toDiscard.Add(kvp.Key);
                else
                {
                    //it passed checking so we know there is loadable data but we might have just finished cashing it 30 seconds ago and theres no point running it for 30s
                    //therefore we will look at the load delay and only trigger a load if there is that much data available to load
                    var shortfall = kvp.Key.GetShortfall();
                    var delay = kvp.Key.GetCacheLagPeriodLoadDelay();
                    
                    if (shortfall < delay)
                        toDiscard.Add(kvp.Key);
                }
              
            }

            foreach (CacheProgress discard in toDiscard)
                cacheCatalogues.Remove(discard);

            if (!cacheCatalogues.Any())
                return null;

            
            return cacheCatalogues.Keys.First();
        }

        private bool IsAlreadyUnderway(CacheProgress cp)
        {
            //if it has a permission window and that permission window is already being executed
            if (cp.PermissionWindow_ID != null && _permissionWindowsAlreadyUnderway.Contains(cp.PermissionWindow))
                return true;//it is already underway

            //if it is a cache progress that is already underway
            return _cacheProgressesAlreadyUnderway.Contains(cp);
        }

        private void RefreshOngoingJobs()
        {
            var ongoingCacheJobs = _catalogueRepository.GetAllObjects<AutomationJob>().Where(j => j.AutomationJobType == AutomationJobType.Cache).ToList();

            _cacheProgressesAlreadyUnderway = new List<CacheProgress>();
            _permissionWindowsAlreadyUnderway = new List<PermissionWindow>();
            
            
            foreach (var job in ongoingCacheJobs.ToArray())
            {
                var permissionWindowIfAny = job.GetCachingJobsPermissionWindowObjectIfAny();
                var cacheProgressIfAny = job.GetCachingJobsProgressObjectIfAny();

                if (permissionWindowIfAny != null)
                    _permissionWindowsAlreadyUnderway.Add(permissionWindowIfAny);
                else
                if(cacheProgressIfAny != null)
                    _cacheProgressesAlreadyUnderway.Add(cacheProgressIfAny);
                else
                    throw new NotSupportedException("There is an AutomationJob with the description '" + job.Description + "' which is apparently not associated with either a CacheProgress or PermissionWindow");
                                                    
            }

        }
    }
}
