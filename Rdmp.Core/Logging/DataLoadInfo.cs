// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;

namespace Rdmp.Core.Logging;

/// <summary>
/// Root object for an ongoing logged activity e.g. 'Loading Biochemistry'.  Includes the package name (exe or class name that is primarily responsible
/// for the activity), start time, description etc.  You must call CloseAndMarkComplete once your activity is completed (whether it has failed or succeeded).
///
/// <para>You should maintain a reference to DataLoadInfo in order to create logs of Progress / Errors / and table load audits
/// (TableLoadInfo) (create these via the LogManager).  The ID property can be used if you want to reference this audit record e.g. when loading a live table
/// you can store the ID of the load batch it appeared in. </para>
/// </summary>
public sealed class DataLoadInfo : IDataLoadInfo
{
    private bool _isClosed = false;
    private readonly string _packageName;
    private readonly string _userAccount;
    private readonly DateTime _startTime;
    private DateTime _endTime;
    private readonly string _description;
    private readonly string _suggestedRollbackCommand;
    private int _id;
    private readonly bool _isTest;


    public DiscoveredServer DatabaseSettings { get; }

    private readonly Lock _oLock = new();
    private Thread _logThread = null;
    private BlockingCollection<LogEntry> _logQueue = null;
    private readonly object _logWaiter = new();


    #region Property setup (these throw exceptions if you try to read them after the record is closed)

    public string PackageName => _packageName;


    public string UserAccount => _userAccount;


    public DateTime StartTime => _startTime;


    public DateTime EndTime => _endTime;


    public string Description => _description;


    public string SuggestedRollbackCommand => _suggestedRollbackCommand;


    public int ID => _id;

    public bool IsTest => _isTest;


    public bool IsClosed => _isClosed;

    #endregion

    public record LogEntry(string EventType, string Description, string Source, DateTime Time);

    /// <summary>
    /// Marks the start of a new data load in the database.  Automatically populates StartTime and UserAccount from
    /// Environment.  Also creates a new ID in the database.
    /// </summary>
    /// <param name="dataLoadTaskName"></param>
    /// <param name="packageName">The SSIS package or executable that started the data load</param>
    /// <param name="description">A description of what the data load is trying to achieve</param>
    /// <param name="suggestedRollbackCommand"></param>
    /// <param name="isTest">If true then the database record will be marked as Test=1</param>
    /// <param name="settings"></param>
    public DataLoadInfo(string dataLoadTaskName, string packageName, string description,
        string suggestedRollbackCommand, bool isTest, DiscoveredServer settings)
    {
        if (settings != null)
            DatabaseSettings = settings;

        _packageName = packageName;
        _userAccount = Environment.UserName;
        _description = description;
        _startTime = DateTime.Now;
        _suggestedRollbackCommand = suggestedRollbackCommand;

        _id = -1;
        _isTest = isTest;

        RecordNewDataLoadInDatabase(dataLoadTaskName);
        LogInit();
    }

    private void LogInit()
    {
        lock (_oLock)
        {
            // This can happen if we get called twice in quick succession: first call succeeds, so second becomes a no-op
            if (_logQueue?.IsAddingCompleted == false)
                return;

            _logQueue?.CompleteAdding();
            _logThread?.Join();
            _logQueue = new BlockingCollection<LogEntry>();
            _logThread = new Thread(LogWorker);
            _logThread.Start();
        }
    }

