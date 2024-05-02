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
///     A Constraint specifying that the date must be a valid, delimited EU format date. e.g. 25-09-67 or 25-9-1967.
///     As such, the leftmost digits are assumed to be the DAY value and the rightmost digits, the YEAR value.
/// </summary>
public partial class Date : PrimaryConstraint
{
    private const string RegExp = @"^(\d{1,2})(\.|/|-)(\d{1,2})(\.|/|-)(\d{2}(\d{2})?)$";

    private readonly CultureInfo _ukCulture;

    public Date()
    {
        _ukCulture = new CultureInfo("en-GB");
    }

    /// <summary>
    ///     Validate a string representation of a UK (ONLY) date of the format d[d]/m[m]/yy[yy].
    ///     The standard C# DateTime.Parse() method is used, which accepts alternative separators such as '.' and '-'.
    /// </summary>
    /// <param name="value"></param>
    public override ValidationFailure Validate(object value)
    {
        switch (value)
        {
            case DateTime:
            case null:
                return null;
            default:
                try
                {
                    var s = (string)value;
                    DateTime.Parse(s, _ukCulture.DateTimeFormat);

                    if (NotAFullySpecifiedDate(s))
                        return new ValidationFailure("Partial dates not allowed.", this);
                }
                catch (FormatException ex)
                {
                    return new ValidationFailure(ex.Message, this);
                }

                return null;
        }
    }

    private static bool NotAFullySpecifiedDate(string s)
    {
        var match = DateRegex().Match(s);
        return !match.Success;
    }


    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return "Checks that the data type is DateTime or a string which can be parsed into a valid DateTime";
    }

    [GeneratedRegex("^(\\d{1,2})(\\.|/|-)(\\d{1,2})(\\.|/|-)(\\d{2}(\\d{2})?)$")]
    private static partial Regex DateRegex();
}