using System;
using System.Diagnostics;

namespace RDMPAutomationService
{
    /// <summary>
    /// Event Args for communication events between AutoRDMP and RDMPAutomationLoop.
    /// </summary>
    public class ServiceEventArgs
    {
        public string Message { get; set; }
        public EventLogEntryType EntryType { get; set; }
        public Exception Exception { get; set; }
    }
}