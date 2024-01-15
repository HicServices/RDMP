// Copyright (c) The University of Dundee 2023-2023
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
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Data;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Root class to facilitate the Tabe and Database remote attachers.
/// </summary>
public class RemoteAttacher : Attacher, IPluginAttacher
{

    public RemoteAttacher() : base(true) { }
    [DemandsInitialization("How far back to pull data from")]
    public AttacherHistoricalDurations HistoricalFetchDuration { get; set; }

    [DemandsInitialization("Which column in the remote table can be used to perform time-based data selection")]
    public string RemoteTableDateColumn { get; set; }

    [DemandsInitialization("Earliest date when using a custom fetch duration")]
    public DateTime CustomFetchDurationStartDate { get; set; }

    [DemandsInitialization("Latest date when using a custom fetch duration")]
    public DateTime CustomFetchDurationEndDate { get; set; }

    [DemandsInitialization("The Current Start Date for procedural fethching of historical data")]
    public DateTime ForwardScanDateInTime { get; set; }
    [DemandsInitialization("How many days the precedural fetching should look back ")]
    public int ForwardScanLookBackDays { get; set; } = 0;
    [DemandsInitialization("How many days the precedural fetching should look forward ")]
    public int ForwardScanLookForwardDays { get; set; } = 0;

    [DemandsInitialization("If you only want to progress the procedural load to the most recent date seen in the procedural load, not the date + X days, then tick this box")]
    public bool SetForwardScanToLatestSeenDatePostLoad { get; set; } = false;

    public DateTime? MostRecentlySeenDate { get; set; }



    private static string GetCorrectDateAddForDatabaseType(DatabaseType dbType,string addType, string amount)
    {
        switch (dbType)
        {
            case DatabaseType.PostgreSql:
                return $"DATEADD({addType},{amount}, NOW())";
            case DatabaseType.Oracle:
                if (addType == "DAY") return $"DateAdd(DATE(),,{amount})";
                if (addType == "WEEK") return $"DateAdd(DATE(),,{amount} *7)"; //todo does this work?
                if (addType == "MONTH") return $"DateAdd(DATE(),,,{amount})";
                if (addType == "YEAR") return $"DateAdd(DATE(),,,,{amount})";
                return $"DateAdd(DATE(),,{amount})";
            case DatabaseType.MicrosoftSQLServer:
                return $"DATEADD({addType}, {amount}, GETDATE())";
            case DatabaseType.MySql:
                return $"DATE_ADD(CURDATE(), INTERVAL {amount} {addType})";
            default:
                return $"DATEADD({addType}, {amount}, GETDATE())";
        }
    }

    public string SqlHistoricalDataFilter(ILoadMetadata loadMetadata, IQuerySyntaxHelper syntaxHelper)
    {
        switch (HistoricalFetchDuration)
        {
            case AttacherHistoricalDurations.Past24Hours:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > {GetCorrectDateAddForDatabaseType(syntaxHelper.DatabaseType,"DAY","-1")}";
            case AttacherHistoricalDurations.Past7Days:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > {GetCorrectDateAddForDatabaseType(syntaxHelper.DatabaseType, "WEEK", "-1")}";
            case AttacherHistoricalDurations.PastMonth:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > {GetCorrectDateAddForDatabaseType(syntaxHelper.DatabaseType, "MONTH", "-1")}";
            case AttacherHistoricalDurations.PastYear:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > {GetCorrectDateAddForDatabaseType(syntaxHelper.DatabaseType, "YEAR", "-1")}";
            case AttacherHistoricalDurations.SinceLastUse:
                if (loadMetadata.LastLoadTime is not null) return $" WHERE CAST({RemoteTableDateColumn} as Date) > CAST('{loadMetadata.LastLoadTime}' as Date)";
                return "";
            case AttacherHistoricalDurations.Custom:
                if(CustomFetchDurationStartDate == DateTime.MinValue && CustomFetchDurationEndDate != DateTime.MinValue)
                {
                    //end only
                    return $" WHERE CAST({RemoteTableDateColumn} as Date) <= CAST('{CustomFetchDurationEndDate}' as Date)";

                }
                if (CustomFetchDurationStartDate != DateTime.MinValue && CustomFetchDurationEndDate == DateTime.MinValue)
                {
                    //start only
                    return $" WHERE CAST({RemoteTableDateColumn} as Date) >= CAST('{CustomFetchDurationStartDate}' as Date)";

                }
                if (CustomFetchDurationStartDate == DateTime.MinValue && CustomFetchDurationEndDate == DateTime.MinValue)
                {
                    //No Dates
                    return "";
                }
                return $" WHERE CAST({RemoteTableDateColumn} as Date) >= CAST('{CustomFetchDurationStartDate}' as Date) AND CAST({RemoteTableDateColumn} as Date) <= CAST('{CustomFetchDurationEndDate}' as Date)";
            case AttacherHistoricalDurations.ForwardScan:
                if (ForwardScanDateInTime == DateTime.MinValue) return "";
                var startDate = ForwardScanDateInTime.AddDays(-ForwardScanLookBackDays);
                var endDate = ForwardScanDateInTime.AddDays(ForwardScanLookForwardDays);
                return $" WHERE CAST({RemoteTableDateColumn} as Date) >= CAST('{startDate}' as Date) AND CAST({RemoteTableDateColumn} as Date) <= CAST('{endDate}' as Date)";
            default:
                return "";
        }
    }

    public DateTime? FindMostRecentDateInLoadedData(IQuerySyntaxHelper syntaxFrom, string table, IDataLoadJob job)
    {
        string maxDateSql = $"SELECT MAX({RemoteTableDateColumn}) FROM {syntaxFrom.EnsureWrapped(table)} {SqlHistoricalDataFilter(job.LoadMetadata,syntaxFrom)}";


        using var con = _dbInfo.Server.GetConnection();
        var dt = new DataTable();
        using var cmd = _dbInfo.Server.GetCommand(maxDateSql, con);
        cmd.CommandTimeout = 30000;
        using var da = _dbInfo.Server.GetDataAdapter(cmd);
        da.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            return DateTime.Parse(dt.Rows[0][0].ToString());
        }
        return null;
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
        throw new NotImplementedException();
    }
}
