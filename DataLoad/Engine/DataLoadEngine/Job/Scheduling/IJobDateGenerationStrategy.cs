using System;
using System.Collections.Generic;

namespace DataLoadEngine.Job.Scheduling
{
    public interface IJobDateGenerationStrategy
    {
        List<DateTime> GetDates(int batchSize, bool allowLoadingFutureDates);
        int GetTotalNumberOfJobs(int batchSize, bool allowLoadingFutureDates);
    }
}