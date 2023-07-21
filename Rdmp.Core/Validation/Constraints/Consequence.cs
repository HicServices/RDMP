// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Validation.Constraints;

/// <summary>
///     Describes the severity of a failing <see cref="Validator" /> <see cref="IConstraint" /> (i.e. a rule in the Data
///     Quality Engine)
/// </summary>
public enum Consequence
{
    /// <summary>
    ///     When the <see cref="IConstraint" /> is broken then it means important information is missing in the record
    /// </summary>
    Missing,

    /// <summary>
    ///     When the <see cref="IConstraint" /> is broken then it means that the wrong important information is present in the
    ///     cell of the record
    /// </summary>
    Wrong,

    /// <summary>
    ///     When the <see cref="IConstraint" /> is broken then it means that the entire record should be considered 'bad'
    /// </summary>
    InvalidatesRow
}