// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Curation.Data.Dashboarding;

/// <summary>
///     Occurs when there is an error restoring a specific Persistence String.  Often occurs when a UI class has been
///     renamed or a plugin unloaded between
///     RDMP application executions.
/// </summary>
public class PersistenceException : Exception
{
    /// <summary>
    ///     Throw when you were unable to resolve a saved Control state
    /// </summary>
    /// <param name="message"></param>
    public PersistenceException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Throw when you were unable to resolve a saved Control state and have an <paramref name="exception" />
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public PersistenceException(string message, Exception exception) : base(message, exception)
    {
    }
}