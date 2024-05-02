// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Curation.Data.Cache;

/// <summary>
///     Describes the period of time for which data should not be fetched during caching (See <see cref="CacheProgress" />)
///     in Months/Days.  This allows for the fact that
///     data in the remote server is unlikely to be real time up to the second accurate and you might need to delay
///     requesting data until it has been collected.
///     <para>Serialises to/from simple string representation of duration + type, where type is month (m) or day (d)</para>
/// </summary>
/// <remarks>
///     Created this because TimeSpans can't handle months and I didn't want to go to the hassle of importing an
///     entire library such as NodaTime just to be able to deal with months
/// </remarks>
public class CacheLagPeriod
{
    /// <summary>
    ///     Is the lag period measured in months or days
    /// </summary>
    public enum PeriodType
    {
        /// <summary>
        ///     Specifies that the <see cref="Duration" /> is a count of Months
        /// </summary>
        Month,

        /// <summary>
        ///     Specifies that the <see cref="Duration" /> is a count of Days
        /// </summary>
        Day
    }

    /// <summary>
    ///     The number of Days/Months etc (See <see cref="Type" />) to wait before the associated CacheProgress should be run.
    ///     This specifies a window of time
    ///     measured from the present not to load e.g. 'do not load any data if it is newer than 3 months since the remote
    ///     endpoint data source is still volatile
    ///     in this range'.
    ///     <para>
    ///         Use this when your remote endpoint to which you make Cache fetch requests is not a realtime system in which
    ///         records are instantly available and
    ///         utterly immutable
    ///     </para>
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    ///     Indicates which interval type (Days / Months etc) <see cref="Duration" /> is measured in.
    /// </summary>
    /// <seealso cref="Duration" />
    public PeriodType Type { get; set; }

    // if a null/empty string is passed in to the constructor then there is no lag period, create it as '0d'
    private const int DefaultDuration = 0;
    private const PeriodType DefaultType = PeriodType.Day;

    /// <summary>
    ///     Returns the Months component represented by this period, note that if period is 2 Months then Days will still be 0
    /// </summary>
    public int Months => Type == PeriodType.Month ? Duration : 0;

    /// <summary>
    ///     Returns the Days component represented by this period, note that if period is 2 Months then Days will be 0,
    ///     therefore you should only use this property in conjunction with Months
    /// </summary>
    public int Days => Type == PeriodType.Day ? Duration : 0;

    /// <summary>
    ///     Define a Zero length CacheLagPeriod i.e. the remote end point from which caching happens is real time up to the
    ///     millisecond so you can always issue a cache fetch
    ///     request for data up to DateTime.Now (obviously you can't request future data).
    /// </summary>
    public static CacheLagPeriod Zero => new(0, PeriodType.Month);

    /// <summary>
    ///     Deserializes a <see cref="CacheProgress.CacheLagPeriodLoadDelay" /> string into an instance of
    ///     <see cref="CacheLagPeriod" />
    /// </summary>
    /// <param name="cacheLagPeriod"></param>
    internal CacheLagPeriod(string cacheLagPeriod)
    {
        var type = cacheLagPeriod[^1..];
        SetTypeFromString(type);

        Duration = string.IsNullOrWhiteSpace(cacheLagPeriod)
            ? DefaultDuration
            : Convert.ToInt32(cacheLagPeriod[..^1]);
    }

    /// <summary>
    ///     Defines a new lag period of the specified number of Months/Days etc
    /// </summary>
    /// <seealso cref="Duration" />
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

        Type = type switch
        {
            "m" => PeriodType.Month,
            "d" => PeriodType.Day,
            _ => throw new Exception("Period type must be either Month (m) or Day (d)")
        };
    }

    /// <summary>
    ///     String representation of the CacheLagPeriod.  This should be a valid serialization such that it can be loaded by
    ///     the internal constructor.
    /// </summary>
    /// <remarks>Do not change this without good reason because its implementation is tied to the internal constructor</remarks>
    /// <returns></returns>
    public override string ToString()
    {
        var s = Duration.ToString();
        s += Type switch
        {
            PeriodType.Month => "m",
            PeriodType.Day => "d",
            _ => throw new ArgumentOutOfRangeException()
        };

        return s;
    }

    /// <summary>
    ///     Adds the <see cref="Duration" /> to the specified DateTime.  Use this to decide whether the lag time is up or not
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    internal DateTime CalculateStartOfLagPeriodFrom(DateTime dt)
    {
        return Type switch
        {
            PeriodType.Month => dt.AddMonths(Duration * -1),
            PeriodType.Day => dt.AddDays(Duration * -1d),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    ///     Pass the date you are about to create a cache fetch request for.  Returns true if the date is outside (i.e. older
    ///     than) the lag period.  E.g. if you have a lag period of
    ///     6 months then any date before 6 months is valid for fetching since the remote endpoint is likely to have had its
    ///     data stabilised.
    /// </summary>
    /// <seealso cref="Duration" />
    /// <param name="time"></param>
    /// <returns></returns>
    public bool TimeIsWithinPeriod(DateTime time)
    {
        return time >= CalculateStartOfLagPeriodFrom(DateTime.Now);
    }

    /// <summary>
    ///     Allows comparing a TimeSpan with a CacheLagPeriod.  This treats the Timespan as an offset equivalent to
    ///     <see cref="Duration" />
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
    ///     Allows comparing a TimeSpan with a CacheLagPeriod.  This treats the Timespan as an offset equivalent to
    ///     <see cref="Duration" />
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