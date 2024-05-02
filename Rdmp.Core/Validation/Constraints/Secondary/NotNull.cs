// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Values must appear in this column, if there are nulls (or whitespace) then the validation will fail.  While this
///     kind of thing is trivially easy to implement
///     at database level you might decided that (especially for unimportant columns) you are happy to load missing data
///     rather than crash the data load.  That
///     is why this constraint exists.
/// </summary>
public class NotNull : SecondaryConstraint
{
    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (value == null || value == DBNull.Value)
            return new ValidationFailure("Value cannot be null", this);

        return value is string && string.IsNullOrWhiteSpace(value.ToString())
            ? new ValidationFailure("Value cannot be whitespace only", this)
            : null;
    }

    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return "not null";
    }
}