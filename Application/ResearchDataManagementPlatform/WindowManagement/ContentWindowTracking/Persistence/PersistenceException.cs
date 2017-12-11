using System;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// Occurs when there is an error restoring a specific Persistence String.  Often occurs when a UI class has been renamed or a plugin unloaded between 
    /// RDMP application executions.
    /// </summary>
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