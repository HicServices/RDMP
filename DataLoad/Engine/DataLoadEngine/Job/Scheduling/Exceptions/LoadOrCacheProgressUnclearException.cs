using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoadEngine.Job.Scheduling.Exceptions
{
    public class LoadOrCacheProgressUnclearException : Exception
    {
        public LoadOrCacheProgressUnclearException(string msg) : base(msg)
        {
            
        }
    }
}
