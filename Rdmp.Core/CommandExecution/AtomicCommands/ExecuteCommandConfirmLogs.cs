// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using TypeGuesser.Deciders;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Checks the RDMP logs for the latest log entry of a given object.  Throws (returns exit code non zero) if
///     the top log entry is failing or if there are no log entries within the expected time span.
/// </summary>
public class ExecuteCommandConfirmLogs : BasicCommandExecution
{
    /// <summary>
    ///     Optional time period in which to expect successful logs
    /// </summary>
    private TimeSpan? WithinTime { get; }

    /// <summary>
    ///     The object which generates logs that you want to check
    /// </summary>
    public ILoggedActivityRootObject LogRootObject { get; }

    /// <summary>
    ///     If <see cref="LogRootObject" /> is a <see cref="LoadMetadata" /> then
    ///     setting this to true requires that rows were updated/inserted into
    ///     the live tables for the command to pass
    /// </summary>
    public bool RequireLoadedRows { get; }

    /// <summary>
    ///     Checks the RDMP logs for the latest log entry of a given object.  Throws (returns exit code non zero) if
    ///     the top log entry is failing or if there are no log entries within the expected time span.
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="obj"></param>
    /// <param name="withinTime"></param>
    /// <param name="requireLoadedRows"></param>
    public ExecuteCommandConfirmLogs(IBasicActivateItems activator,
        [DemandsInitialization("The object you want to confirm passing log entries for")]
        ILoggedActivityRootObject obj,
        [DemandsInitialization("Optional time period in which to expect successful logs e.g. 24:00:00 (24 hours)")]
        string withinTime = null,
        [DemandsInitialization("Optional.  Pass true to require rows to be loaded if obj is a LoadMetadata")]
        bool requireLoadedRows = false) : base(activator)
    {
        LogRootObject = obj;
        RequireLoadedRows = requireLoadedRows;
        if (withinTime != null)
        {
            var decider = new TimeSpanTypeDecider(CultureInfo.CurrentCulture);
            WithinTime = (TimeSpan)decider.Parse(withinTime);
        }
    }

    public override void Execute()
    {
        base.Execute();

        var logManager = new LogManager(LogRootObject.GetDistinctLoggingDatabase());

        // run first without additional restrictions so that
        // the user gets the most high level fail conditions
        // e.g. no runs or latest run was a failure etc
        ThrowIfNoEntries(logManager, false);

        if (LogRootObject is ILoadMetadata && RequireLoadedRows)
            // run again but only consider loads where rows were loaded
            ThrowIfNoEntries(logManager, true);
    }

    private void ThrowIfNoEntries(LogManager logManager, bool checkInclusionCriteria)
    {
        // get the latest log entry
        var unfilteredResults = logManager.GetArchivalDataLoadInfos(LogRootObject.GetDistinctLoggingTask());
        var latest = LogRootObject
            .FilterRuns(unfilteredResults)
            .FirstOrDefault(a => !checkInclusionCriteria || Include(a));

        var messageClarification = checkInclusionCriteria ? " (where rows were loaded)" : "";

        // if no logs
        if (latest == null)
            throw new LogsNotConfirmedException($"There are no log entries for {LogRootObject}{messageClarification}");

        // we have logs but are they in the time period we are interested in
        if (WithinTime.HasValue)
        {
            var thresholdDate = DateTime.Now.Subtract(WithinTime.Value);
            var startTime = latest.StartTime;

            // if the latest log entry is older than the time period the user indicated
            if (startTime < thresholdDate)
                throw new LogsNotConfirmedException(
                    $"Latest logged activity for {LogRootObject}{messageClarification} is {startTime}.  This is older than the requested date threshold:{thresholdDate}");
        }

        // we have an acceptably recent log entry
        if (latest.HasErrors)
            throw new LogsNotConfirmedException(
                $"Latest logs for {LogRootObject}{messageClarification} ({latest.StartTime}) indicate that it failed");

        // most recent log entry did not complete
        if (!latest.EndTime.HasValue)
            throw new LogsNotConfirmedException(
                $"Latest logs for {LogRootObject}{messageClarification} ({latest.StartTime}) indicate that it did not complete");
        // latest log entry is passing yay!
    }

    /// <summary>
    ///     Returns false if <paramref name="arg" /> is an audit for <see cref="LogRootObject" />
    ///     as a DLE load (<see cref="LoadMetadata" />) and <see cref="RequireLoadedRows" /> is set
    ///     and no rows were reported as loaded into the final tables of the load
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private bool Include(ArchivalDataLoadInfo arg)
    {
        if (!RequireLoadedRows || LogRootObject is not ILoadMetadata lmd)
            return true;

        var liveTableNames = lmd.GetAllCatalogues()
            .SelectMany(c => c.GetTableInfoList(true))
            .Select(t => new Regex($"\\b{Regex.Escape(t.GetRuntimeName())}\\b"))
            .Distinct();

        // tables where there was at least 1 insert or update and it wasn't in a RAW or STAGING table
        var loadedTables = arg.TableLoadInfos
            .Where(l => l.Inserts > 0 || l.Updates > 0)
            .Where(l => !l.TargetTable.Contains("_RAW", StringComparison.Ordinal) &&
                        !l.TargetTable.Contains("_STAGING", StringComparison.Ordinal));

        // of those they must match the live table name
        return loadedTables.Any(l =>
            liveTableNames.Any(n => n.IsMatch(l.TargetTable)));
    }
}