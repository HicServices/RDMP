using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DataProvider;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// Return a ScheduledDataLoadJob hydrated with appropriate dates for the LoadProgress supplied.  This class differs from SingleScheduledJobFactory only
    /// in that it lets you pass multiple ILoadProgress instead of only one, the class will decide which is the next most sensible one to run.  For example
    /// you might have 'Load Biochem For Tayside' and 'Load Biochem For Fife' as two LoadProgress in the LoadMetadata 'Loading Biochemistry', this class would 
    /// pick which one to execute next. 
    /// </summary>
    public class MultipleScheduleJobFactory : ScheduledJobFactory
    {
        private readonly Dictionary<ILoadProgress, IJobDateGenerationStrategy> _availableSchedules;
        private readonly List<ILoadProgress> _scheduleList;
        private int _lastScheduleId;
        private int _lastJobId;

        public MultipleScheduleJobFactory(Dictionary<ILoadProgress, IJobDateGenerationStrategy> availableSchedules, int? overrideNumberOfDaysToLoad , ILoadMetadata loadMetadata, ILogManager logManager)
            : base(overrideNumberOfDaysToLoad, loadMetadata, logManager)
        {
            _availableSchedules = availableSchedules;
            
            _scheduleList = _availableSchedules.Keys.ToList();

            _lastScheduleId = 0;
            _lastJobId = 0;
        }

        /// <summary>
        /// Returns false only if no schedule has any jobs associated with it
        /// </summary>
        /// <returns></returns>
        public override bool HasJobs()
        {
            return _scheduleList.Any(loadProgress => _availableSchedules[loadProgress].GetTotalNumberOfJobs(OverrideNumberOfDaysToLoad??loadProgress.DefaultNumberOfDaysToLoadEachTime, false) > 0);
        }

        public override IDataLoadJob Create(IDataLoadEventListener listener)
        {
            ScheduledDataLoadJob job;
            var loadProgress = _scheduleList[_lastScheduleId];
            var datesToRetrieve = _availableSchedules[loadProgress].GetDates(OverrideNumberOfDaysToLoad??_scheduleList[_lastScheduleId].DefaultNumberOfDaysToLoadEachTime, false);
            if (!datesToRetrieve.Any())
                return null;

            var hicProjectDirectory = new HICProjectDirectory(LoadMetadata.LocationOfFlatFiles, false);
            job = new ScheduledDataLoadJob(JobDescription, LogManager, LoadMetadata, hicProjectDirectory, listener);
            
            job.LoadProgress = loadProgress;
            job.DatesToRetrieve = datesToRetrieve;

            // move our circular pointer for the round-robin assignment
            _lastScheduleId = (_lastScheduleId + 1) % _scheduleList.Count;

            return job;
        }
    }
}