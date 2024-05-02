// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Xml;
using System.Xml.Serialization;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Note all times are stored as UTC
/// </summary>
public class PermissionWindowPeriod
{
    /// <summary>
    ///     Which day this period is in (periods cannot cross day boundaries)
    /// </summary>
    public int DayOfWeek { get; set; }

    // XML Serialiser doesn't support TimeSpan :(

    /// <summary>
    ///     The start time on the day in which the activity becomes allowable
    /// </summary>
    [XmlIgnore]
    public TimeSpan Start { get; set; }

    /// <summary>
    ///     The end time on the day in which the activity is no longer allowed
    /// </summary>
    [XmlIgnore]
    public TimeSpan End { get; set; }

    #region Required for correct XML serialisation

    // These pollute the public API unfortunately, but are required
    /// <inheritdoc cref="Start" />
    [XmlElement(DataType = "duration", ElementName = "Start")]
    public string StartString
    {
        get => XmlConvert.ToString(Start);
        set => Start = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
    }

    /// <inheritdoc cref="End" />
    [XmlElement(DataType = "duration", ElementName = "End")]
    public string EndString
    {
        get => XmlConvert.ToString(End);
        set => End = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
    }

    /// <summary>
    ///     Used by serialization only
    /// </summary>
    internal PermissionWindowPeriod()
    {
        // needed for serialisation
    }

    #endregion

    /// <summary>
    ///     Defines a period of day during which an activity is allowable
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public PermissionWindowPeriod(int dayOfWeek, TimeSpan start, TimeSpan end)
    {
        DayOfWeek = dayOfWeek;
        Start = start;
        End = end;
    }

    /// <summary>
    ///     True if the <paramref name="timeToTest" /> falls with the allowable time range (and on the correct
    ///     <see cref="DayOfWeek" />)
    /// </summary>
    /// <param name="timeToTest"></param>
    /// <param name="testToNearestSecond"></param>
    /// <returns></returns>
    public bool Contains(DateTime timeToTest, bool testToNearestSecond = false)
    {
        if ((int)timeToTest.DayOfWeek != DayOfWeek)
            return false;

        // If we are not testing to the nearest second, set the seconds var in the test to 0 so any Start and Ends defined without seconds are compared correctly
        var testTime = new TimeSpan(timeToTest.Hour, timeToTest.Minute, testToNearestSecond ? timeToTest.Second : 0);
        return testTime >= Start && testTime <= End;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Start:hh':'mm}-{End:hh':'mm}";
    }
}