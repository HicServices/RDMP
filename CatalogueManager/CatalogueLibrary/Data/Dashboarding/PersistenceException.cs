using System;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// Occurs when there is an error restoring a specific Persistence String.  Often occurs when a UI class has been renamed or a plugin unloaded between 
    /// RDMP application executions.
    /// </summary>
    public class PersistenceException : Exception
    {
        /// <summary>
        /// Throw when you were unable to resolve a saved Control state
        /// </summary>
        /// <param name="message"></param>
        public PersistenceException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Throw when you were unable to resolve a saved Control state and have an <paramref name="exception"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public PersistenceException(string message, Exception exception):base(message,exception)
        {
        }
    }
}