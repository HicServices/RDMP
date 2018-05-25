using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CachingEngine;
using CachingEngine.Factories;
using CatalogueLibrary.Data;

using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.LoadExecution;
using HIC.Logging;
using HIC.Logging.Listeners;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.Cache
{
    /// <summary>
    /// Automation task that runs a single CacheProgress until it is up-to-date (all available data read) or crashes.
    /// </summary>
    public class AutomatedCacheRun
    {
        private readonly CacheProgress _cacheProgress;
        private readonly LogManager _logManager;
        private readonly string _dataLoadTask;
        private readonly CatalogueRepository _repository;

        public AutomatedCacheRun(CacheProgress cacheProgress)
        {
            _cacheProgress = cacheProgress;

            var defaults = new ServerDefaults((CatalogueRepository)_cacheProgress.Repository);
            var loggingServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            if(loggingServer == null)
                throw new NotSupportedException("No default logging server specified, you must specify one in ");
            
            _logManager = new LogManager(loggingServer);
            _dataLoadTask = "caching";
            _logManager.CreateNewLoggingTaskIfNotExists(_dataLoadTask);
            _repository = (CatalogueRepository) _cacheProgress.Repository;
        }
        
        public void RunTask()
        {
            //Setup dual listeners for the Cache process, one ticks the lifeline one very message and one logs to the logging db
                
            var toLoggingDatabaseEventHandler = new ToLoggingDatabaseDataLoadEventListener(this,_logManager,_dataLoadTask,"Caching " + _cacheProgress);

            var cachingHost = new CachingHost(_repository);
            cachingHost.CacheProgressList = new ICacheProgress[] { _cacheProgress }.ToList(); //run the cp

            //if it has a permission window
            if (_cacheProgress.PermissionWindow_ID != null)
                cachingHost.PermissionWindows = new[] {_cacheProgress.PermissionWindow}.ToList();

            //By default caching host will block 
            cachingHost.TerminateIfOutsidePermissionWindow = true;

            cachingHost.Start(toLoggingDatabaseEventHandler, new GracefulCancellationToken(new CancellationToken(), new CancellationToken()));
                
            //finish everything
            toLoggingDatabaseEventHandler.FinalizeTableLoadInfos();
        }
    }
}
