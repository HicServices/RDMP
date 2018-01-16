using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CachingEngine;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.LoadExecution;
using HIC.Logging;
using HIC.Logging.Listeners;
using RDMPAutomationService.EventHandlers;
using RDMPAutomationService.Interfaces;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.Cache
{
    /// <summary>
    /// Automation task that runs a single CacheProgress until it is up-to-date (all available data read) or crashes.
    /// </summary>
    public class AutomatedCacheRun:IAutomateable
    {
        private readonly AutomationServiceSlot _slot;
        private readonly CacheProgress _cacheProgress;
        private readonly LogManager _logManager;
        private readonly string _dataLoadTask;

        
        public AutomatedCacheRun(AutomationServiceSlot slot, CacheProgress cacheProgress)
        {
            _slot = slot;
            _cacheProgress = cacheProgress;

            var defaults = new ServerDefaults((CatalogueRepository) slot.Repository);
            var loggingServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            if(loggingServer == null)
                throw new NotSupportedException("No default logging server specified, you must specify one in ");
            
            _logManager = new LogManager(loggingServer);
            _dataLoadTask = "caching";
            _logManager.CreateNewLoggingTaskIfNotExists(_dataLoadTask);

        }

        private bool _runWithPermissionWindow = false;
        public OnGoingAutomationTask GetTask()
        {
            OnGoingAutomationTask toReturn;

            //if it doesn't have a permission window then run the cache progress alone
            if(_cacheProgress.PermissionWindow_ID == null)
            {
                _runWithPermissionWindow = false;

                toReturn = new OnGoingAutomationTask(_slot.AddNewJob(_cacheProgress), this);
            }
            else
            {
                //it has a cache window
                _runWithPermissionWindow = true;
                toReturn = new OnGoingAutomationTask(_slot.AddNewJob(_cacheProgress.PermissionWindow), this);
            }

            toReturn.Job.LockCatalogues(_cacheProgress.GetAllCataloguesMaximisingOnPermissionWindow());

            return toReturn;
        }

   
        public void RunTask(OnGoingAutomationTask task)
        {
            //Tell them we have started
            task.Job.SetLastKnownStatus(AutomationJobStatus.Running);
            task.Job.TickLifeline();
            try
            {
                //Setup dual listeners for the Cache process, one ticks the lifeline one very message and one logs to the logging db
                var lifelinePingerEventHandler = new AutomatedThrowImmediatelyDataLoadEventsListener(task);
                var toLoggingDatabaseEventHandler = new ToLoggingDatabaseDataLoadEventListener(this,_logManager,_dataLoadTask,task.Job.Description);
                var forkEventsHandler = new ForkDataLoadEventListener(lifelinePingerEventHandler, toLoggingDatabaseEventHandler);

                var cachingHost = new CachingHost((CatalogueRepository)_slot.Repository);
                cachingHost.CacheProgressList = new ICacheProgress[] { _cacheProgress }.ToList(); //run the cp

                //if it has a permission window
                if (_cacheProgress.PermissionWindow_ID != null)
                {
                    if (!_runWithPermissionWindow)
                        throw new NotSupportedException("When task was created it was decided that it should NOT run as a PermissionWindow (because it didn't have one) but now the Task does have a PermissionWindow! has somoene edited the database in the mean time? seems unlikley");
                    cachingHost.PermissionWindows = new IPermissionWindow[] {_cacheProgress.PermissionWindow}.ToList();
                }
                
                //By default caching host will block 
                cachingHost.TerminateIfOutsidePermissionWindow = true;
                
                cachingHost.Start(forkEventsHandler, new GracefulCancellationToken(task.CancellationTokenSource.Token, new CancellationToken()));
                task.Job.SetLastKnownStatus(AutomationJobStatus.Finished);
                task.Job.DeleteInDatabase();

                //finish everything
                toLoggingDatabaseEventHandler.FinalizeTableLoadInfos();
            }
            catch (Exception e)
            {
                task.Job.SetLastKnownStatus(AutomationJobStatus.Crashed);
                new AutomationServiceException((ICatalogueRepository) task.Repository, e);
            }
        }
    }
}
