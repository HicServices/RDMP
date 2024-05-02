// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;

/// <summary>
///     Used to indicate a property that contains sql e.g. Where logic, Select logic etc.  Using this property makes the
///     Attribute
///     'find and replaceable' through the FindAndReplaceUI
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SqlAttribute : Attribute
{
    /// <summary>
    ///     True to skip the component when executing (but still show it at design time).
    /// </summary>
    private bool IsDisabled { get; set; }
}