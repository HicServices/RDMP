using System;

namespace MapsDirectlyToDatabaseTable.RepositoryResultCaching
{
    /// <summary>
    /// Thrown when a given action is forbidden because SuperCaching is turned on on the current Thread (See SuperCache)
    /// </summary>
    public class SuperCachingModeIsOnException : Exception
    {
        public SuperCachingModeIsOnException()
        {
            
        }

        public SuperCachingModeIsOnException(string msg):base(msg)
        {
            
        }
    }
}