using System;
using System.Xml;
using System.Xml.Serialization;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Note all times are stored as UTC
    /// </summary>
    public class PermissionWindowPeriod
    {
        public int DayOfWeek { get; set; }
        
        // XML Serialiser doesn't support TimeSpan :(
        [XmlIgnore]
        public TimeSpan Start { get; set; }
        [XmlIgnore]
        public TimeSpan End { get; set; }

        #region Required for correct XML serialisation
        // These pollute the public API unfortunately, but are required
        [XmlElement(DataType = "duration", ElementName = "Start")]
        public string StartString
        {
            get { return XmlConvert.ToString(Start); }
            set { Start = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value); }
        }

        [XmlElement(DataType = "duration", ElementName = "End")]
        public string EndString
        {
            get { return XmlConvert.ToString(End); }
            set { End = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value); }
        }

        public PermissionWindowPeriod()
        {
            // needed for serialisation
        }

        #endregion

        public PermissionWindowPeriod(int dayOfWeek, TimeSpan start, TimeSpan end)
        {
            DayOfWeek = dayOfWeek;
            Start = start;
            End = end;
        }

        public bool Contains(DateTime timeToTest, bool testToNearestSecond = false)
        {
            if ((int) timeToTest.DayOfWeek != DayOfWeek)
                return false;

            // If we are not testing to the nearest second, set the seconds var in the test to 0 so any Start and Ends defined without seconds are compared correctly
            var testTime = new TimeSpan(timeToTest.Hour, timeToTest.Minute, testToNearestSecond? timeToTest.Second : 0);
            return testTime >= Start && testTime <= End;
        }

        public override string ToString()
        {
            return Start.ToString("hh':'mm") + "-" + End.ToString("hh':'mm");
        }
    }
}