// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
///     load component which can stop an ongoing load early if a given PrematureLoadEndCondition is met with a given
///     ExitCodeType.
///     <para>
///         Conditionally ends the data load early if a given set of circumstances occurs e.g. you might choose to return
///         LoadNotRequired if there are no records in RAW
///     </para>
/// </summary>
public class PrematureLoadEnder : IPluginMutilateDataTables
{
    private DiscoveredDatabase _databaseInfo;

    [DemandsInitialization(
        "An exit code that reflects the nature off the stop.  Do not set Success because loads do not stop halfway through Successfully.  If you do not want an error use LoadNotRequired")]
    public ExitCodeType ExitCodeToReturnIfConditionMet { get; set; }

    [DemandsInitialization(
        "Condition under which to return the exit code.  Use cases for Always are few and far between I guess if you have a big configuration but you want to stop it running ever you could put an Always abort step in")]
    public PrematureLoadEndCondition ConditionsToTerminateUnder { get; set; }

    public void Check(ICheckNotifier notifier)
    {
        if (ExitCodeToReturnIfConditionMet == ExitCodeType.Success)
            notifier.OnCheckPerformed(new CheckEventArgs(
                "You cannot return Success if you are anticipating terminating the load early, you must choose LoadNotRequired or Error",
                CheckResult.Fail));

        if (ConditionsToTerminateUnder == PrematureLoadEndCondition.Always)
            notifier.OnCheckPerformed(new CheckEventArgs(
                "ConditionsToTerminateUnder is Always.  This means that the load will not complete if executed",
                CheckResult.Warning));
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _databaseInfo = dbInfo;
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        switch (ConditionsToTerminateUnder)
        {
            case PrematureLoadEndCondition.Always:
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"ConditionsToTerminateUnder is {ConditionsToTerminateUnder} so terminating load with {ExitCodeToReturnIfConditionMet}"));
                return ExitCodeToReturnIfConditionMet;

            case PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase:
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"About to inspect what tables have rows in them in database {_databaseInfo.GetRuntimeName()}"));

                foreach (var t in _databaseInfo.DiscoverTables(false))
                {
                    var rowCount = t.GetRowCount();

                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"Found table {t.GetRuntimeName()} with row count {rowCount}"));

                    if (rowCount > 0)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                            $"Found at least 1 record in 1 table so condition {ConditionsToTerminateUnder} is not met.  Therefore returning Success so the load can continue normally."));
                        return ExitCodeType.Success;
                    }
                }

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"No tables had any rows in them so returning {ExitCodeToReturnIfConditionMet} which should terminate the load here"));
                return ExitCodeToReturnIfConditionMet;
            }

            case PrematureLoadEndCondition.NoFilesInForLoading:
            {
                var dataLoadJob = job ??
                                  throw new Exception(
                                      $"IDataLoadEventListener {job} was not an IDataLoadJob (very unexpected)");
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"About to check ForLoading directory for files, the directory is:{dataLoadJob.LoadDirectory.ForLoading.FullName}"));

                var files = dataLoadJob.LoadDirectory.ForLoading.GetFiles();

                if (!files.Any())
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"No files in ForLoading so returning {ExitCodeToReturnIfConditionMet} which should terminate the load here"));
                    return ExitCodeToReturnIfConditionMet;
                }

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Found {files.Length} files in ForLoading so not terminating ({string.Join(",", files.Select(f => f.Name))})"));

                //There were
                return ExitCodeType.Success;
            }

            default:
                throw new Exception($"Didn't know how to handle condition:{ConditionsToTerminateUnder}");
        }
    }
}