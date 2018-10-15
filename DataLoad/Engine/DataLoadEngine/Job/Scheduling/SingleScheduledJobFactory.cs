using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DataProvider;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// Return a ScheduledDataLoadJob hydrated with appropriate dates for the LoadProgress supplied (e.g. load the next 5 days of Load Progress 'Tayside Biochem
    /// Loading').
    /// </summary>
    public class SingleScheduledJobFactory : ScheduledJobFactory
    {
        private readonly ILoadProgress _loadProgress;
        private readonly IJobDateGenerationStrategy _jobDateGenerationStrategy;

        public SingleScheduledJobFactory(ILoadProgress loadProgress, IJobDateGenerationStrategy jobDateGenerationStrategy, int overrideNumberOfDaysToLoad , ILoadMetadata loadMetadata, ILogManager logManager) : base(overrideNumberOfDaysToLoad, loadMetadata, logManager)
        {
            _loadProgress = loadProgress;
            _jobDateGenerationStrategy = jobDateGenerationStrategy;
        }

        public override bool HasJobs()
        {
            return _jobDateGenerationStrategy.GetTotalNumberOfJobs(OverrideNumberOfDaysToLoad??_loadProgress.DefaultNumberOfDaysToLoadEachTime, false) > 0;
        }

        public override IDataLoadJob Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator,IDataLoadEventListener listener,HICDatabaseConfiguration configuration)
        {
            var hicProjectDirectory = new HICProjectDirectory(LoadMetadata.LocationOfFlatFiles, false);
            return new ScheduledDataLoadJob(repositoryLocator,JobDescription, LogManager, LoadMetadata, hicProjectDirectory, listener,configuration)
            {
                LoadProgress = _loadProgress,
                DatesToRetrieve = _jobDateGenerationStrategy.GetDates(OverrideNumberOfDaysToLoad??_loadProgress.DefaultNumberOfDaysToLoadEachTime, false)
            };
        }
    }
}