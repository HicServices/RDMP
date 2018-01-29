using System;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// Thrown when there is a problem updating the date of a LoadProgress during a ScheduledDataLoadJob (or identifying a suitable date to use for the update).
    /// </summary>
    public class DataLoadProgressUpdateException : Exception
    {
        public DataLoadProgressUpdateException(string msg):base(msg)
        {
            
        }
        public DataLoadProgressUpdateException(string msg,Exception ex): base(msg,ex)
        {

        }
    }
}