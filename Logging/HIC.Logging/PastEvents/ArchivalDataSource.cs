using System;

namespace HIC.Logging.PastEvents
{
    /// <summary>
    /// Readonly audit of a historical 'data source' (See HIC.Logging.DataSource) that contributed records to a table that was loaded in the last (See 
    /// ArchivalTableLoadInfo).
    /// </summary>
    public class ArchivalDataSource : IArchivalLoggingRecordOfPastEvent, IComparable
    {

        public string MD5 { get; internal set; }
        public string Source { get; internal set; }
        public string Archive { get; internal set; }
        public DateTime? OriginDate { get; internal set; }
        public int ID { get; set; }

        public ArchivalDataSource(int id, object originDate, string source, string archive, string mD5)
        {
            ID = id;
            if (originDate == null || originDate == DBNull.Value)
                OriginDate = null;
            else
                OriginDate = (DateTime) originDate;
            Source = source;
            Archive = archive;
            MD5 = mD5;
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
            return  "Source:" + Source + (string.IsNullOrWhiteSpace(MD5) ? "" : "(MD5=" + MD5 + ")");
        }

        public int CompareTo(object obj)
        {
            var other = obj as ArchivalDataSource;
            if (other != null)
                if (OriginDate == other.OriginDate)
                    return 0;
                else
                {

                    if (!OriginDate.HasValue)
                        return -1;

                    if (!other.OriginDate.HasValue)
                        return 1;
                    
                    return OriginDate > other.OriginDate ? 1 : -1;

                }

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }
    }
}