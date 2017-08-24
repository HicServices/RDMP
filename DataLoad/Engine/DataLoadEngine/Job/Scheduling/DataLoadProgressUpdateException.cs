using System;

namespace DataLoadEngine.Job.Scheduling
{
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