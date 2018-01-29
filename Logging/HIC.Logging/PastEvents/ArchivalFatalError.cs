using System;
using System.Security.Policy;

namespace HIC.Logging.PastEvents
{
    /// <summary>
    /// Readonly audit of a historical error which resulted in the failure of the logged activity (which is also a past / readonly event).
    /// </summary>
    public class ArchivalFatalError : IArchivalLoggingRecordOfPastEvent
    {
        public int ID { get; private set; }
        public DateTime Date { get; internal set; }
        public string Source { get; internal set; }
        public string Description { get; internal set; }
        public string Explanation { get; set; }

        public ArchivalFatalError(int id, DateTime date,string source, string description, string explanation)
        {
            ID = id;
            Date = date;
            Source = source;
            Description = description;
            Explanation = explanation;
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
            return Source + " - " + Description + (string.IsNullOrWhiteSpace(Explanation)?"(UNRESOLVED)":"(RESOLVED:"+Explanation+")");
        }
        public int CompareTo(object obj)
        {
            var other = obj as ArchivalFatalError;
            if (other != null)
                if (Date == other.Date)
                    return 0;
                else
                    return Date > other.Date ? 1 : -1;

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }

    }
}