// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rdmp.Core.ReusableLibraryCode.Checks;

/// <summary>
///     All standard <see cref="ErrorCode" /> which can be exposed by RDMP or plugins
/// </summary>
public static class ErrorCodes
{
    public static readonly ErrorCode ExistingExtractionTableInDatabase = new("R001",
        "Table {0} already exists in the extraction database {1}", CheckResult.Fail);

    public static readonly ErrorCode ExtractTimeoutChecking =
        new("R002", "Failed to read rows after {0}s", CheckResult.Warning);

    public static readonly ErrorCode ExtractionProgressColumnProbablyNotADate = new("R003",
        "Data type for column '{0}' is '{1}' which is not a date.  If the SelectSQL is a transform to then this is ok",
        CheckResult.Warning);

    public static readonly ErrorCode ExtractionIsIdentifiable = new("R004",
        "PrivateIdentifierField and ReleaseIdentifierField are the same, this means your cohort will extract identifiable data (no cohort identifier substitution takes place)",
        CheckResult.Fail);

    public static readonly ErrorCode ExtractionContainsSpecialApprovalRequired = new("R005",
        "ExtractionConfiguration '{0}' dataset '{1}' contains SpecialApprovalRequired columns: {2}",
        CheckResult.Warning);

    public static readonly ErrorCode ExtractionContainsInternal = new("R006",
        "ExtractionConfiguration '{0}' dataset '{1}' contains Internal columns: {2}", CheckResult.Warning);

    public static readonly ErrorCode ExtractionContainsDeprecated = new("R007",
        "ExtractionConfiguration '{0}' dataset '{1}' contains Deprecated columns: {2}", CheckResult.Fail);

    public static readonly ErrorCode CouldNotLoadDll =
        new("R008", "Encountered Bad Assembly loading {0} into memory", CheckResult.Success);

    public static readonly ErrorCode CouldOnlyHalfLoadDll =
        new("R009", "Loaded {0}/{1} Types from {2}", CheckResult.Warning);

    public static readonly ErrorCode CohortAndExtractableDatasetsAreOnDifferentServers = new("R010",
        "Cohort is on server '{0}' ({1}) but dataset '{2}' is on '{3}' ({4})", CheckResult.Warning);

    public static readonly ErrorCode CouldNotReachCohort = new("R011",
        "Could not reach cohort '{0}' (it may be slow responding or inaccessible due to user permissions)",
        CheckResult.Warning);

    public static readonly ErrorCode ExtractionFailedToExecuteTop1 =
        new("R012", "Failed to execute Top 1 on dataset '{0}'", CheckResult.Warning);

    public static readonly ErrorCode TextColumnsInExtraction = new("R013",
        "The following columns are data type ntext or text and so may be incompatible with the DISTINCT keyword({0}).  Ensure that PipelineSources are set to use extraction strategy 'OrderByAndDistinctInMemory' (ignore this message if you have already enabled this setting)",
        CheckResult.Warning);

    public static readonly ErrorCode ExtractionInformationMissing = new("R014",
        "The following columns no longer map to an ExtractionInformation(it may have been deleted){0}",
        CheckResult.Warning);

    public static readonly ErrorCode AttemptToReleaseUnfinishedExtractionProgress = new("R015",
        "Dataset {0} should not be released because its ExtractionProgress has a progress date of {1} but an end date of {2}",
        CheckResult.Fail);

    public static readonly ErrorCode NoSqlAuditedForExtractionProgress = new("R0016",
        "ExtractionProgress '{0}' is 'in progress' (ProgressDate is not null) but there is no audit of previously extracted SQL (needed for checking cohort changes)",
        CheckResult.Fail);

    public static readonly ErrorCode CohortSwappedMidExtraction = new("R0017",
        "ExtractionProgress '{0}' is 'in progress' (ProgressDate is not null) but we did not find the expected Cohort WHERE Sql in the audit of SQL extracted with the last batch.  Did you change the cohort without resetting the ProgressDate? The SQL we expected to find was '{1}'",
        CheckResult.Fail);

    static ErrorCodes()
    {
        var fields = typeof(ErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.FieldType == typeof(ErrorCode));

        foreach (var field in fields) KnownCodes.Add((ErrorCode)field.GetValue(null));
    }

    /// <summary>
    ///     Collection of all known error codes.  Plugins are free to add to these if desired but must do so pre startup
    /// </summary>
    public static readonly List<ErrorCode> KnownCodes = new();
}