// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;

/// <summary>
///     Helper for finding public properties on objects which are decorated with a given Attribute.  Includes
///     support for set/get and rapidly determining which Types have relevant properties.
/// </summary>
public interface IAttributePropertyFinder
{
    IEnumerable<PropertyInfo> GetProperties(IMapsDirectlyToDatabaseTable o);
    bool ObjectContainsProperty(IMapsDirectlyToDatabaseTable arg);
}