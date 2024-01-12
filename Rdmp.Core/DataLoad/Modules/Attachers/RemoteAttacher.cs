using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

public class RemoteAttacher(bool requestsExternalDatabaseCreation) : Attacher(requestsExternalDatabaseCreation), IPluginAttacher
{
    [DemandsInitialization("How far back to pull data from")]
    public AttacherHistoricalDurations HistoricalFetchDuration { get; set; }

    [DemandsInitialization("Which column in the remote table can be used to perform time-based data selection")]
    public string RemoteTableDateColumn { get; set; }

    [DemandsInitialization("Earliest date when using a custom fetch duration")]
    public string CustomFetchDurationStartDate { get; set; }

    [DemandsInitialization("Latest date when using a custom fetch duration")]
    public string CustomFetchDurationEndDate { get; set; }

    [DemandsInitialization("The Current Start Date for procedural fethching of historical data")]
    public DateTime ForwardScanDateInTime { get; set; }
    [DemandsInitialization("How many days the precedural fetching should look back ")]
    public int ForwardScanLookBackDays { get; set; } = 0;
    [DemandsInitialization("How many days the precedural fetching should look forward ")]
    public int ForwardScanLookForwardDays { get; set; } = 0;


    public string SqlHistoricalDataFilter(ILoadMetadata loadMetadata)
    {
        switch (HistoricalFetchDuration)
        {
            case AttacherHistoricalDurations.Past24Hours:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > dateadd(DAY, -1, GETDATE())";
            case AttacherHistoricalDurations.Past7Days:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > dateadd(WEEK, -1, GETDATE())";
            case AttacherHistoricalDurations.PastMonth:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > dateadd(MONTH, -1, GETDATE())";
            case AttacherHistoricalDurations.PastYear:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) > dateadd(YEAR, -1, GETDATE())";
            case AttacherHistoricalDurations.SinceLastUse:
                if (loadMetadata.LastLoadTime is not null) return $" WHERE CAST({RemoteTableDateColumn} as Date) > CAST({loadMetadata.LastLoadTime} as Date)";
                return "";
            case AttacherHistoricalDurations.Custom:
                return $" WHERE CAST({RemoteTableDateColumn} as Date) >= CAST('{CustomFetchDurationStartDate}' as Date) AND CAST({RemoteTableDateColumn} as Date) <= CAST('{CustomFetchDurationEndDate}' as Date)";
            case AttacherHistoricalDurations.ForwardScan:
                var startDate = ForwardScanDateInTime.AddDays(-ForwardScanLookBackDays);
                var endDate = ForwardScanDateInTime.AddDays(ForwardScanLookForwardDays);
                return $" WHERE CAST({RemoteTableDateColumn} as Date) >= CAST('{startDate}' as Date) AND CAST({RemoteTableDateColumn} as Date) <= CAST('{endDate}' as Date)";
            default:
                return "";
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
        throw new NotImplementedException();
    }
}
