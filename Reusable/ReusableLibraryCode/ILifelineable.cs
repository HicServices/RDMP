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
        /// The last time the object reported in as alive.  This lets you monitor remote activities to see if they still seem to be responding or if they
        /// have been hung/dead for days.
        /// </summary>
        DateTime? Lifeline { get; set; }

        /// <summary>
        /// Updates the Lifeline property to DateTime.Now in the database.  This should be regularly called for any job that knows it is alive and making progress
        /// </summary>
        void TickLifeline();

        /// <summary>
        /// Synchronise the Lifeline property with the value currently stored in the database (allows an ILifelineable to be shared between different 
        /// users/computers)
        /// </summary>
        void RefreshLifelinePropertyFromDatabase();
    }
}