using System;
using System.Security.Cryptography.X509Certificates;

namespace ReusableLibraryCode
{
    /// <summary>
    /// object which is accessible from multiple locations (which could be on different computers) and is updated asynchronously from one location on a regular
    /// basis to indicate it is still alive/executing.
    /// </summary>
    public interface ILifelineable
    {
        /// <summary>
        /// The last known time that the object was alive
        /// </summary>
        DateTime? Lifeline { get; set; }

        /// <summary>
        /// Updates the Lifeline to now
        /// </summary>
        void TickLifeline();

        /// <summary>
        /// Synchronise the Lifeline property with the value currently stored in the database (allows an ILifelineable to be shared between different 
        /// users/computers)
        /// </summary>
        void RefreshLifelinePropertyFromDatabase();
    }
}