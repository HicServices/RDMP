using System;

namespace CatalogueLibrary.Data.Cache
{
    /// <summary>
    /// Created this because TimeSpans can't handle months and I didn't want to go to the hassle of importing an entire library such as NodaTime just to be able to deal with months.
    /// 
    /// Serialises to/from simple string representation of duration + type, where type is month (m) or day (d)
    /// </summary>
    public class CacheLagPeriod
    {
        /// <summary>
        /// Is the lag period measured in moths or days
        /// </summary>
        public enum PeriodType
        {
            Month,
            Day
        };

        public int Duration { get; set; }
        public PeriodType Type { get; set; }

        // if a null/empty string is passed in to the constructor then there is no lag period, create it as '0d'
        private const int DefaultDuration = 0;
        private const PeriodType DefaultType = PeriodType.Day;

        /// <summary>
        /// Returns the Months component represented by this period, note that if period is 2 Months then Days will still be 0
        /// </summary>
        public int Months { get { return Type == PeriodType.Month ? Duration : 0; } }

        /// <summary>
        /// Returns the Days component represented by this period, note that if period is 2 Months then Days will be 0, therefore you should only use this property in conjunction with Months
        /// </summary>
        public int Days { get { return Type == PeriodType.Day ? Duration : 0; } }

        public static CacheLagPeriod Zero {get { return new CacheLagPeriod(0, PeriodType.Month); }} 

        public CacheLagPeriod(string cacheLagPeriod)
        {
            var type = cacheLagPeriod.Substring(cacheLagPeriod.Length - 1);
            SetTypeFromString(type);

            Duration = string.IsNullOrWhiteSpace(cacheLagPeriod)
                ? DefaultDuration
                : Convert.ToInt32(cacheLagPeriod.Substring(0, cacheLagPeriod.Length - 1));
        }

        public CacheLagPeriod(int duration, PeriodType type)
        {
            Duration = duration;
            Type = type;
        }

        private void SetTypeFromString(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                Type = DefaultType;
                return;
            }

            switch (type)
            {
                case "m": 
                    Type = PeriodType.Month;
                    break;
                case "d":
                    Type = PeriodType.Day;
                    break;
                default:
                    throw new Exception("Period type must be either Month (m) or Day (d)");
            }
        }

        public override string ToString()
        {
            var s = Duration.ToString();
            switch (Type)
            {
                case PeriodType.Month:
                    s += "m";
                    break;
                case PeriodType.Day:
                    s += "d";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return s;
        }

        public DateTime CalculateStartOfLagPeriodFrom(DateTime dt)
        {
            switch (Type)
            {
                case PeriodType.Month:
                    return dt.AddMonths(Duration*-1);
                case PeriodType.Day:
                    return dt.AddDays(Duration*-1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool TimeIsWithinPeriod(DateTime time)
        {
            return time >= CalculateStartOfLagPeriodFrom(DateTime.Now);
        }

        public static bool operator <(TimeSpan timespan, CacheLagPeriod lagPeriod)
        {
            var dt1 = DateTime.Now;
            var dt2 = dt1;

            dt1 = dt1.Add(timespan);
            dt2 = dt2.AddMonths(lagPeriod.Months);
            dt2 = dt2.AddDays(lagPeriod.Days);

            return dt1 < dt2;
        }

        public static bool operator >(TimeSpan timespan, CacheLagPeriod lagPeriod)
        {
            var dt1 = DateTime.Now;
            var dt2 = dt1;

            dt1 = dt1.Add(timespan);
            dt2 = dt2.AddMonths(lagPeriod.Months);
            dt2 = dt2.AddDays(lagPeriod.Days);

            return dt1 > dt2;
        }
    }
}