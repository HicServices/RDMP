// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
/// Provides extension methods for all objects
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Returns a beautiful representation of the given object.  For example splitting array elements into
    /// a comma separated list.  Falls back on calling <paramref name="o"/> normal ToString if no specific logic
    /// is defined for its Type.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static string ToBeautifulString(this object o)
    {
        return o is Array a
            ? string.Join(", ", a.Cast<object>().Select(v => v == null ? "" : v.ToString()).ToArray())
            : o.ToString();
    }
}