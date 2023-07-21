// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;

namespace Rdmp.Core;

/// <summary>
///     Thrown when a piece of code needs to know what <see cref="DatabaseType" /> is being targeted but no determination
///     can be made either because there are no objects of a known <see cref="DatabaseType" /> or because there are objects
///     of multiple different <see cref="DatabaseType" />.
/// </summary>
public class AmbiguousDatabaseTypeException : Exception
{
    /// <summary>
    ///     Creates a new Exception with the given message
    /// </summary>
    /// <param name="s"></param>
    public AmbiguousDatabaseTypeException(string s) : base(s)
    {
    }
}