using System;

namespace MapsDirectlyToDatabaseTable.RepositoryResultCaching
{
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