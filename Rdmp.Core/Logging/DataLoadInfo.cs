// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;

namespace Rdmp.Core.Logging;

/// <summary>
/// Root object for an ongoing logged activity e.g. 'Loading Biochemistry'.  Includes the package name (exe or class name that is primarily responsible
/// for the activity), start time, description etc.  You must call CloseAndMarkComplete once your activity is completed (whether it has failed or suceeded).
/// 
/// <para>You should maintain a reference to DataLoadInfo in order to create logs of Progress / Errors / and table load audits
/// (TableLoadInfo) (create these via the LogManager).  The ID property can be used if you want to reference this audit record e.g. when loading a live table
/// you can store the ID of the load batch it appeared in. </para>
/// </summary>
public class DataLoadInfo : IDataLoadInfo
{
        
    private bool _isClosed = false;
    private readonly string _packageName;
    private readonly string _userAccount;
    private readonly DateTime _startTime;
    private DateTime _endTime;
    private readonly string _description;
    private string _suggestedRollbackCommand;
    private int _id;
    private bool _isTest;



    private DiscoveredServer _server ;

    public DiscoveredServer DatabaseSettings => _server;

    private object oLock = new object();
        
        
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
    public DataLoadInfo(string dataLoadTaskName, string packageName, string description,string suggestedRollbackCommand,bool isTest,DiscoveredServer settings)
    {
        if(settings != null)
            _server = settings;

        _packageName = packageName;
        _userAccount = Environment.UserName;
        _description = description;
        _startTime = DateTime.Now;
        _suggestedRollbackCommand = suggestedRollbackCommand;
            
        _id = -1;
        _isTest = isTest;

        RecordNewDataLoadInDatabase(dataLoadTaskName);
    }

    private void RecordNewDataLoadInDatabase(string dataLoadTaskName)
    {
        using (var con = _server.GetConnection())
        {
            con.Open();

            var cmd = _server.GetCommand("SELECT ID FROM DataLoadTask WHERE name=@name", con);
            _server.AddParameterWithValueToCommand("@name",cmd, dataLoadTaskName);


            var result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                throw new Exception($"Could not find data load task named:{dataLoadTaskName}");

            //ID can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
            var parentTaskID = int.Parse(result.ToString());
                                
            cmd = _server.GetCommand(
                @"INSERT INTO DataLoadRun (description,startTime,dataLoadTaskID,isTest,packageName,userAccount,suggestedRollbackCommand) VALUES (@description,@startTime,@dataLoadTaskID,@isTest,@packageName,@userAccount,@suggestedRollbackCommand);
SELECT @@IDENTITY;", con);

            _server.AddParameterWithValueToCommand("@description", cmd, _description);
            _server.AddParameterWithValueToCommand("@startTime", cmd, _startTime);
            _server.AddParameterWithValueToCommand("@dataLoadTaskID", cmd, parentTaskID);
            _server.AddParameterWithValueToCommand("@isTest",cmd, _isTest);
            _server.AddParameterWithValueToCommand("@packageName", cmd, _packageName);
            _server.AddParameterWithValueToCommand("@userAccount", cmd, _userAccount);
            _server.AddParameterWithValueToCommand("@suggestedRollbackCommand", cmd, _suggestedRollbackCommand ?? string.Empty);


            //ID can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
            _id = int.Parse(cmd.ExecuteScalar().ToString());
        }

    }

    /// <summary>
    /// Marks that the data load ended
    /// </summary>
    public void CloseAndMarkComplete()
    {
        lock (oLock)
        {
            //prevent double closing
            if (_isClosed)
                return; 
            
            _endTime = DateTime.Now;

            using (var con = _server.BeginNewTransactedConnection())
            {
                try
                {

                    var cmdUpdateToClosed =
                        _server.GetCommand("UPDATE DataLoadRun SET endTime=@endTime WHERE ID=@ID",
                            con);

                    _server.AddParameterWithValueToCommand("@endTime", cmdUpdateToClosed,DateTime.Now);
                    _server.AddParameterWithValueToCommand("@ID", cmdUpdateToClosed, ID);
                        
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

                //once a record has been commited to the database it is redundant and no further attempts to read/change it should be made by anyone
                foreach (var t in TableLoads.Values)
                {
                    //close any table loads that have not yet completed
                    if (!t.IsClosed)
                        t.CloseAndArchive();
                }
            }
        }
    }


    private Dictionary<int, TableLoadInfo> _TableLoads = new Dictionary<int, TableLoadInfo>();

    public Dictionary<int, TableLoadInfo> TableLoads => _TableLoads;

    public ITableLoadInfo CreateTableLoadInfo(string suggestedRollbackCommand, string destinationTable, DataSource[] sources, int expectedInserts)
    {
        return new TableLoadInfo(this, suggestedRollbackCommand, destinationTable, sources, expectedInserts);
    }

    public static DataLoadInfo Empty= new DataLoadInfo();

    private DataLoadInfo()
    {
    }

    public void AddTableLoad(TableLoadInfo tableLoadInfo)
    {
        lock (oLock)
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
        using (var con = DatabaseSettings.GetConnection())
        {
            con.Open();

            //look up the fatal error ID (get hte name of the Enum so that we can refactor if nessesary without breaking the code looking for a constant string)
            var initialErrorStatus = Enum.GetName(typeof(FatalErrorStates), FatalErrorStates.Outstanding);

                
            var cmdLookupStatusID = _server.GetCommand("SELECT ID from z_FatalErrorStatus WHERE status=@status", con);
            _server.AddParameterWithValueToCommand("@status",cmdLookupStatusID, initialErrorStatus);

            var statusID = int.Parse(cmdLookupStatusID.ExecuteScalar().ToString());

            var cmdRecordFatalError = _server.GetCommand(
                @"INSERT INTO FatalError (time,source,description,statusID,dataLoadRunID) VALUES (@time,@source,@description,@statusID,@dataLoadRunID);", con);
            _server.AddParameterWithValueToCommand("@time", cmdRecordFatalError, DateTime.Now);
            _server.AddParameterWithValueToCommand("@source", cmdRecordFatalError, errorSource);
            _server.AddParameterWithValueToCommand("@description", cmdRecordFatalError, errorDescription);
            _server.AddParameterWithValueToCommand("@statusID", cmdRecordFatalError, statusID);
            _server.AddParameterWithValueToCommand("@dataLoadRunID", cmdRecordFatalError, ID);

            cmdRecordFatalError.ExecuteNonQuery();

            //this might get called multiple times (many errors in rapid succession as the program crashes) but only close the dataLoadInfo once
            if (!IsClosed)
                CloseAndMarkComplete();

        }
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
        using (var con = DatabaseSettings.GetConnection())
        using (var cmdRecordProgress = _server.GetCommand("INSERT INTO ProgressLog " +
                                                          "(dataLoadRunID,eventType,source,description,time) " +
                                                          "VALUES (@dataLoadRunID,@eventType,@source,@description,@time);", con))
        {
            con.Open();

            _server.AddParameterWithValueToCommand("@dataLoadRunID",cmdRecordProgress, ID);
            _server.AddParameterWithValueToCommand("@eventType", cmdRecordProgress, pevent.ToString());
            _server.AddParameterWithValueToCommand("@source", cmdRecordProgress, Source);
            _server.AddParameterWithValueToCommand("@description", cmdRecordProgress, Description);
            _server.AddParameterWithValueToCommand("@time", cmdRecordProgress, DateTime.Now);

            cmdRecordProgress.ExecuteNonQuery();
        }
    }
}