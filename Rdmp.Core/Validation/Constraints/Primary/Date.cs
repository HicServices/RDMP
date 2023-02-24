// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Rdmp.Core.Validation.Constraints.Primary;

/// <summary>
/// A Constraint specifying that the date must be a valid, delimited EU format date. e.g. 25-09-67 or 25-9-1967.
/// As such, the leftmost digits are assumed to be the DAY value and the rightmost digits, the YEAR value.
/// </summary>
public class Date : PrimaryConstraint
{
    private static readonly Regex RegExp = new(@"^(\d{1,2})(\.|/|-)(\d{1,2})(\.|/|-)(\d{2}(\d{2})?)$",RegexOptions.Compiled);

    private static readonly DateTimeFormatInfo UkDateFormat = new CultureInfo("en-GB").DateTimeFormat;

    /// <summary>
    /// Validate a string representation of a UK (ONLY) date of the format d[d]/m[m]/yy[yy].
    /// The standard C# DateTime.Parse() method is used, which accepts alternative separators such as '.' and '-'.
    /// </summary>
    /// <param name="value"></param>
    public override ValidationFailure Validate(object value)
    {
        if (value is DateTime or null)
            return null;

        var s = (string)value;
        if (!DateTime.TryParse(s, UkDateFormat, DateTimeStyles.None, out _))
            return new ValidationFailure($"Unable to parse '{s}' as a date", this);

        if (NotAFullySpecifiedDate(s)) 
            return new ValidationFailure("Partial dates not allowed.",this);

        return null;
    }

    private static bool NotAFullySpecifiedDate(string s)
    {
        return !RegExp.IsMatch(s);
    }


    public override void RenameColumn(string originalName, string newName)
    {
            
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return "Checks that the data type is DateTime or a string which can be parsed into a valid DateTime";
    }
}