    private void LogWorker()
    {
        using var l = _logQueue;
        while (!l.IsCompleted)
        {
            if (l.Count == 0)
            {
                lock (_logWaiter)
                    Monitor.Wait(_logWaiter, 1000);
                continue;
            }
            using var con = DatabaseSettings.BeginNewTransactedConnection();
            if (DatabaseSettings.DatabaseType == DatabaseType.MicrosoftSQLServer)
                using (var lockQuery = DatabaseSettings.GetCommand("SELECT TOP 1 * FROM ProgressLog WITH (HOLDLOCK, UPDLOCK, TABLOCKX) WHERE 1=0", con))
                    lockQuery.ExecuteNonQuery();
            using var cmdRecordProgress = DatabaseSettings.GetCommand(
                "INSERT INTO ProgressLog (dataLoadRunID,eventType,source,description,time) VALUES (@dataLoadRunID,LEFT(RTRIM(@eventType),50),LEFT(RTRIM(@source),100),LEFT(RTRIM(@description),8000),@time);",
                con);

            DatabaseSettings.AddParameterWithValueToCommand("@dataLoadRunID", cmdRecordProgress, ID);

            var type = DatabaseSettings.AddParameterWithValueToCommand("@eventType", cmdRecordProgress, null);
            var source = DatabaseSettings.AddParameterWithValueToCommand("@source", cmdRecordProgress, null);
            var desc = DatabaseSettings.AddParameterWithValueToCommand("@description", cmdRecordProgress, null);
            var time = DatabaseSettings.AddParameterWithValueToCommand("@time", cmdRecordProgress, null);
            while (l.TryTake(out var entry))
            {
                type.Value = entry.EventType;
                source.Value = entry.Source;
                desc.Value = entry.Description;
                time.Value = entry.Time;
                cmdRecordProgress.ExecuteNonQuery();
            }
            con.ManagedTransaction.CommitAndCloseConnection();
        }
        _logQueue = null;
    }

    private void RecordNewDataLoadInDatabase(string dataLoadTaskName)
    {
        using var con = DatabaseSettings.GetConnection();
        con.Open();

        var cmd = DatabaseSettings.GetCommand("SELECT ID FROM DataLoadTask WHERE name=@name", con);
        DatabaseSettings.AddParameterWithValueToCommand("@name", cmd, dataLoadTaskName);


        var result = cmd.ExecuteScalar();

        if (result == null || result == DBNull.Value)
            throw new Exception($"Could not find data load task named:{dataLoadTaskName}");

        //ID can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
        var parentTaskID = int.Parse(result.ToString());

        cmd = DatabaseSettings.GetCommand(
            """
            INSERT INTO DataLoadRun (description,startTime,dataLoadTaskID,isTest,packageName,userAccount,suggestedRollbackCommand) VALUES (@description,@startTime,@dataLoadTaskID,@isTest,@packageName,@userAccount,@suggestedRollbackCommand);
            SELECT @@IDENTITY;
            """, con);

        DatabaseSettings.AddParameterWithValueToCommand("@description", cmd, _description);
        DatabaseSettings.AddParameterWithValueToCommand("@startTime", cmd, _startTime);
        DatabaseSettings.AddParameterWithValueToCommand("@dataLoadTaskID", cmd, parentTaskID);
        DatabaseSettings.AddParameterWithValueToCommand("@isTest", cmd, _isTest);
        DatabaseSettings.AddParameterWithValueToCommand("@packageName", cmd, _packageName.Substring(Math.Max(0, _packageName.Length - 750)));
        DatabaseSettings.AddParameterWithValueToCommand("@userAccount", cmd, _userAccount.Substring(Math.Max(0, _packageName.Length - 500)));
        DatabaseSettings.AddParameterWithValueToCommand("@suggestedRollbackCommand", cmd,
            _suggestedRollbackCommand ?? string.Empty);


        //ID can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
        _id = int.Parse(cmd.ExecuteScalar().ToString());
    }

    /// <summary>
    /// Marks that the data load ended
    /// </summary>
    public void CloseAndMarkComplete()
    {
        lock (_oLock)
        {
            if (_logQueue?.IsAddingCompleted == false)
                _logQueue.CompleteAdding();
            _logThread?.Join();

            //prevent double closing - but only after flushing log entries
            if (_isClosed)
                return;

            _endTime = DateTime.Now;

            using var con = DatabaseSettings.BeginNewTransactedConnection();
            try
            {
                var cmdUpdateToClosed =
                    DatabaseSettings.GetCommand("UPDATE DataLoadRun SET endTime=@endTime WHERE ID=@ID",
                        con);

                DatabaseSettings.AddParameterWithValueToCommand("@endTime", cmdUpdateToClosed, DateTime.Now);
                DatabaseSettings.AddParameterWithValueToCommand("@ID", cmdUpdateToClosed, ID);

                var rowsAffected = cmdUpdateToClosed.ExecuteNonQuery();

                if (rowsAffected != 1)
                    throw new Exception(
                        $"Error closing off DataLoad in database, the update command resulted in {rowsAffected} rows being affected (expected 1) - will try to rollback");

                con.ManagedTransaction.CommitAndCloseConnection();

                _isClosed = true;
            }
            catch (Exception)
            {
                //if something goes wrong with the update, roll it back
                con.ManagedTransaction.AbandonAndCloseConnection();

                throw;
            }

            //once a record has been committed to the database it is redundant and no further attempts to read/change it should be made by anyone
            foreach (var t in TableLoads.Values)
                //close any table loads that have not yet completed
                if (!t.IsClosed)
                    t.CloseAndArchive();
        }
    }


