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
/// Values (if present) in a column must be within a certain range of dates.  This can include referencing another column.  For example you could specify that
///  Date of Birth must have an Inclusive Upper bound of Date of Death.
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
            return new ValidationFailure(CreateViolationReportUsingDates(d),this);
            
        if (value != null && !IsWithinRange(d,otherColumns, otherColumnNames))
            return new ValidationFailure(CreateViolationReportUsingFieldNames(d),this);

        return null;
    }

    private bool IsWithinRange(DateTime d)
    {
        if (Inclusive)
            return !(d<Lower) && !(d>Upper);
        return !(d<=Lower) && !(d>=Upper);
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

    private DateTime? SafeConvertToDate(object lookupFieldNamed)
    {
        if (lookupFieldNamed == null)
            return null;

        if (lookupFieldNamed == DBNull.Value)
            return null;

        return lookupFieldNamed switch
        {
            DateTime dateTime => dateTime,
            string named when string.IsNullOrWhiteSpace(named) => null,
            string named => DateTime.TryParse(named, out var result) ? result : null,
            _ => throw new ArgumentException(
                $"Did not know how to deal with object of type {lookupFieldNamed.GetType().Name}")
        };
    }

    private string CreateViolationReportUsingDates(DateTime d)
    {
        if (Lower != null && Upper != null)
            return BetweenMessage(d, Lower.ToString(), Upper.ToString());

        if (Lower != null)
            return GreaterThanMessage(d, Lower.ToString());

        if (Upper!= null)
            return LessThanMessage(d, Upper.ToString());

        throw new InvalidOperationException("Illegal state.");
    }

    private string CreateViolationReportUsingFieldNames(DateTime d)
    {
        if (!string.IsNullOrWhiteSpace(LowerFieldName) && !string.IsNullOrWhiteSpace(UpperFieldName)) 
            return BetweenMessage(d, LowerFieldName, UpperFieldName);

        if (!string.IsNullOrWhiteSpace(LowerFieldName))
            return GreaterThanMessage(d, LowerFieldName);

        if (!string.IsNullOrWhiteSpace(UpperFieldName))
            return LessThanMessage(d, UpperFieldName);

        throw new InvalidOperationException("Illegal state.");
    }

    private string BetweenMessage(DateTime d, string l, string u)
    {
        return
            $"Date {Wrap(d.ToString(CultureInfo.InvariantCulture))} out of range. Expected a date between {Wrap(l)} and {Wrap(u)}{(Inclusive ? " inclusively" : " exclusively")}.";
    }

    private string GreaterThanMessage(DateTime d, string s)
    {
        return
            $"Date {Wrap(d.ToString(CultureInfo.InvariantCulture))} out of range. Expected a date greater than {Wrap(s)}.";
    }

    private string LessThanMessage(DateTime d, string s)
    {
        return
            $"Date {Wrap(d.ToString(CultureInfo.InvariantCulture))} out of range. Expected a date less than {Wrap(s)}.";
    }

    private string Wrap(string s)
    {
        return $"[{s}]";
    }
        
    public override string GetHumanReadableDescriptionOfValidation()
    {
        var result = "Checks that a date is within a given set of bounds.  This field is currently configured to be ";
            
        if (Lower != null )
            if(Inclusive)
                result += $" >={Lower}";
            else
                result += $" >{Lower}";
            
        if(Upper != null)
            if (Inclusive)
                result += $" <={Upper}";
            else
                result += $" <{Upper}";

        return result;
    }
}