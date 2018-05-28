using System;
using System.Collections.Generic;
using System.Linq;
using CachingEngine;
using CatalogueLibrary.Data;

using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.Cache
{
    /// <summary>
    /// Identifies CacheProgresses which are not already executing/crashed/locked and are available to run (current time is within the CacheProgress PermissionWindow
    /// if any).  
    /// 
    /// <para>Used by CacheAutomationSource to identify when a new AutomatedCacheRun can be started.</para>
    /// </summary>
    public class CacheRunFinder
    {
        private readonly ICatalogueRepository _catalogueRepository;
        private List<PermissionWindow> _permissionWindowsAlreadyUnderway;
        private List<CacheProgress> _cacheProgressesAlreadyUnderway;
        private IDataLoadEventListener _listener;

        public CacheRunFinder(ICatalogueRepository catalogueRepository, IDataLoadEventListener listener)
        {
            _catalogueRepository = catalogueRepository;
            _listener = listener;
        }

        public CacheProgress SuggestCacheProgress()
        {
            var caches = _catalogueRepository.GetAllObjects<CacheProgress>().ToArray();
            
            if (!caches.Any())
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No cache progress defined... exiting."));
                return null;
            }

            //if there is no logging server then we can't do automated cache runs
            var defaults = new ServerDefaults((CatalogueRepository) _catalogueRepository);
            if (defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID) == null)
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No Logging server has been defined, we cannot automate cache runs."));
                return null;
            }
            
            Dictionary<CacheProgress,Catalogue[]> cacheCatalogues 
                = caches.ToDictionary(
                cache => cache, 
                cache => cache.GetAllCataloguesMaximisingOnPermissionWindow());

            List<CacheProgress> toDiscard = new List<CacheProgress>();

            foreach (KeyValuePair<CacheProgress, Catalogue[]> kvp in cacheCatalogues)
            {
                var checker = new ToMemoryCheckNotifier();
                new CachingPreExecutionChecker(kvp.Key).Check(checker);

                foreach (var check in checker.Messages)
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Debug, check.Message, check.Ex));
                }

                //if it fails pre load checks
                if(checker.GetWorst() == CheckResult.Fail)
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        String.Format("Cache Progress {0} has failed pre-execution checks... skipping.", kvp.Key)));
                    toDiscard.Add(kvp.Key);
                }
                else if(!kvp.Value.Any())//if there are no catalogues
                {
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        String.Format("Cache Progress {0} has no associated catalogues... skipping.", kvp.Key)));
                    toDiscard.Add(kvp.Key);
                }
                else
                {
                    //it passed checking so we know there is loadable data but we might have just finished cashing it 30 seconds ago and theres no point running it for 30s
                    //therefore we will look at the load delay and only trigger a load if there is that much data available to load
                    var shortfall = kvp.Key.GetShortfall();
                    var delay = kvp.Key.GetCacheLagPeriodLoadDelay();
                    
                    if (shortfall < delay)
                    {
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                           String.Format("Cache Progress {0} has been cached too recently ({1} ago)... skipping.", kvp.Key, shortfall)));
                        toDiscard.Add(kvp.Key);
                    }
                }
              
            }

            foreach (CacheProgress discard in toDiscard)
                cacheCatalogues.Remove(discard);

            if (!cacheCatalogues.Any())
                return null;

            
            return cacheCatalogues.Keys.First();
        }
    }
}
