// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

/// <summary>
/// Pipeline component designed to prevent unwanted data existing within DataTables passing through the pipeline.  The component will crash the entire pipeline
/// if it sees columns which match the forbidlist.  Use cases for this include when the user wants to prevent private identifiers being accidentally released
/// due to system misconfiguration e.g. you might forbidlist all columns containing the strings starting "Patient" on the grounds that they are likely to be
/// identifiable (PatientName, PatientDob etc).
/// 
/// <para>Crashes the pipeline if any column matches the regex e.g. '^(mCHI)|(chi)$'</para>
/// </summary>
public class ColumnForbidder : IPluginDataFlowComponent<DataTable>
{
    private Regex _crashIfAnyColumnMatches;

    [DemandsInitialization("Crashes the load if any column name matches this regex. This option takes precedence over any selected Standard Regex")]
    public Regex CrashIfAnyColumnMatches
    {
        get => _crashIfAnyColumnMatches;
        set => _crashIfAnyColumnMatches = value != null ? new Regex(value.ToString(), value.Options | RegexOptions.IgnoreCase) : null;
    }

    [DemandsInitialization(
        "Alternative to specifying a Regex pattern in CrashIfAnyColumnMatches.  Select an existing StandardRegex.  This has the advantage of centralising the concept.  See StandardRegexUI for configuring StandardRegexes")]
    public StandardRegex StandardRegex { get; set; }

    [DemandsInitialization(
        "Crash message (if any) to explain why columns matching the Regex are a problem e.g. 'Patient telephone numbers should never be extracted!'")]
    public string Rationale { get; set; }

    private Regex _reCache;

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var checkPattern = _reCache ??= _crashIfAnyColumnMatches ?? new Regex(GetPattern(), RegexOptions.IgnoreCase);

        foreach (var c in toProcess.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
            if (checkPattern.IsMatch(c))
                if (string.IsNullOrWhiteSpace(Rationale))
                    throw new Exception($"Column {c} matches forbidlist regex");
                else
                    throw new Exception(
                        $"{Rationale}{Environment.NewLine}Exception generated because Column {c} matches forbidlist regex");

        return toProcess;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        try
        {
            var p = GetPattern();
            _ = new Regex(p);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred getting Regex pattern for forbidlist",
                CheckResult.Fail, e));
        }
    }

    private string GetPattern()
    {
        string pattern = null;

        if (CrashIfAnyColumnMatches != null)
            pattern = CrashIfAnyColumnMatches.ToString();
        else if (StandardRegex != null)
            pattern = StandardRegex.Regex;


        return string.IsNullOrWhiteSpace(pattern)
            ? throw new Exception(
                "You must specify either a pattern in CrashIfAnyColumnMatches or pick an existing StandardRegex with a pattern to match on")
            : pattern;
    }
}