// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Logging.PastEvents;

/// <summary>
/// Readonly historical version of DataLoadInfo.  The central hierarchical RDMP logging database records activites across all areas of the program in a central
/// place.  You can process these records programmatically via LogManager.  This class contains public properties for each of the sub concepts (Errors, Progress
/// messages, Tables loaded etc).  See Logging.cd for more information
/// </summary>
public class ArchivalDataLoadInfo : IArchivalLoggingRecordOfPastEvent, IComparable
{
    private readonly DiscoveredDatabase _loggingDatabase;

    public int ID { get; private set; }
    public int DataLoadTaskID { get; set; }
    public const int MaxDescriptionLength = 300;

    public DateTime StartTime { get; internal set; }
    public DateTime? EndTime { get; internal set; }

    public bool HasErrors => _knownErrors.Value.Any();

    public string ToShortString()
    {
        var s = ToString();
        return s.Length > MaxDescriptionLength ? $"{s[..MaxDescriptionLength]}..." : s;
    }

    public override string ToString()
    {
        var elapsed = "";
        if (EndTime != null)
        {
            var ts = EndTime.Value.Subtract(StartTime);
            elapsed = $" ({ts.TotalHours:N0}:{ts.Minutes:D2}:{ts.Seconds:D2})";
        }

        return
            $"{Description}(ID={ID}) - {StartTime} - {(EndTime != null ? EndTime.ToString() : "<DidNotFinish>")}{elapsed}";
    }


    /// <summary>
    /// All tables loaded during the run
    /// </summary>
    public List<ArchivalTableLoadInfo> TableLoadInfos => _knownTableInfos.Value;

    /// <summary>
    /// All errors that occurred during the run
    /// </summary>
    public List<ArchivalFatalError> Errors => _knownErrors.Value;

    /// <summary>
    /// All progress messages recorded during the run
    /// </summary>
    public List<ArchivalProgressLog> Progress => _knownProgress.Value;

    private readonly Lazy<List<ArchivalTableLoadInfo>> _knownTableInfos;
    private readonly Lazy<List<ArchivalFatalError>> _knownErrors;
    private readonly Lazy<List<ArchivalProgressLog>> _knownProgress;

    public string Description { get; set; }

    /// <summary>
    /// Creates a blank unknown instance not associated with a logging database
    /// Use this constructor for testing only.
    /// </summary>
    internal ArchivalDataLoadInfo()
    {
    }

    internal ArchivalDataLoadInfo(DbDataReader r, DiscoveredDatabase loggingDatabase)
    {
        _loggingDatabase = loggingDatabase;
        ID = Convert.ToInt32(r["ID"]);
        DataLoadTaskID = Convert.ToInt32(r["dataLoadTaskID"]);

        //populate basic facts from the table
        StartTime = (DateTime)r["startTime"];
        if (r["endTime"] == null || r["endTime"] == DBNull.Value)
            EndTime = null;
        else
            EndTime = Convert.ToDateTime(r["endTime"]);

        Description = r["description"] as string;

        _knownTableInfos = new Lazy<List<ArchivalTableLoadInfo>>(GetTableInfos);
        _knownErrors = new Lazy<List<ArchivalFatalError>>(GetErrors);
        _knownProgress = new Lazy<List<ArchivalProgressLog>>(GetProgress);
    }


    public int CompareTo(object obj)
    {
        if (obj is ArchivalDataLoadInfo other)
            return StartTime == other.StartTime ? 0 : StartTime > other.StartTime ? 1 : -1;

        return string.Compare(ToString(), obj.ToString(), StringComparison.Ordinal);
    }

    private List<ArchivalTableLoadInfo> GetTableInfos()
    {
        var toReturn = new List<ArchivalTableLoadInfo>();

        using var con = _loggingDatabase.Server.GetConnection();
        con.Open();

        using var cmd = _loggingDatabase.Server.GetCommand($"SELECT * FROM TableLoadRun WHERE dataLoadRunID={ID}", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var audit = new ArchivalTableLoadInfo(this, r, _loggingDatabase);

            if ((audit.Inserts ?? 0) <= 0 && (audit.Updates ?? 0) <= 0 && (audit.Deletes ?? 0) <= 0 &&
                UserSettings.HideEmptyTableLoadRunAudits) continue;

            toReturn.Add(audit);
        }

        return toReturn;
    }

    private List<ArchivalProgressLog> GetProgress()
    {
        var toReturn = new List<ArchivalProgressLog>();

        using var con = _loggingDatabase.Server.GetConnection();
        con.Open();

        using var cmd = _loggingDatabase.Server.GetCommand($"SELECT * FROM ProgressLog WHERE dataLoadRunID={ID}", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            toReturn.Add(new ArchivalProgressLog(r));

        return toReturn;
    }

    private List<ArchivalFatalError> GetErrors()
    {
        var toReturn = new List<ArchivalFatalError>();

        using var con = _loggingDatabase.Server.GetConnection();
        con.Open();

        using var cmd = _loggingDatabase.Server.GetCommand($"SELECT * FROM FatalError WHERE dataLoadRunID={ID}", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            toReturn.Add(new ArchivalFatalError(r));

        return toReturn;
    }
}