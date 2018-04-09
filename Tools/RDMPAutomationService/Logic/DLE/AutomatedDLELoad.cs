using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.DataLoad;
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
    /// <summary>
    /// Automation task that runs a single data load (LoadMetadata) that is due according to it's routine execution schedule (LoadPeriodically).
    /// </summary>
    public class AutomatedDLELoad : IAutomateable
    {
        private readonly AutomationServiceSlot _slottedService;
        private readonly LoadPeriodically _loadPeriodically;
        private readonly LoadMetadata _rootLoadMetadata;
        private OnGoingAutomationTask _task;

        public AutomatedDLELoad(AutomationServiceSlot slottedService, LoadPeriodically loadPeriodically)
        {
            _slottedService = slottedService;
            _loadPeriodically = loadPeriodically;
            _rootLoadMetadata = _loadPeriodically.LoadMetadata;
        }

        public OnGoingAutomationTask GetTask()
        {
            var toReturn = new OnGoingAutomationTask(_slottedService.AddNewJob(AutomationJobType.DLE,"Loading " + _rootLoadMetadata.Name), this);
            
            toReturn.Job.LockCatalogues((Catalogue[]) _rootLoadMetadata.GetAllCatalogues());

            return toReturn;
        }

        public void RunTask(OnGoingAutomationTask task)
        {
            _task = task;
            LaunchLoad(_rootLoadMetadata,_loadPeriodically);
        }

        private void LaunchLoad(LoadMetadata currentMetadata, LoadPeriodically dueLoad)
        {
            try
            {
                PreExecutionChecker p = new PreExecutionChecker(currentMetadata, null);
                p.Check(new AutomatedAcceptAllCheckNotifier(_task));

                IExternalDatabaseServer serverChosen;
                var loggingServer = currentMetadata.GetDistinctLoggingDatabaseSettings(out serverChosen, false);
                var logManager = new LogManager(loggingServer);

                //Create a callback with the log manager that will set the known logging data load run id and server ID (if logging does actually happen - sometimes a load run will decide nothing requires done and therefore will not log)
                logManager.DataLoadInfoCreated += (s, d) => _task.Job.SetLoggingInfo(serverChosen, d.ID);

                var databaseConfiguration = new HICDatabaseConfiguration(currentMetadata);

                // Create the pipeline to pass into the DataLoadProcess object
                var dataLoadFactory = new HICDataLoadFactory(currentMetadata, databaseConfiguration,
                    new HICLoadConfigurationFlags(), (CatalogueRepository) _task.Repository, logManager);
                var listener = new AutomatedThrowImmediatelyDataLoadEventsListener(_task);

                IDataLoadExecution execution = dataLoadFactory.Create(listener);

                IDataLoadProcess dataLoadProcess;
                //if there are any LoadProgresses associated with the metadata
                if (currentMetadata.LoadProgresses.Any())
                {
                    //Then the load is designed to run X days of source data at a time
                    //Load Progress
                    var whichLoadProgress = new AnyAvailableLoadProgressSelectionStrategy(currentMetadata);
                    var jobDateFactory = new JobDateGenerationStrategyFactory(whichLoadProgress);
                    dataLoadProcess = new SingleJobScheduledDataLoadProcess(currentMetadata, p, execution, jobDateFactory,
                        whichLoadProgress, null, logManager, listener);
                }
                else
                    //OnDemand
                    dataLoadProcess = new DataLoadProcess(currentMetadata, p, logManager, listener, execution);

                var exitCode =
                    dataLoadProcess.Run(new GracefulCancellationToken(_task.CancellationTokenSource.Token,
                        _task.CancellationTokenSource.Token));

                _task.Job.SetLastKnownStatus(GetStatusForExitCode(exitCode));

                //is never null the first time, is only null if there is an OnSuccessLaunchLoadMetadata_ID followon LoadMetadata which itself has no children LoadPeriodicallies
                if (dueLoad != null)
                {
                    //load completed so save
                    dueLoad.LastLoaded = DateTime.Now;
                    dueLoad.SaveToDatabase();
                }

                //if the exit code was success
                if (exitCode == ExitCodeType.Success)
                {
                    //is there a chained load 
                    if (dueLoad != null && dueLoad.OnSuccessLaunchLoadMetadata_ID != null)
                    {
                        LoadMetadata chainLoad = dueLoad.OnSuccessLaunchLoadMetadata; //this is the chain load
                        LoadPeriodically chainLoadPeriodically = chainLoad.LoadPeriodically;

                        //but the chain load might have a chain of it's own!
                        LaunchLoad(chainLoad, chainLoadPeriodically);
                            //this is recursive call which will eventually end in a LoadMetadata that doesnt have another chain which will leave you in the else below
                    }
                    else //there is no chained load so we are definetly done!
                    {
                        //we are done now
                        _task.Job.SetLastKnownStatus(AutomationJobStatus.Finished);
                    }
                }
                else if (exitCode == ExitCodeType.OperationNotRequired)
                    _task.Job.SetLastKnownStatus(AutomationJobStatus.Finished);
                        //exit code was not required so we are finished but also don't launch any chained loads
                else
                    throw new Exception("Chained load " + currentMetadata.Name + " ended with exit code " + exitCode);
            }
            catch (Exception e)
            {
                new AutomationServiceException((ICatalogueRepository) _task.Repository, e);
                _task.Job.SetLastKnownStatus(AutomationJobStatus.Crashed);
            }
            finally
            {
                //if we are at base level of recursion
                if (_rootLoadMetadata.Equals(currentMetadata))
                {
                    _task.Job.RevertToDatabaseState();
                    if (_task.Job.LastKnownStatus == AutomationJobStatus.Finished)//and we are at status Finished delete it to free up space for the next load
                        _task.Job.DeleteInDatabase();
                }
            }

        }

        private AutomationJobStatus GetStatusForExitCode(ExitCodeType exitCode)
        {
            switch (exitCode)
            {
                case ExitCodeType.Success:
                    return AutomationJobStatus.Running;//Notice that we pass back Running because there might be a chained load, we are not finished until we have checked for chained load
                case ExitCodeType.Error:
                    return AutomationJobStatus.Crashed;
                case ExitCodeType.Abort:
                    return AutomationJobStatus.Crashed;
                case ExitCodeType.OperationNotRequired:
                    return AutomationJobStatus.Running;//Notice that we pass back Running because there might be a chained load, we are not finished until we have checked for chained load
                default:
                    throw new ArgumentOutOfRangeException("exitCode");
            }
        }
    }
}
