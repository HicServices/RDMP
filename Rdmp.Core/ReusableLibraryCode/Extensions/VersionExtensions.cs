// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Extensions;

public static class VersionExtensions
{
    /// <summary>
    /// Returns true if the two versions are identical up to the significant parts specified
    /// </summary>
    /// <param name="version">The version which depends on <paramref name="other"/> (can include short version e.g. "3.0" will have -1 for Build) </param>
    /// <param name="other">The full version</param>
    /// <param name="significantParts"></param>
    /// <returns></returns>
    public static bool IsCompatibleWith(this Version version, Version other, int significantParts) =>
        version.CompareTo(other, significantParts) == 0;

    /// <summary>
    /// Compares two versions but only up to the significant parts specified.
    /// </summary>
    /// <param name="version">The version which depends on <paramref name="otherVersion"/> (can include short version e.g. "3.0" will have -1 for Build) </param>
    /// <param name="otherVersion">The full version</param>
    /// <param name="significantParts"></param>
    /// <returns></returns>
    public static int CompareTo(this Version version, Version otherVersion, int significantParts)
    {
        if (version == null) throw new ArgumentNullException(nameof(version));
        if (otherVersion == null) return 1;

        if (version.Major != otherVersion.Major && significantParts >= 1)
            return version.Major > otherVersion.Major ? 1 : -1;

        if (version.Minor != otherVersion.Minor && version.Minor != -1 && significantParts >= 2)
            return version.Minor > otherVersion.Minor ? 1 : -1;

        if (version.Build != otherVersion.Build && version.Build != -1 && significantParts >= 3)
            return version.Build > otherVersion.Build ? 1 : -1;

        if (version.Revision != otherVersion.Revision && version.Revision != -1 && significantParts >= 4)
            return version.Revision > otherVersion.Revision ? 1 : -1;

        return 0;
    }
}