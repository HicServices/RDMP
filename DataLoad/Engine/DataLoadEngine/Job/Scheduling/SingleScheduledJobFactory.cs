using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DataProvider;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
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

        public override IDataLoadJob Create(IDataLoadEventListener listener)
        {
            var hicProjectDirectory = new HICProjectDirectory(LoadMetadata.LocationOfFlatFiles, false);
            return new ScheduledDataLoadJob(JobDescription, LogManager, LoadMetadata, hicProjectDirectory, listener)
            {
                LoadProgress = _loadProgress,
                DatesToRetrieve = _jobDateGenerationStrategy.GetDates(OverrideNumberOfDaysToLoad??_loadProgress.DefaultNumberOfDaysToLoadEachTime, false)
            };
        }
    }
}