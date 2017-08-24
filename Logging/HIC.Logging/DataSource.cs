using System;

namespace HIC.Logging
{
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