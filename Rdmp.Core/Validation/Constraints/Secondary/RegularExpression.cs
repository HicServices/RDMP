// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Values being validated are expected to pass the Regex pattern
/// </summary>
public class RegularExpression : SecondaryConstraint
{
    private string _pattern;

    public RegularExpression()
    {
        // Required for serialisation.
    }

    public RegularExpression(string pattern)
    {
        _pattern = pattern;
    }

    [Description(
        "The Regular Expression pattern that MUST match the value being validated.  If you find yourself copy pasting the same Pattern all over the place you should instead consider a StandardRegexConstraint")]
    [ExpectsLotsOfText]
    public string Pattern
    {
        get => _pattern;
        set
        {
            //throws if you pass an invalid pattern
            new Regex(value);


            _pattern = value;
        }
    }

    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (value == null)
            return null;

        //if it is a basic type e.g. user wants to validate using regex [0-9]? (single digit!) then let them
        if (value is string == false)
            value = Convert.ToString(value);

        var text = (string)value;
        var match = Regex.Match(text, _pattern);

        return !match.Success
            ? new ValidationFailure($"Failed to match text [{value}] to regular expression /{_pattern}/", this)
            : null;
    }

    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return $"Matches regex {Pattern}";
    }
}