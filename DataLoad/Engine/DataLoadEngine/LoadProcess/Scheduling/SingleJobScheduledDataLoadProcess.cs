using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadProcess.Scheduling
{
    /// <summary>
    /// DataLoadProcess for LoadMetadata's which have one or more LoadProgresses (See ScheduledDataLoadProcess).  This version of ScheduledDataLoadProcess will
    /// run a single execution of a LoadProgress.  For example if you have a load 'Load biochemistry records' with a LoadProgress which 'loads 5 days at a time'
    /// and is currently at LoadProgress.DataLoadProgress of 2001-01-01 it will run a single load (See ScheduledDataLoadJob) for the next 5 days and then stop.
    /// </summary>
    public class SingleJobScheduledDataLoadProcess : ScheduledDataLoadProcess
    {
        private SingleScheduledJobFactory _scheduledJobFactory;

        // todo: refactor to cut down on ctor params
        public SingleJobScheduledDataLoadProcess(IRDMPPlatformRepositoryServiceLocator repositoryLocator,ILoadMetadata loadMetadata, ICheckable preExecutionChecker, IDataLoadExecution loadExecution, JobDateGenerationStrategyFactory jobDateGenerationStrategyFactory, ILoadProgressSelectionStrategy loadProgressSelectionStrategy, int? overrideNumberOfDaysToLoad, ILogManager logManager, IDataLoadEventListener dataLoadEventListener) :
            base(repositoryLocator,loadMetadata, preExecutionChecker, loadExecution, jobDateGenerationStrategyFactory, loadProgressSelectionStrategy, overrideNumberOfDaysToLoad, logManager, dataLoadEventListener)
        {
        }

        public override ExitCodeType Run(GracefulCancellationToken loadCancellationToken, object payload = null)
        {
            // single job, so grab the first available LoadProgress and lock it
            var loadProgresses = LoadProgressSelectionStrategy.GetAllLoadProgresses();
            if (!loadProgresses.Any())
                return ExitCodeType.OperationNotRequired;

            var loadProgress = loadProgresses.First();

            // we don't need any other schedules the strategy may have given us
            loadProgresses.Remove(loadProgress);

            // Create the job factory
            if (_scheduledJobFactory != null)
                throw new Exception("Job factory should only be created once");

            _scheduledJobFactory = new SingleScheduledJobFactory(loadProgress, JobDateGenerationStrategyFactory.Create(loadProgress,DataLoadEventListener), OverrideNumberOfDaysToLoad??loadProgress.DefaultNumberOfDaysToLoadEachTime, LoadMetadata, LogManager);
            
            // If the job factory won't produce any jobs we can bail out here
            if (!_scheduledJobFactory.HasJobs())
                return ExitCodeType.OperationNotRequired;

            // Run the data load
            JobProvider = _scheduledJobFactory;

            return base.Run(loadCancellationToken,payload);
        }
    }
}