using System;

namespace HIC.Logging.PastEvents
{
    /// <summary>
    /// Readonly audit of a historical logged event which was noteworthy during the logged activity (See ArchivalDataLoadInfo)
    /// </summary>
    public class ArchivalProgressLog : IArchivalLoggingRecordOfPastEvent, IComparable
    {
        public int ID { get; internal set; }
        public DateTime Date { get; internal set; }
        public string EventType { get; internal set; }
        public string Description { get; internal set; }

        public ArchivalProgressLog(int id, DateTime date,string eventType,string description)
        {
            ID = id;
            Date = date;
            EventType = eventType;
            Description = description;
        }
        public string ToShortString()
        {
            var s = ToString();
            if (s.Length > ArchivalDataLoadInfo.MaxDescriptionLength)
                return s.Substring(0, ArchivalDataLoadInfo.MaxDescriptionLength) + "...";
            return s;
        }
        public override string ToString()
        {
            return Date + " - " + Description;
        }

        public int CompareTo(object obj)
        {
            var other = obj as ArchivalProgressLog;
            if (other != null)
                if (Date == other.Date)
                    return 0;
                else
                    return Date > other.Date ? 1 : -1;

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }
    }
}