using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadProcess.Scheduling
{
    /// <summary>
    /// Loads data according to a data-based schedule, e.g. Biochemistry.
    /// Needs to know: how to generate dates for the job, how to select a load schedule
    /// </summary>
    public abstract class ScheduledDataLoadProcess : DataLoadProcess
    {
        protected readonly JobDateGenerationStrategyFactory JobDateGenerationStrategyFactory;
        protected readonly ILoadProgressSelectionStrategy LoadProgressSelectionStrategy;
        protected readonly int? OverrideNumberOfDaysToLoad;

        protected ScheduledDataLoadProcess(IRDMPPlatformRepositoryServiceLocator repositoryLocator,ILoadMetadata loadMetadata, ICheckable preExecutionChecker, IDataLoadExecution loadExecution, JobDateGenerationStrategyFactory jobDateGenerationStrategyFactory, ILoadProgressSelectionStrategy loadProgressSelectionStrategy, int? overrideNumberOfDaysToLoad, ILogManager logManager, IDataLoadEventListener dataLoadEventListener)
            : base(repositoryLocator,loadMetadata, preExecutionChecker, logManager, dataLoadEventListener, loadExecution)
        {
            JobDateGenerationStrategyFactory = jobDateGenerationStrategyFactory;
            LoadProgressSelectionStrategy = loadProgressSelectionStrategy;
            OverrideNumberOfDaysToLoad = overrideNumberOfDaysToLoad;
        }
    }
}