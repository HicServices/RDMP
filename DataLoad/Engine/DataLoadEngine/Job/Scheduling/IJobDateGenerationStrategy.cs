using System;
using System.Collections.Generic;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// Determines how DateTimes for a Scheduled load are determined.  Scheduled loads are those where the LoadMetadata has one or more LoadProgresses.  We
    /// could simply add the next 'batchSize' days to the head date of the LoadProgress.  Alternatively we could inspect the cache to make sure that there
    /// are files for those dates and skip any holes. 
    /// </summary>
    public interface IJobDateGenerationStrategy
    {
        List<DateTime> GetDates(int batchSize, bool allowLoadingFutureDates);
        int GetTotalNumberOfJobs(int batchSize, bool allowLoadingFutureDates);
    }
}