// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Validation.Constraints;

/// <summary>
///     Base interface for all validation rules (and accompanying failure Consqeuences)
/// </summary>
public interface IConstraint
{
    /// <summary>
    ///     The consequences of the rule being broken
    /// </summary>
    Consequence? Consequence { get; set; }

    /// <summary>
    ///     Updates the state / persistence of the <see cref="IConstraint" /> to reflect that it's currently
    ///     referencing column (<paramref name="originalName" />) has been renamed (<paramref name="newName" />)
    ///     and it needs to update its persistence / state.
    /// </summary>
    /// <param name="originalName">The name that this constraint is currently pointing at</param>
    /// <param name="newName">The replacement name that the constraint should update itself to reference</param>
    void RenameColumn(string originalName, string newName);

    /// <summary>
    ///     Human readable description of the validation rule and what it does
    /// </summary>
    /// <returns></returns>
    string GetHumanReadableDescriptionOfValidation();
}