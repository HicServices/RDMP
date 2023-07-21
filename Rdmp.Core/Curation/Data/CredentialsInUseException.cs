// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Exception thrown when you attempt to delete an DataAccessCredentials upon which a TableInfo or other class relies
///     upon to access data.
/// </summary>
public class CredentialsInUseException : Exception
{
    /// <inheritdoc>
    ///     <cref>base(string)</cref>
    /// </inheritdoc>
    public CredentialsInUseException(string s) : base(s)
    {
    }

    /// <inheritdoc>
    ///     <cref>base(string, Exception)</cref>
    /// </inheritdoc>
    public CredentialsInUseException(string s, Exception inner) : base(s, inner)
    {
    }
}