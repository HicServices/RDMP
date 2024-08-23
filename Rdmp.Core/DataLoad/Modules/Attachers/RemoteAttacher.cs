// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Linq;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Root class to facilitate the Table and Database remote attachers.
/// </summary>
public class RemoteAttacher : Attacher, IPluginAttacher
{

    public RemoteAttacher() : base(true) { }
    [DemandsInitialization("How far back to pull data from")]
    public AttacherHistoricalDurations HistoricalFetchDuration { get; set; }

    [DemandsInitialization("Which column in the remote table can be used to perform time-based data selection")]
    public string RemoteTableDateColumn { get; set; }

    private readonly string RemoteTableDateFormat = "yyyy-MM-dd HH:mm:ss.fff";

    [DemandsInitialization("Earliest date when using a custom fetch duration")]
    public DateTime CustomFetchDurationStartDate { get; set; }

    [DemandsInitialization("Latest date when using a custom fetch duration")]
    public DateTime CustomFetchDurationEndDate { get; set; }

    [DemandsInitialization("The Current Start Date for procedural fetching of historical data")]
    public DateTime DeltaReadingStartDate { get; set; }
    [DemandsInitialization("How many days the procedural fetching should look back ")]
    public int DeltaReadingLookBackDays { get; set; } = 0;
    [DemandsInitialization("How many days the procedural fetching should look forward ")]
    public int DeltaReadingLookForwardDays { get; set; } = 0;

    [DemandsInitialization("If you only want to progress the procedural load to the most recent date seen in the procedural load, not the date + X days, then tick this box")]
    public bool SetDeltaReadingToLatestSeenDatePostLoad { get; set; } = false;

    [DemandsInitialization("Internal Value")]
    public DateTime? MostRecentlySeenDate { get; set; }



    private static string GetCorrectDateAddForDatabaseType(DatabaseType dbType, string addType, string amount)
    {
        switch (dbType)
        {
            case DatabaseType.PostgreSql:
                return $"cast((NOW() + interval '{amount} {addType}S') as Date)";
            case DatabaseType.Oracle:
                if (addType == "DAY") return $"DateAdd(DATE(),,{amount})";
                if (addType == "WEEK") return $"DateAdd(DATE(),,{amount} *7)";
                if (addType == "MONTH") return $"DateAdd(DATE(),,,{amount})";
                if (addType == "YEAR") return $"DateAdd(DATE(),,,,{amount})";
                return $"DateAdd(DATE(),,{amount})";
            case DatabaseType.MicrosoftSQLServer:
                return $"DATEADD({addType}, {amount}, GETDATE())";
            case DatabaseType.MySql:
                return $"DATE_ADD(CURDATE(), INTERVAL {amount} {addType})";
            default:
                throw new InvalidOperationException("Unknown Database Type");
        }
    }

    private string ConvertDateString(DatabaseType dbType, string dateString)
    {
        switch (dbType)
        {
            case DatabaseType.PostgreSql:
                return $"'{dateString}'";
            case DatabaseType.Oracle:
                return $"TO_DATE('{dateString}')";
            case DatabaseType.MicrosoftSQLServer:
                return $"convert(Date,'{dateString}')";
            case DatabaseType.MySql:
                return $"convert('{dateString}',Date)";
            default:
                return $"convert(Date,'{dateString}')";
        }

    }

