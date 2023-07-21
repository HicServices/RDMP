// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Icons.IconProvision;

/// <summary>
///     Exception thrown when an icon cannot be properly provided for an object or setting up an icon
///     cache fails
/// </summary>
public class IconProvisionException : Exception
{
    /// <summary>
    ///     Creates a new instance with the given message
    /// </summary>
    /// <param name="msg">Text of the error</param>
    public IconProvisionException(string msg) : base(msg)
    {
    }

    /// <summary>
    ///     Creates a new instance with the given message and inner exception
    /// </summary>
    /// <param name="msg">Text of the error</param>
    /// <param name="ex">Inner exception triggering the situation</param>
    public IconProvisionException(string msg, Exception ex) : base(msg, ex)
    {
    }
}