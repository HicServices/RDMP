using System;

namespace HIC.Logging
{
    /// <summary>
    /// A conceptual 'source' which contributed records to a table being loaded during a logged activity (See TableLoadInfo).  This can be as explicit
    /// as a flat file 'myfile.csv' or as isoteric as an sql query run on a server (e.g. during extraction we audit the extraction sql with one of these).
    /// </summary>
    public class DataSource
    {
        public DataSource(string source, DateTime originDate)
        {
            Source = source;
            OriginDate = originDate;
            UnknownOriginDate = false;
        }

        public DataSource(string source)
        {
            Source = source;
            UnknownOriginDate = true;
        }

        public int ID { get; internal set; }
        public string Source { get; set; }
        public string Archive { get; set; }
        public DateTime OriginDate { get; internal set; }
        public bool UnknownOriginDate { get; internal set;}

        private byte[] _md5;
        public byte[] MD5 {
            get
            {
                return _md5;
            }
            set
            {
           
                _md5 = value;
            } 
        }
    }
}