    private Dictionary<int, TableLoadInfo> _TableLoads = new();

    public Dictionary<int, TableLoadInfo> TableLoads => _TableLoads;

    public ITableLoadInfo CreateTableLoadInfo(string suggestedRollbackCommand, string destinationTable,
        DataSource[] sources, int expectedInserts) => new TableLoadInfo(this, suggestedRollbackCommand,
        destinationTable, sources, expectedInserts);

    public static DataLoadInfo Empty = new();

    private DataLoadInfo()
    {
    }

    public void AddTableLoad(TableLoadInfo tableLoadInfo)
    {
        lock (_oLock)
        {
            TableLoads.Add(tableLoadInfo.ID, tableLoadInfo);
        }
    }

    public enum FatalErrorStates
    {
        Outstanding = 1,
        Resolved = 2,
        Blocked = 3
    }

    /// <summary>
    /// Terminates the current DataLoadInfo and records that it resulted in a fatal error
    /// </summary>
    /// <param name="errorSource">The component that generated the failure(in SSIS try System::SourceName)</param>
    /// <param name="errorDescription">A description of the error (in SSIS try System::ErrorDescription)</param>
    public void LogFatalError(string errorSource, string errorDescription)
    {
        using var con = DatabaseSettings.GetConnection();
        con.Open();

        //look up the fatal error ID (get the name of the Enum so that we can refactor if necessary without breaking the code looking for a constant string)
        var initialErrorStatus = Enum.GetName(typeof(FatalErrorStates), FatalErrorStates.Outstanding);


        var cmdLookupStatusID = DatabaseSettings.GetCommand("SELECT ID from z_FatalErrorStatus WHERE status=@status", con);
        DatabaseSettings.AddParameterWithValueToCommand("@status", cmdLookupStatusID, initialErrorStatus);

        var statusID = int.Parse(cmdLookupStatusID.ExecuteScalar().ToString());

        var cmdRecordFatalError = DatabaseSettings.GetCommand(
            @"INSERT INTO FatalError (time,source,description,statusID,dataLoadRunID) VALUES (@time,@source,@description,@statusID,@dataLoadRunID);",
            con);
        DatabaseSettings.AddParameterWithValueToCommand("@time", cmdRecordFatalError, DateTime.Now);
        DatabaseSettings.AddParameterWithValueToCommand("@source", cmdRecordFatalError, errorSource);
        DatabaseSettings.AddParameterWithValueToCommand("@description", cmdRecordFatalError, errorDescription);
        DatabaseSettings.AddParameterWithValueToCommand("@statusID", cmdRecordFatalError, statusID);
        DatabaseSettings.AddParameterWithValueToCommand("@dataLoadRunID", cmdRecordFatalError, ID);

        cmdRecordFatalError.ExecuteNonQuery();

        //this might get called multiple times (many errors in rapid succession as the program crashes) but only close the dataLoadInfo once
        if (!IsClosed)
            CloseAndMarkComplete();
    }

    public enum ProgressEventType
    {
        OnInformation,
        OnProgress,
        OnQueryCancel,
        OnTaskFailed,
        OnWarning
    }

    public void LogProgress(ProgressEventType pevent, string Source, string Description)
    {
        lock (_oLock)
        {
            if (_logQueue is null || _logQueue.IsAddingCompleted)
                LogInit();
            if (_logQueue is null)
                throw new InvalidOperationException("LogInit failed to create new worker");

            _logQueue.Add(new LogEntry(pevent.ToString(), Description, Source, DateTime.Now));
        }
        lock (_logWaiter)
            Monitor.Pulse(_logWaiter);
    }
}
