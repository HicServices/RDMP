// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Startup.Events;

/// <summary>
///     Describes the state of one of the RDMP platform databases or satellite databases (e.g. Logging, third party plugin
///     databases etc)
/// </summary>
public enum RDMPPlatformDatabaseStatus
{
    /// <summary>
    ///     The database could not be reached
    /// </summary>
    Unreachable,

    /// <summary>
    ///     The database is not in a valid state for some unknown reason
    /// </summary>
    Broken,

    /// <summary>
    ///     The database schema is behind the current software version and there are
    ///     unapplied patch files for that database
    /// </summary>
    RequiresPatching,

    /// <summary>
    ///     The database is in a valid state
    /// </summary>
    Healthy,

    /// <summary>
    ///     The database being read is at least one version ahead of the currently running software.
    /// </summary>
    SoftwareOutOfDate
}