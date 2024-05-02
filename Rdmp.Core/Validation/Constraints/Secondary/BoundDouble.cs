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
///     Values (if present) in a column must be within a certain range of numeric values.  This can include referencing
///     another column.  For example you could
///     specify that the column 'AverageResult' must have an Inclusive Upper bound of the column 'MaxResult'.
/// </summary>
public class BoundDouble : Bound
{
    [Description("Optional, Requires the value being validated to be HIGHER than this number")]
    public double? Lower { get; set; }

    [Description("Optional, Requires the value being validated to be LOWER than this number")]
    public double? Upper { get; set; }

    public BoundDouble()
    {
        Inclusive = true;
    }

    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        switch (value)
        {
            //nulls are fine
            case null:
            //nulls are also fine if we are passed blanks
            case string s when string.IsNullOrWhiteSpace(s):
                return null;
        }

        double v;

        try
        {
            v = Convert.ToDouble(value);
        }
        catch (FormatException)
        {
            return new ValidationFailure("Invalid format for double ", this);
        }


        if (Lower.HasValue || Upper.HasValue)
            if (value != null && !IsWithinRange(v))
                return new ValidationFailure(CreateViolationReportUsingValues(v), this);

        if (value != null && !IsWithinRange(v, otherColumns, otherColumnNames))
            return new ValidationFailure(CreateViolationReportUsingFieldNames(v), this);

        //            if (v < Lower || v > Upper)
        //                throw new ValidationException("Value [" + v + "] out of range. Expected a value between " + Lower + " and " + Upper + ".");

        return null;
    }

    private bool IsWithinRange(double v)
    {
        if (Inclusive)
        {
            if (v < Lower)
                return false;

            if (v > Upper)
                return false;
        }
        else
        {
            if (v <= Lower)
                return false;

            if (v >= Upper)
                return false;
        }

        return true;
    }

    private bool IsWithinRange(double d, object[] otherColumns, string[] otherColumnNames)
    {
        var low = LookupFieldNamed(LowerFieldName, otherColumns, otherColumnNames);
        var up = LookupFieldNamed(UpperFieldName, otherColumns, otherColumnNames);

        var l = Convert.ToDouble(low);
        var u = Convert.ToDouble(up);

        if (Inclusive)
        {
            if (low != null && d < l)
                return false;

            if (up != null && d > u)
                return false;
        }
        else
        {
            if (low != null && d <= l)
                return false;

            if (up != null && d >= u)
                return false;
        }

        return true;
    }

    private string CreateViolationReportUsingValues(double d)
    {
        if (Lower.HasValue && Upper.HasValue)
            return BetweenMessage(d, Lower.ToString(), Upper.ToString());

        if (Lower.HasValue)
            return GreaterThanMessage(d, Lower.ToString());

        return Upper.HasValue
            ? LessThanMessage(d, Upper.ToString())
            : throw new InvalidOperationException("Illegal state.");
    }

    private string CreateViolationReportUsingFieldNames(double d)
    {
        if (!string.IsNullOrWhiteSpace(LowerFieldName) && !string.IsNullOrWhiteSpace(UpperFieldName))
            return BetweenMessage(d, LowerFieldName, UpperFieldName);

        if (!string.IsNullOrWhiteSpace(LowerFieldName))
            return GreaterThanMessage(d, LowerFieldName);

        return !string.IsNullOrWhiteSpace(UpperFieldName)
            ? LessThanMessage(d, UpperFieldName)
            : throw new InvalidOperationException("Illegal state.");
    }

    private string BetweenMessage(double d, string l, string u)
    {
        return
            $"Value {Wrap(d.ToString(CultureInfo.CurrentCulture))} out of range. Expected a value between {Wrap(l)} and {Wrap(u)}{(Inclusive ? " inclusively" : " exclusively")}.";
    }

    private static string GreaterThanMessage(double d, string s)
    {
        return
            $"Value {Wrap(d.ToString(CultureInfo.CurrentCulture))} out of range. Expected a value greater than {Wrap(s)}.";
    }

    private static string LessThanMessage(double d, string s)
    {
        return
            $"Value {Wrap(d.ToString(CultureInfo.CurrentCulture))} out of range. Expected a value less than {Wrap(s)}.";
    }

    private static string Wrap(string s)
    {
        return $"[{s}]";
    }

    public BoundDouble And(int upper)
    {
        Upper = upper;

        return this;
    }

    public void Incl()
    {
        Inclusive = true;
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        var result = base.GetHumanReadableDescriptionOfValidation();


        if (Lower != double.MinValue)
            if (Inclusive)
                result += $" >={Lower}";
            else
                result += $" >{Lower}";

        if (Upper != double.MaxValue)
            if (Inclusive)
                result += $" <={Upper}";
            else
                result += $" <{Upper}";

        return result;
    }
}