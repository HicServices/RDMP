// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Validation.Constraints.Primary;

/// <summary>
///     Each column can have a single PrimaryConstraint, this is usually related to the datatype (either exact e.g.
///     DateTime or semantic e.g. NHS number).
///     Validation of a PrimaryConstraint involves ensuring that the value is of the correct pattern/type as the concept.
/// </summary>
public interface IPrimaryConstraint : IConstraint
{
    /// <summary>
    ///     Validates the current cell <paramref name="value" /> returning null or a <see cref="ValidationFailure" />
    ///     describing
    ///     the reason it does not pass the <see cref="IConstraint" />
    /// </summary>
    /// <param name="value">The cell value that must be validated</param>
    /// <returns>null if valid otherwise the reason for validation failing</returns>
    ValidationFailure Validate(object value);
}