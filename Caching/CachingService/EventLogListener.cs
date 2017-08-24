using System.Diagnostics;
using ReusableLibraryCode.Progress;

namespace CachingService
{
    public class EventLogListener : IDataLoadEventListener
    {
        private readonly EventLog _eventLog;

        public EventLogListener(EventLog eventLog)
        {
            _eventLog = eventLog;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            // map entry types
            EventLogEntryType entryType;
            switch (e.ProgressEventType)
            {
                case ProgressEventType.Information:
                    entryType = EventLogEntryType.Information;
                    break;
                case ProgressEventType.Warning:
                    entryType = EventLogEntryType.Warning;
                    break;
                case ProgressEventType.Error:
                    entryType = EventLogEntryType.Error;
                    break;
                default:
                    entryType = EventLogEntryType.Information;
                    break;
            }

            _eventLog.WriteEntry(e.Message, entryType);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            // just swallow progress events for now, we don't want to flood the log with entries
        }
    }
}