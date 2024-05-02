// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
///     Attempts to resolves primary key collisions by ordering on a specific column and deleting rows which differ on this
///     column
/// </summary>
public class SafePrimaryKeyCollisionResolverMutilation : IPluginMutilateDataTables
{
    private DiscoveredDatabase _database;
    private LoadStage _loadStage;

    [DemandsInitialization(
        "The non primary key column to be used for deduplication.  This must contain different values for the same primary key and you must only want to keep one e.g. DataAge")]
    public ColumnInfo ColumnToResolveOn { get; set; }

    [DemandsInitialization(
        @"Determines behaviour when a primary key collision is the result of one record having null and another not.
True - Delete the non null record
False - Delete the null record")]
    public bool PreferNulls { get; set; }

    [DemandsInitialization(@"Determines which record is deleted when two values of ColumnToResolveOn are conflicting
True - Delete the smaller value
False - Delete the larger value")]
    public bool PreferLargerValues { get; set; }

    [DemandsInitialization("Timeout in seconds to allow the operation to run for", DefaultValue = 600)]
    public int Timeout { get; set; }

    public void DeleteRows(DiscoveredTable tbl, ColumnInfo[] primaryKeys, IDataLoadEventListener listener)
    {
        var join = string.Join(" AND ", primaryKeys.Select(k => $"t1.{k.GetRuntimeName()}=t2.{k.GetRuntimeName()}"));


        var t1DotColumn = $"t1.{ColumnToResolveOn.GetRuntimeName(_loadStage)}";
        var t2DotColumn = $"t2.{ColumnToResolveOn.GetRuntimeName(_loadStage)}";

        //FYI - we are considering whether to delete records from table {0}

        var deleteConditional =
            PreferNulls
                ?
                //delete rows {0} where {0} is not null and {1} is null - leaving only the null records {1}
                $"({t1DotColumn} IS NOT NULL AND {t2DotColumn} IS NULL)"
                : string.Format("({1} IS NOT NULL AND {0} IS NULL)", t1DotColumn, t2DotColumn);

        deleteConditional += " OR ";

        deleteConditional += PreferLargerValues
            //delete rows {0} where {0} less than {1} - leaving only the greater records {1}
            ? string.Format("({0} <> {1} AND {0} < {1})", t1DotColumn, t2DotColumn)
            : string.Format("({0} <> {1} AND {1} < {0})", t1DotColumn, t2DotColumn);


        var sql = string.Format(@"DELETE t1 FROM {0} t1
  JOIN {0} t2
  ON {1}
  AND ({2})",
            tbl.GetRuntimeName(), join, deleteConditional);

        using var con = tbl.Database.Server.GetConnection();
        con.Open();
        using var cmd = tbl.Database.Server.GetCommand(sql, con);
        cmd.CommandTimeout = Timeout;

        var affectedRows = cmd.ExecuteNonQuery();
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Deleted {affectedRows} rows"));
    }

    public void Check(ICheckNotifier notifier)
    {
        if (ColumnToResolveOn is { IsPrimaryKey: true })
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"You cannot use {ColumnToResolveOn} to resolve primary key collisions because it is part of the primary key",
                CheckResult.Fail));
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _database = dbInfo;
        _loadStage = loadStage;
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var tbl = _database.ExpectTable(
            ColumnToResolveOn.TableInfo.GetRuntimeName(_loadStage, job.Configuration.DatabaseNamer));
        var pks = ColumnToResolveOn.TableInfo.ColumnInfos.Where(ci => ci.IsPrimaryKey).ToArray();

        DeleteRows(tbl, pks, job);

        return ExitCodeType.Success;
    }
}