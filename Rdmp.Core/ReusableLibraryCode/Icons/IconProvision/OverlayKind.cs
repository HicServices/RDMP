// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

/// <summary>
///     Describes a small overlay image that appears on top of the main icon to indicate something about it (e.g. that
///     there is a problem with it).
/// </summary>
public enum OverlayKind
{
    None = 0,
    Add,
    Problem,
    Link,
    Shortcut,
    Execute,
    Import,
    Extractable,
    Extractable_Internal,
    Extractable_SpecialApproval,
    Extractable_Supplemental,
    Key,
    Filter,
    FavouredItem,
    Deprecated,
    Internal,
    Delete,
    Edit,
    Locked,
    Help,
    Hashed,
    Cloud,
    BigE,
    IsExtractionIdentifier,
    Parameter
}