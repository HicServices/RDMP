// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Validation.Constraints.Primary;

/// <summary>
///     A rule for validating the data in a column.  Each column can have a single <see cref="IPrimaryConstraint" /> but
///     many
///     <see cref="ISecondaryConstraint" />
/// </summary>
public abstract class PrimaryConstraint : IPrimaryConstraint
{
    /// <inheritdoc />
    public Consequence? Consequence { get; set; }

    /// <inheritdoc />
    public abstract void RenameColumn(string originalName, string newName);

    /// <inheritdoc />
    public abstract string GetHumanReadableDescriptionOfValidation();

    /// <inheritdoc />
    public abstract ValidationFailure Validate(object value);
}