// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace ReusableLibraryCode.Extensions
{
    public static class VersionExtensions
    {
        /// <summary>
        /// Returns true if the two versions are idential up to the significant parts specified
        /// </summary>
        /// <param name="version"></param>
        /// <param name="other"></param>
        /// <param name="significantParts"></param>
        /// <returns></returns>
        public static bool IsCompatibleWith(this Version version, Version other, int significantParts)
        {
            return version.CompareTo(other, significantParts) == 0;
        }

        /// <summary>
        /// Compares two versions but only up to the significant parts specified.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="otherVersion"></param>
        /// <param name="significantParts"></param>
        /// <returns></returns>
        public static int CompareTo(this Version version, Version otherVersion, int significantParts)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (otherVersion == null)
            {
                return 1;
            }

            if (version.Major != otherVersion.Major && significantParts >= 1)
                if (version.Major > otherVersion.Major)
                    return 1;
                else
                    return -1;

            if (version.Minor != otherVersion.Minor && significantParts >= 2)
                if (version.Minor > otherVersion.Minor)
                    return 1;
                else
                    return -1;

            if (version.Build != otherVersion.Build && significantParts >= 3)
                if (version.Build > otherVersion.Build)
                    return 1;
                else
                    return -1;

            if (version.Revision != otherVersion.Revision && significantParts >= 4)
                if (version.Revision > otherVersion.Revision)
                    return 1;
                else
                    return -1;

            return 0;
        }
    }
}
