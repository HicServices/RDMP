// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;

namespace Rdmp.Core.Validation.Constraints.Primary;

/// <summary>
///     Field can contain only letters and numbers (but no spaces or symbols)
/// </summary>
public class AlphaNumeric : PrimaryConstraint
{
    public const string RegExp = @"^[A-Za-z0-9]([A-Za-z0-9]*)$";

    public override ValidationFailure Validate(object value)
    {
        if (value == null)
            return null;

        var text = (string)value;
        var match = Regex.Match(text, RegExp);

        return !match.Success
            ? new ValidationFailure($"Value [{value}] contains characters other than alphanumeric", this)
            : null;
    }

    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return "Checks that values have 1 or more characters/numbers in a sequence with no spaces or other punctuation";
    }
}