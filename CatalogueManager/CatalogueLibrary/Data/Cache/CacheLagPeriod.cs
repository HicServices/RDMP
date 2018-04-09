using System;

namespace CatalogueLibrary.Data.Cache
{
    /// <summary>
    /// Created this because TimeSpans can't handle months and I didn't want to go to the hassle of importing an entire library such as NodaTime just to be able to deal with months.
    /// 
    /// <para>Serialises to/from simple string representation of duration + type, where type is month (m) or day (d)</para>
    /// </summary>
    public class CacheLagPeriod
    {
        /// <summary>
        /// Is the lag period measured in months or days
        /// </summary>
        public enum PeriodType
        {
            /// <summary>
            /// Specifies that the <see cref="Duration"/> is a count of Months
            /// </summary>
            Month,

            /// <summary>
            /// Specifies that the <see cref="Duration"/> is a count of Days
            /// </summary>
            Day
        };

        /// <summary>
        /// The number of Days/Months etc (See <see cref="Type"/>) to wait before the associated CacheProgress should be run.  This specifies a window of time
        /// measured from the present not to load e.g. 'do not load any data if it is newer than 3 months since the remote endpoint data source is still volatile
        /// in this range'.
        /// 
        /// <para>Use this when your remote endpoint to which you make Cache fetch requests is not a realtime system in which records are instantly available and
        /// utterly immutable</para>
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Indicates which interval type (Days / Months etc) <see cref="Duration"/> is measured in.
        /// </summary>
        /// <seealso cref="Duration"/>
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

        /// <summary>
        /// Deserializes a <see cref="CacheProgress.CacheLagPeriodLoadDelay"/> string into an instance of <see cref="CacheLagPeriod"/>
        /// </summary>
        /// <param name="cacheLagPeriod"></param>
        internal CacheLagPeriod(string cacheLagPeriod)
        {
            var type = cacheLagPeriod.Substring(cacheLagPeriod.Length - 1);
            SetTypeFromString(type);

            Duration = string.IsNullOrWhiteSpace(cacheLagPeriod)
                ? DefaultDuration
                : Convert.ToInt32(cacheLagPeriod.Substring(0, cacheLagPeriod.Length - 1));
        }

        /// <summary>
        /// Defines a new lag period of the specified number of Months/Days etc
        /// </summary>
        /// <seealso cref="Duration"/>
        /// <param name="duration"></param>
        /// <param name="type"></param>
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

        /// <summary>
        /// String representation of the CacheLagPeriod.  This should be a valid serialization such that it can be loaded by the internal constructor.
        /// </summary>
        /// <remarks>Do not change this without good reason because it's implementation is tied to the internal constructor</remarks>
        /// <returns></returns>
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

        /// <summary>
        /// Adds the <see cref="Duration"/> to the specified DateTime.  Use this to decide whether the lag time is up or not
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        internal DateTime CalculateStartOfLagPeriodFrom(DateTime dt)
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

        /// <summary>
        /// Pass the date you are about to create a cache fetch request for.  Returns true if the date is outside (i.e. older than) the lag period.  E.g. if you have a lag period of 
        /// 6 months then any date before 6 months is valid for fetching since the remote endpoint is likely to have had it's data stabalised.
        /// </summary>
        /// <seealso cref="Duration"/>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool TimeIsWithinPeriod(DateTime time)
        {
            return time >= CalculateStartOfLagPeriodFrom(DateTime.Now);
        }

        /// <summary>
        /// Allows comparing a TimeSpan with a CacheLagPeriod.  This treats the Timespan as an offset equivalent to <see cref="Duration"/>
        /// </summary>
        /// <param name="timespan"></param>
        /// <param name="lagPeriod"></param>
        /// <returns></returns>
        public static bool operator <(TimeSpan timespan, CacheLagPeriod lagPeriod)
        {
            var dt1 = DateTime.Now;
            var dt2 = dt1;

            dt1 = dt1.Add(timespan);
            dt2 = dt2.AddMonths(lagPeriod.Months);
            dt2 = dt2.AddDays(lagPeriod.Days);

            return dt1 < dt2;
        }

        /// <summary>
        /// Allows comparing a TimeSpan with a CacheLagPeriod.  This treats the Timespan as an offset equivalent to <see cref="Duration"/>
        /// </summary>
        /// <param name="timespan"></param>
        /// <param name="lagPeriod"></param>
        /// <returns></returns>
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
