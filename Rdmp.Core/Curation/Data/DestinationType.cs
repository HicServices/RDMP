// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes the destination of an artifact (e.g. a linked project extract)
/// </summary>
public enum DestinationType
{
    /// <summary>
    ///     Destination is unknown
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Artifact destination is a database (e.g. into an sql server instance)
    /// </summary>
    Database,

    /// <summary>
    ///     Artifact destination is the file system (e.g. into a folder)
    /// </summary>
    FileSystem
}