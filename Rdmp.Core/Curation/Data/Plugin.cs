// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     A nupkg file which contains compiled code to add additional capabilities to RDMP (e.g. to handle Dicom images).
///     Plugins are loaded and
///     stored in the RDMP platform databases and written to disk/loaded when executed by the RDMP client - this ensures
///     that all users run the same
///     version of the Plugin(s).
/// </summary>
public class Plugin
{
    #region Database Properties

    /// <inheritdoc />
    [NotNull]
    public string Name { get; }

    #endregion
}