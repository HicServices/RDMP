

namespace Rdmp.Core.Logging;
using System;

// add surce
public class LogEntry
{
    public LogEntry(string eventType, string description, string source, DateTime time)
    {
        EventType = eventType;
        Description = description;
        Time = time;
        Source = source;
    }

    public string EventType { get; private set; }
    public string Description { get; private set; }
    public DateTime Time { get; private set; }
    public string Source { get; private set; }
}