    public string SqlHistoricalDataFilter(ILoadMetadata loadMetadata, DatabaseType dbType)
    {
        const string dateConvert = "Date";

        switch (HistoricalFetchDuration)
        {
            case AttacherHistoricalDurations.Past24Hours:
                return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {GetCorrectDateAddForDatabaseType(dbType, "DAY", "-1")}";
            case AttacherHistoricalDurations.Past7Days:
                return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {GetCorrectDateAddForDatabaseType(dbType, "WEEK", "-1")}";
            case AttacherHistoricalDurations.PastMonth:
                return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {GetCorrectDateAddForDatabaseType(dbType, "MONTH", "-1")}";
            case AttacherHistoricalDurations.PastYear:
                return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {GetCorrectDateAddForDatabaseType(dbType, "YEAR", "-1")}";
            case AttacherHistoricalDurations.SinceLastUse:
                return loadMetadata.LastLoadTime is not null ? $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {ConvertDateString(dbType, loadMetadata.LastLoadTime.GetValueOrDefault().ToString(RemoteTableDateFormat))}" : "";
            case AttacherHistoricalDurations.Custom:
                if (CustomFetchDurationStartDate == DateTime.MinValue && CustomFetchDurationEndDate != DateTime.MinValue)
                {
                    //end only
                    return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) <= {ConvertDateString(dbType, CustomFetchDurationEndDate.ToString(RemoteTableDateFormat))}";
                }

                if (CustomFetchDurationStartDate != DateTime.MinValue && CustomFetchDurationEndDate == DateTime.MinValue)
                {
                    //start only
                    return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {ConvertDateString(dbType, CustomFetchDurationStartDate.ToString(RemoteTableDateFormat))}";
                }

                if (CustomFetchDurationStartDate == DateTime.MinValue && CustomFetchDurationEndDate == DateTime.MinValue)
                {
                    //No Dates
                    return "";
                }

                return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {ConvertDateString(dbType, CustomFetchDurationStartDate.ToString(RemoteTableDateFormat))} AND CAST({RemoteTableDateColumn} as {dateConvert}) <= {ConvertDateString(dbType, CustomFetchDurationEndDate.ToString(RemoteTableDateFormat))}";
            case AttacherHistoricalDurations.DeltaReading:
                if (DeltaReadingStartDate == DateTime.MinValue) return "";
                var startDate = DeltaReadingStartDate.AddDays(-DeltaReadingLookBackDays);
                var endDate = DeltaReadingStartDate.AddDays(DeltaReadingLookForwardDays);
                return $" WHERE CAST({RemoteTableDateColumn} as {dateConvert}) >= {ConvertDateString(dbType, startDate.ToString(RemoteTableDateFormat))} AND CAST({RemoteTableDateColumn} as {dateConvert}) <= {ConvertDateString(dbType, endDate.ToString(RemoteTableDateFormat))}";
            default:
                return "";
        }
    }



    private bool IsThisRemoteAttacher(IProcessTask task)
    {
        if (task.ProcessTaskType != ProcessTaskType.Attacher) return false;
        try
        {
            if (HistoricalFetchDuration.ToString() != task.ProcessTaskArguments.First(static arg => arg.Name == "HistoricalFetchDuration").Value) return false;
            if (RemoteTableDateColumn.ToString() != task.ProcessTaskArguments.First(static arg => arg.Name == "RemoteTableDateColumn").Value) return false;

            if (CustomFetchDurationStartDate == DateTime.MinValue && task.ProcessTaskArguments.First(static arg => arg.Name == "CustomFetchDurationStartDate").Value != null) return false;
            if (CustomFetchDurationStartDate != DateTime.MinValue && task.ProcessTaskArguments.First(static arg => arg.Name == "CustomFetchDurationStartDate").Value == null) return false;
            if (CustomFetchDurationStartDate != DateTime.MinValue && DateTime.Parse(task.ProcessTaskArguments.First(static arg => arg.Name == "CustomFetchDurationStartDate").Value) != CustomFetchDurationStartDate) return false;

            if (CustomFetchDurationEndDate == DateTime.MinValue && task.ProcessTaskArguments.First(static arg => arg.Name == "CustomFetchDurationStartDate").Value != null) return false;
            if (CustomFetchDurationEndDate != DateTime.MinValue && task.ProcessTaskArguments.First(static arg => arg.Name == "CustomFetchDurationEndDate").Value == null) return false;
            if (CustomFetchDurationEndDate != DateTime.MinValue && DateTime.Parse(task.ProcessTaskArguments.First(static arg => arg.Name == "CustomFetchDurationEndDate").Value) != CustomFetchDurationEndDate) return false;

            if (DeltaReadingStartDate == DateTime.MinValue && task.ProcessTaskArguments.First(static arg => arg.Name == "DeltaReadingStartDate").Value != null) return false;
            if (DeltaReadingStartDate != DateTime.MinValue && task.ProcessTaskArguments.First(static arg => arg.Name == "DeltaReadingStartDate").Value == null) return false;
            if (DeltaReadingStartDate != DateTime.MinValue && DateTime.Parse(task.ProcessTaskArguments.First(static arg => arg.Name == "DeltaReadingStartDate").Value) != DeltaReadingStartDate) return false;

            if (DeltaReadingLookBackDays.ToString() != task.ProcessTaskArguments.First(static arg => arg.Name == "DeltaReadingLookBackDays").Value) return false;
            if (DeltaReadingLookForwardDays.ToString() != task.ProcessTaskArguments.First(static arg => arg.Name == "DeltaReadingLookForwardDays").Value) return false;
            if (SetDeltaReadingToLatestSeenDatePostLoad.ToString() != task.ProcessTaskArguments.First(static arg => arg.Name == "SetDeltaReadingToLatestSeenDatePostLoad").Value) return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public void FindMostRecentDateInLoadedData(IQuerySyntaxHelper syntaxFrom, DatabaseType dbType, string table, IDataLoadJob job)
    {
        var maxDateSql = $"SELECT MAX({RemoteTableDateColumn}) FROM {syntaxFrom.EnsureWrapped(table)} {SqlHistoricalDataFilter(job.LoadMetadata, dbType)}";

        using var con = _dbInfo.Server.GetConnection();
        using var dt = new DataTable();
        using var cmd = _dbInfo.Server.GetCommand(maxDateSql, con);
        cmd.CommandTimeout = 30000;
        using var da = _dbInfo.Server.GetDataAdapter(cmd);
        da.Fill(dt);
        MostRecentlySeenDate = dt.Rows.Count > 0 && dt.Rows[0].ItemArray[0].ToString() != "" ? DateTime.Parse(dt.Rows[0].ItemArray[0].ToString()) : null;
        foreach (var task in job.LoadMetadata.ProcessTasks.Where(IsThisRemoteAttacher).OfType<ProcessTask>())
        {
            task.SetArgumentValue("MostRecentlySeenDate", MostRecentlySeenDate);
            task.SaveToDatabase();
        }
    }

    public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void Check(ICheckNotifier notifier)
    {
        throw new NotImplementedException();
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }
}
