// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// All standard <see cref="ErrorCode"/> which can be exposed by RDMP or plugins
    /// </summary>
    public static class ErrorCodes
    {
        public static ErrorCode ExistingExtractionTableInDatabase = new ErrorCode("R001", "Table {0} already exists in the extraction database {1}", CheckResult.Fail);
        public static ErrorCode ExtractTimeoutChecking = new ErrorCode("R002", "Failed to read rows after {0}s", CheckResult.Warning);

        static ErrorCodes()
        {
            var fields = typeof(ErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Where(p => p.FieldType == typeof(ErrorCode));

            foreach(var field in fields)
            {
                KnownCodes.Add((ErrorCode)field.GetValue(null));
            }
        }

        /// <summary>
        /// Collection of all known error codes.  Plugins are free to add to these if desired but must do so pre startup
        /// </summary>
        public static List<ErrorCode> KnownCodes = new List<ErrorCode>();
    }
}
