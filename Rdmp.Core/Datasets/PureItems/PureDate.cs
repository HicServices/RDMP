// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
#nullable enable

using System;

namespace Rdmp.Core.Datasets.PureItems;

/// <summary>
/// Internal PURE system class
/// </summary>
public class PureDate
{
    public PureDate(DateTime dateTime)
    {
        Year = dateTime.Year;
        Month = dateTime.Month;
        Day = dateTime.Day;
    }

    public PureDate() { }


    public DateTime ToDateTime()
    {
        return new DateTime(Year, Month ?? 1, Day ?? 1, 0, 0, 0);
    }

    public bool IsBefore(PureDate date)
    {
        if (Year < date.Year) return true;
        if (Year == date.Year)
        {
            if (Month < date.Month) return true;
            if (Month == date.Month)
            {
                return Day < date.Day;
            }
        }

        return false;
    }

    public PureDate(int year, int? month = null, int? day = null)
    {
        Year = year;
        if (month != null) Month = month;
        if (day != null) Day = day;
    }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
}
