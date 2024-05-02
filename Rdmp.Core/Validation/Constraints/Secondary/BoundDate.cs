// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Values (if present) in a column must be within a certain range of dates.  This can include referencing another
///     column.  For example you could specify that
///     Date of Birth must have an Inclusive Upper bound of Date of Death.
/// </summary>
public class BoundDate : Bound
{
    [Description("Optional, Requires the value being validated to be AFTER this date")]
    public DateTime? Lower { get; set; }

    [Description("Optional, Requires the value being validated to be BEFORE this date")]
    public DateTime? Upper { get; set; }

    public BoundDate()
    {
        Inclusive = true;
    }

    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        switch (value)
        {
            case null:
                return null;
            case string stringValue:
            {
                value = SafeConvertToDate(stringValue);

                if (!((DateTime?)value).HasValue)
                    return null;
                break;
            }
        }

        var d = (DateTime)value;

        if (value != null && !IsWithinRange(d))
            return new ValidationFailure(CreateViolationReportUsingDates(d), this);

        return value != null && !IsWithinRange(d, otherColumns, otherColumnNames)
            ? new ValidationFailure(CreateViolationReportUsingFieldNames(d), this)
            : null;
    }

    private bool IsWithinRange(DateTime d)
    {
        if (Inclusive)
        {
            if (d < Lower)
                return false;

            if (d > Upper)
                return false;
        }
        else
        {
            if (d <= Lower)
                return false;

            if (d >= Upper)
                return false;
        }

        return true;
    }

    private bool IsWithinRange(DateTime d, object[] otherColumns, string[] otherColumnNames)
    {
        var low = SafeConvertToDate(LookupFieldNamed(LowerFieldName, otherColumns, otherColumnNames));
        var up = SafeConvertToDate(LookupFieldNamed(UpperFieldName, otherColumns, otherColumnNames));

        if (Inclusive)
        {
            if (low.HasValue && d < low.Value)
                return false;

            if (up.HasValue && d > up.Value)
                return false;
        }
        else
        {
            if (low.HasValue && d <= low.Value)
                return false;

            if (up.HasValue && d >= up.Value)
                return false;
        }

        return true;
    }

    private static DateTime? SafeConvertToDate(object lookupFieldNamed)
    {
        if (lookupFieldNamed == null)
            return null;

        if (lookupFieldNamed == DBNull.Value)
            return null;

        if (lookupFieldNamed is DateTime time)
            return time;

        if (lookupFieldNamed is string named)
        {
            if (string.IsNullOrWhiteSpace(named))
                return null;
            try
            {
                lookupFieldNamed = DateTime.Parse(named);
            }
            catch (InvalidCastException)
            {
                return
                    null; //it's not our responsibility to look for malformed dates in this constraint (leave that to primary constraint date)
            }
            catch (FormatException)
            {
                return null;
            }

            return (DateTime)lookupFieldNamed;
        }

        throw new ArgumentException($"Did not know how to deal with object of type {lookupFieldNamed.GetType().Name}");
    }

    private string CreateViolationReportUsingDates(DateTime d)
    {
        if (Lower != null && Upper != null)
            return BetweenMessage(d, Lower.ToString(), Upper.ToString());

        if (Lower != null)
            return GreaterThanMessage(d, Lower.ToString());

        return Upper != null
            ? LessThanMessage(d, Upper.ToString())
            : throw new InvalidOperationException("Illegal state.");
    }

    private string CreateViolationReportUsingFieldNames(DateTime d)
    {
        if (!string.IsNullOrWhiteSpace(LowerFieldName) && !string.IsNullOrWhiteSpace(UpperFieldName))
            return BetweenMessage(d, LowerFieldName, UpperFieldName);

        if (!string.IsNullOrWhiteSpace(LowerFieldName))
            return GreaterThanMessage(d, LowerFieldName);

        return !string.IsNullOrWhiteSpace(UpperFieldName)
            ? LessThanMessage(d, UpperFieldName)
            : throw new InvalidOperationException("Illegal state.");
    }

    private string BetweenMessage(DateTime d, string l, string u)
    {
        return
            $"Date {Wrap(d.ToString(CultureInfo.InvariantCulture))} out of range. Expected a date between {Wrap(l)} and {Wrap(u)}{(Inclusive ? " inclusively" : " exclusively")}.";
    }

    private static string GreaterThanMessage(DateTime d, string s)
    {
        return
            $"Date {Wrap(d.ToString(CultureInfo.InvariantCulture))} out of range. Expected a date greater than {Wrap(s)}.";
    }

    private static string LessThanMessage(DateTime d, string s)
    {
        return
            $"Date {Wrap(d.ToString(CultureInfo.InvariantCulture))} out of range. Expected a date less than {Wrap(s)}.";
    }

    private static string Wrap(string s)
    {
        return $"[{s}]";
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        var result = "Checks that a date is within a given set of bounds.  This field is currently configured to be ";

        if (Lower != null)
            if (Inclusive)
                result += $" >={Lower}";
            else
                result += $" >{Lower}";

        if (Upper != null)
            if (Inclusive)
                result += $" <={Upper}";
            else
                result += $" <{Upper}";

        return result;
    }
}