using System;
using System.Collections.Generic;

namespace HIC.Logging.PastEvents
{
    public class ArchivalTableLoadInfo : IArchivalLoggingRecordOfPastEvent, IComparable
    {
        public ArchivalDataLoadInfo Parent { get; private set; }

        public ArchivalTableLoadInfo(ArchivalDataLoadInfo parent, int id, DateTime start, object end, string targetTable, object inserts, object updates, object deletes, string notes)
        {
            Parent = parent;
            ID = id;
            Start = start;

            if (end == null || end == DBNull.Value)
                End = null;
            else
                End = (DateTime) end;

            TargetTable = targetTable;


            Inserts = ToNullableInt(inserts);
            Updates = ToNullableInt(updates);
            Deletes = ToNullableInt(deletes);
            Notes = notes;

            DataSources = new List<ArchivalDataSource>();
        }

        private int? ToNullableInt(object i)
        {
            if (i == null || i == DBNull.Value)
                return null;

            return Convert.ToInt32(i);

        }

        public int ID { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string TargetTable { get; set; }
        public int? Inserts { get; set; }
        public int? Deletes { get; set; }
        public int? Updates { get; set; }
        public string Notes { get; set; }

        public List<ArchivalDataSource> DataSources { get; set; }
        
        public string ToShortString()
        {

            var s = ToString();
            if (s.Length > ArchivalDataLoadInfo.MaxDescriptionLength)
                return s.Substring(0, ArchivalDataLoadInfo.MaxDescriptionLength) + "...";
            return s;
        }

        public override string ToString()
        {
            return Start + " - " + TargetTable + " (Inserts=" + Inserts + ",Updates=" + Updates + ",Deletes=" + Deletes +")";
        }

        public int CompareTo(object obj)
        {
            var other = obj as ArchivalTableLoadInfo;
            if (other != null)
                if (Start == other.Start)
                    return 0;
                else
                    return Start > other.Start ? 1 : -1;

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }
    }
}