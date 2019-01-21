using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
