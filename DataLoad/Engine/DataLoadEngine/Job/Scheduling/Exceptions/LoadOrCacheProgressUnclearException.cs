using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoadEngine.Job.Scheduling.Exceptions
{
    /// <summary>
    /// Thrown when it is not possible to determine the starting date of a scheduled data load e.g. when there is a LoadProgress record where there is no 
    /// LoadProgress.OriginDate or LoadProgress.DataLoadProgress recorded (so we don't know when the data allegedly started being available/loadable).
    /// </summary>
    public class LoadOrCacheProgressUnclearException : Exception
    {
        public LoadOrCacheProgressUnclearException(string msg) : base(msg)
        {
            
        }
    }
}
