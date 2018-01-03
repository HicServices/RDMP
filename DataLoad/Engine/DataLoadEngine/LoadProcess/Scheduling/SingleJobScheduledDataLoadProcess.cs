using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadProcess.Scheduling
{
    public class SingleJobScheduledDataLoadProcess : ScheduledDataLoadProcess
    {
        private SingleScheduledJobFactory _scheduledJobFactory;

        // todo: refactor to cut down on ctor params
        public SingleJobScheduledDataLoadProcess(ILoadMetadata loadMetadata, ICheckable preExecutionChecker, IDataLoadExecution loadExecution, JobDateGenerationStrategyFactory jobDateGenerationStrategyFactory, ILoadProgressSelectionStrategy loadProgressSelectionStrategy, int? overrideNumberOfDaysToLoad, ILogManager logManager, IDataLoadEventListener dataLoadEventListener) :
            base(loadMetadata, preExecutionChecker, loadExecution, jobDateGenerationStrategyFactory, loadProgressSelectionStrategy, overrideNumberOfDaysToLoad, logManager, dataLoadEventListener)
        {
        }

        public override ExitCodeType Run(GracefulCancellationToken loadCancellationToken)
        {
            // single job, so grab the first available LoadProgress and lock it
            var loadProgresses = LoadProgressSelectionStrategy.GetAllLoadProgresses();
            if (!loadProgresses.Any())
                return ExitCodeType.OperationNotRequired;

            var loadProgress = loadProgresses.First();

            // we don't need any other schedules the strategy may have given us, so unlock them
            loadProgresses.Remove(loadProgress);
            loadProgresses.ForEach(progress => progress.Unlock());

            // Create the job factory
            if (_scheduledJobFactory != null)
                throw new Exception("Job factory should only be created once");

            _scheduledJobFactory = new SingleScheduledJobFactory(loadProgress, JobDateGenerationStrategyFactory.Create(loadProgress,DataLoadEventListener), OverrideNumberOfDaysToLoad??loadProgress.DefaultNumberOfDaysToLoadEachTime, LoadMetadata, LogManager);
            
            try
            {
                // If the job factory won't produce any jobs we can bail out here
                if (!_scheduledJobFactory.HasJobs())
                    return ExitCodeType.OperationNotRequired;

                // Run the data load
                JobProvider = _scheduledJobFactory;

             return base.Run(loadCancellationToken);
            }
            finally
            {
                // Remember to unlock load schedule after completion
                loadProgress.Unlock();
            }
        }
    }
}