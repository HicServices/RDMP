using System;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    public class PersistenceException : Exception
    {
        public PersistenceException(string message) : base(message)
        {
            
        }
        public PersistenceException(string message, Exception exception):base(message,exception)
        {
        }
    }
}