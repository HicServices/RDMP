using System;

namespace HIC.Logging.PastEvents
{
    public class ArchivalProgressLog : IArchivalLoggingRecordOfPastEvent, IComparable
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }

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