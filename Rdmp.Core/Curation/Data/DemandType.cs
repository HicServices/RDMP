// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Used to define non Type semantically important things about a [DemandsInitialization] which cannot be determined
///     from the Type of the property.  For example if a
///     System.String property is expected to contain Sql then this DemandType can be specified which will result in a
///     better user experience than a basic Textbox when it
///     comes time to provide a value at Design time.
/// </summary>
public enum DemandType
{
    /// <summary>
    ///     There is no special subcategory
    /// </summary>
    Unspecified,

    /// <summary>
    ///     The property is String but it should be rendered/edited the user interface as a SQL syntax (e.g. big editor with
    ///     highlighting)
    /// </summary>
    SQL
}