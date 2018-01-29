using System;

namespace HIC.Logging.PastEvents
{
    /// <summary>
    /// Interface for all Logging messages/objects fetched out of the logging database (immutable PastEvents).  The RDMP logging database supports both logging 
    /// of new messages/objects e.g. TableLoadInfo and fetching archival history objects of old runs logged in the past ArchivalTableLoadInfo.  All archival 
    /// history objects are immutable (cannot be edited) and inherit from IArchivalLoggingRecordOfPastEvent
    /// </summary>
    public interface IArchivalLoggingRecordOfPastEvent: IComparable
    {
        int ID { get; }
    }
}