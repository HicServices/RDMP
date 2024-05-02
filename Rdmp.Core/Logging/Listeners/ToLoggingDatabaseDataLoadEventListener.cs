// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Logging.Listeners;

/// <summary>
///     Handles transparently all the logging complexity by using the IDataLoadEventListener interface.  Use this interface
///     if you want to log to the
///     logging database events that might otherwise go elsewhere or the component/system you are dealing with already uses
///     IDataLoadEventListeners
/// </summary>
public class ToLoggingDatabaseDataLoadEventListener : IDataLoadEventListener
{
    public Dictionary<string, ITableLoadInfo> TableLoads = new();

    private readonly object _hostingApplication;
    private readonly LogManager _logManager;
    private readonly string _loggingTask;
    private readonly string _runDescription;

    /// <summary>
    ///     true if we were passed an IDataLoadInfo that was created by someone else (in which case we shouldn't just
    ///     arbitrarily close it at any point).
    /// </summary>
    private readonly bool _wasAlreadyOpen;

    /// <summary>
    ///     The root logging object under which all events will be stored, will be null if logging has not started yet (first
    ///     call to OnNotify/StartLogging).
    /// </summary>
    public IDataLoadInfo DataLoadInfo { get; private set; }

    public ToLoggingDatabaseDataLoadEventListener(object hostingApplication, LogManager logManager, string loggingTask,
        string runDescription)
    {
        _hostingApplication = hostingApplication;
        _logManager = logManager;
        _loggingTask = loggingTask;
        _runDescription = runDescription;
    }

    public ToLoggingDatabaseDataLoadEventListener(LogManager logManager, IDataLoadInfo dataLoadInfo)
    {
        DataLoadInfo = dataLoadInfo;
        _logManager = logManager;
        _wasAlreadyOpen = true;
    }

    public virtual void StartLogging()
    {
        _logManager.CreateNewLoggingTaskIfNotExists(_loggingTask);

        DataLoadInfo =
            _logManager.CreateDataLoadInfo(_loggingTask, _hostingApplication.ToString(), _runDescription, "", false);
    }

    private const string RDMPLoggingStringLengthLimit = "RDMP_LOGGING_STRING_LENGTH_LIMIT";
    private static readonly int StrStringLengthLimit;

    static ToLoggingDatabaseDataLoadEventListener()
    {
        StrStringLengthLimit =
            int.TryParse(Environment.GetEnvironmentVariable(RDMPLoggingStringLengthLimit), out var limit)
                ? limit
                : int.MaxValue;
    }

    private static string EnsureMessageAValidLength(string message)
    {
        return StrStringLengthLimit < 4 ? "" :
            message.Length > StrStringLengthLimit ? message[..(StrStringLengthLimit - 3)] + "..." : message;
    }

    public virtual void OnNotify(object sender, NotifyEventArgs e)
    {
        if (DataLoadInfo == null)
            StartLogging();
        if (StrStringLengthLimit < 4) // Logging suppressed
            return;

        switch (e.ProgressEventType)
        {
            case ProgressEventType.Trace:
            case ProgressEventType.Debug:
                break;
            case ProgressEventType.Information:
                DataLoadInfo?.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnInformation, sender.ToString(),
                    EnsureMessageAValidLength(e.Message));
                break;
            case ProgressEventType.Warning:
                var msg = e.Message + (e.Exception == null
                    ? ""
                    : Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true));
                msg = EnsureMessageAValidLength(msg);
                DataLoadInfo?.LogProgress(Logging.DataLoadInfo.ProgressEventType.OnWarning, sender.ToString(), msg);
                break;
            case ProgressEventType.Error:
                var err = e.Message + (e.Exception == null
                    ? ""
                    : Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true));
                err = EnsureMessageAValidLength(err);
                DataLoadInfo?.LogFatalError(sender.ToString(), err);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e));
        }
    }

    public virtual void OnProgress(object sender, ProgressEventArgs e)
    {
        if (DataLoadInfo == null)
            StartLogging();

        Debug.Assert(DataLoadInfo != null, "DataLoadInfo != null");

        if (e.Progress.UnitOfMeasurement != ProgressType.Records) return;

        if (!TableLoads.TryGetValue(e.TaskDescription, out var t))
        {
            t = DataLoadInfo.CreateTableLoadInfo("", e.TaskDescription,
                new[] { new DataSource(sender.ToString()) }, e.Progress.KnownTargetValue);
            TableLoads.Add(e.TaskDescription, t);
        }

        t.Inserts = e.Progress.Value;
    }

    public virtual void FinalizeTableLoadInfos()
    {
        foreach (var tableLoadInfo in TableLoads.Values)
            tableLoadInfo.CloseAndArchive();

        if (!_wasAlreadyOpen)
            DataLoadInfo.CloseAndMarkComplete();
    }
}