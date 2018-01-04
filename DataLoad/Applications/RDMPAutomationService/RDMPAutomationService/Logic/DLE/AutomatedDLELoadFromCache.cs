using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using DataLoadEngine.LoadProcess.Scheduling;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using HIC.Logging;
using RDMPAutomationService.EventHandlers;
using RDMPAutomationService.Interfaces;

namespace RDMPAutomationService.Logic.DLE
{
    class AutomatedDLELoadFromCache:IAutomateable
    {
        private AutomationServiceSlot _slottedService;
        private LoadProgress _cacheBasedLoadWeCanRun;
        private CacheProgress _cache;

        public AutomatedDLELoadFromCache(AutomationServiceSlot serviceSlot, LoadProgress cacheBasedLoadWeCanRun)
        {
            _slottedService = serviceSlot;
            _cacheBasedLoadWeCanRun = cacheBasedLoadWeCanRun;
            _cache = cacheBasedLoadWeCanRun.CacheProgress;
        }

        public OnGoingAutomationTask GetTask()
        {
            var toReturn = new OnGoingAutomationTask(
                _slottedService.AddNewJob(AutomationJobType.DLE, "Loading Cache Based LoadProgress " + _cacheBasedLoadWeCanRun),
                this);
            
            //lock the permission window too which prevents caching happening (I don't like that idea)
            if (_cache.PermissionWindow_ID != null)
                _cache.PermissionWindow.Lock();
            
            //Lock all the catalogues for automation use
            toReturn.Job.LockCatalogues(_cacheBasedLoadWeCanRun.LoadMetadata.GetAllCatalogues().Cast<Catalogue>().ToArray());

            return toReturn;
        }

        public void RunTask(OnGoingAutomationTask task)
        {
            try
            {
                var dataLoadEventsListener = new AutomatedThrowImmediatelyDataLoadEventsListener(task);

                var lmd = _cacheBasedLoadWeCanRun.LoadMetadata;
                var catalogueRepository = (CatalogueRepository)lmd.Repository;

                var databaseConfiguration = new HICDatabaseConfiguration(lmd);
                var loadFlags = new HICLoadConfigurationFlags();

                var loadStrategy = new SingleLoadProgressSelectionStrategy(_cacheBasedLoadWeCanRun);
                var dayStrategy = new JobDateGenerationStrategyFactory(loadStrategy);
                var logManager = new LogManager(lmd.GetDistinctLoggingDatabaseSettings());
            
                var preExecutionChecker = new PreExecutionChecker(lmd, databaseConfiguration);

                var factory = new HICDataLoadFactory(lmd, databaseConfiguration, loadFlags, catalogueRepository, logManager);
                var loadExecution = factory.Create(dataLoadEventsListener);

                var cancellationTokenSource = new GracefulCancellationTokenSource();

                var dataLoadProcess = new SingleJobScheduledDataLoadProcess(lmd, preExecutionChecker, loadExecution, dayStrategy, loadStrategy,null, logManager, dataLoadEventsListener);

                var exitCode = dataLoadProcess.Run(cancellationTokenSource.Token);

                if (exitCode == ExitCodeType.Success || exitCode == ExitCodeType.OperationNotRequired)
                    task.Job.SetLastKnownStatus(AutomationJobStatus.Finished);
                else
                    task.Job.SetLastKnownStatus(AutomationJobStatus.Crashed);
            }
            catch (Exception e)
            {
                new AutomationServiceException((ICatalogueRepository)task.Repository, e);
                task.Job.SetLastKnownStatus(AutomationJobStatus.Crashed);
            }
            finally
            {
                //if we are at base level of recursion
                task.Job.RevertToDatabaseState();
                if (task.Job.LastKnownStatus == AutomationJobStatus.Finished)//and we are at status Finished delete it to free up space for the next load
                {
                    //unlock the permission window too which prevents caching happening (I don't like that idea)
                    if (_cache.PermissionWindow_ID != null)
                        _cache.PermissionWindow.Unlock();;
            

                    task.Job.DeleteInDatabase();
                }
            }
        }
    }
}
