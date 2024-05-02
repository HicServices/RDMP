// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Values (if present) in a column must be within a certain range.  This can include referencing another column.  For
///     example you could specify that
///     Date of Birth must have an Inclusive Upper bound of Date of Death.
/// </summary>
public abstract class Bound : SecondaryConstraint
{
    [Description(
        "Optional, requires that the value being validated is HIGHER than the value stored in the referenced column (where values are present in both columns)")]
    [ExpectsColumnNameAsInput]
    public string LowerFieldName { get; set; }

    [Description(
        "Optional, requires that the value being validated is LOWER than the value stored in the referenced column (where values are present in both columns)")]
    [ExpectsColumnNameAsInput]
    public string UpperFieldName { get; set; }

    [Description(
        "When ticked allows values that are EXACTLY THE SAME AS either the Upper or Lower boundary (including field boundaries) to pass validation")]
    public bool Inclusive { get; set; }


    protected static object LookupFieldNamed(string name, object[] otherColumns, object[] otherColumnNames)
    {
        for (var i = 0; i < otherColumnNames.Length; i++)
            if (otherColumnNames[i].Equals(name))
                return otherColumns[i];

        if (!string.IsNullOrWhiteSpace(name))
            SignalThatFieldWasNotFound(name);

        return null;
    }

    private static void SignalThatFieldWasNotFound(string name)
    {
        throw new MissingFieldException($"Validation failed: Comparator field [{name}] not found in dictionary.");
    }


    public override void RenameColumn(string originalName, string newName)
    {
        if (LowerFieldName != null)
            if (LowerFieldName.EndsWith(originalName))
                LowerFieldName = newName;

        if (UpperFieldName != null)
            if (UpperFieldName.EndsWith(originalName))
                UpperFieldName = newName;
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        var result = "";

        if (LowerFieldName != null)
            if (Inclusive)
                result += $" >={LowerFieldName}";
            else
                result += $" >{LowerFieldName}";

        if (UpperFieldName != null)
            if (Inclusive)
                result += $" <={UpperFieldName}";
            else
                result += $" <{UpperFieldName}";

        return result;
    }
}