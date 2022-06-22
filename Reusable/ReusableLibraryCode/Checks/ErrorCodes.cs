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
        public static ErrorCode ExtractionProgressColumnProbablyNotADate = new ErrorCode("R003","Data type for column '{0}' is '{1}' which is not a date.  If the SelectSQL is a transform to then this is ok",CheckResult.Warning);
        public static ErrorCode ExtractionIsIdentifiable = new ErrorCode("R004", "PrivateIdentifierField and ReleaseIdentifierField are the same, this means your cohort will extract identifiable data (no cohort identifier substitution takes place)", CheckResult.Fail);

        public static ErrorCode ExtractionContainsSpecialApprovalRequired = new ErrorCode("R005", "ExtractionConfiguration '{0}' dataset '{1}' contains SpecialApprovalRequired columns: {2}", CheckResult.Warning);
        public static ErrorCode ExtractionContainsInternal = new ErrorCode("R006", "ExtractionConfiguration '{0}' dataset '{1}' contains Internal columns: {2}", CheckResult.Warning);
        public static ErrorCode ExtractionContainsDeprecated = new ErrorCode("R007", "ExtractionConfiguration '{0}' dataset '{1}' contains Deprecated columns: {2}", CheckResult.Fail);
        
        public static ErrorCode CouldNotLoadDll = new ErrorCode("R008", "Encountered Bad Assembly loading {0} into memory", CheckResult.Success);
        public static ErrorCode CouldOnlyHalfLoadDll = new ErrorCode("R009", "Loaded {0}/{1} Types from {2}", CheckResult.Success);
        public static ErrorCode CohortAndExtractableDatasetsAreOnDifferentServers = new ErrorCode("R010","Cohort is on server '{0}' ({1}) but dataset '{2}' is on '{3}' ({4})", CheckResult.Warning);
        public static ErrorCode CouldNotReachCohort = new ErrorCode("R011", "Could not reach cohort '{0}' (it may be slow responding or inaccessible due to user permissions)",CheckResult.Warning);
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
