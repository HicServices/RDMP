using System;

namespace DataLoadEngine.Job.Scheduling.Exceptions
{
    /// <summary>
    /// Thrown when we are attempting to determine the loadable date range of a Scheduled data load (See LoadProgress) by looking at the Cache.  For example if the
    /// load doesn't have any ICachedDataProvider components.
    /// </summary>
    public class CacheDataProviderFindingException : Exception
    {
        public CacheDataProviderFindingException(string msg):base(msg)
        {
            
        }
    }
}