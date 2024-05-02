// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
///     Resolves primary key collisions that are the result of non primary key fields being null in some records and not
///     null in others (where primary keys of those records
///     are the same).  Or to put it simpler, resolves primary key collisions by making records less null.  This can only
///     be applied in the Adjust RAW stage of a data load.
///     This creates deviation from ground truth of the data you are loading and reducing nullness might not always be
///     correct according to your data.
/// </summary>
public class Coalescer : MatchingTablesMutilator
{
    [DemandsInitialization(
        "Pass true to create an index on the primary keys which are joined together (can improve performance)",
        DefaultValue = false)]
    public bool CreateIndex { get; set; }

    public Coalescer() : base(LoadStage.AdjustRaw)
    {
    }

    protected override void MutilateTable(IDataLoadEventListener job, ITableInfo tableInfo, DiscoveredTable table)
    {
        var server = table.Database.Server;

        job.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"About to run Coalesce on table {table}"));

        var allCols = table.DiscoverColumns();

        var pkColumnInfos = tableInfo.ColumnInfos.Where(c => c.IsPrimaryKey).Select(c => c.GetRuntimeName()).ToArray();
        var nonPks = allCols.Where(c => !pkColumnInfos.Contains(c.GetRuntimeName())).ToArray();
        var pks = allCols.Except(nonPks).ToArray();

        if (!pkColumnInfos.Any())
            throw new Exception($"Table '{tableInfo}' has no IsPrimaryKey columns");

        if (allCols.Length == pkColumnInfos.Length)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Skipping Coalesce on table {table} because it has no non primary key columns"));
            return;
        }

        var affectedRows = 0;

        using (var con = table.Database.Server.GetConnection())
        {
            con.Open();

            if (CreateIndex)
            {
                using var idxCmd =
                    server.GetCommand(
                        string.Format(@"CREATE INDEX IX_PK_{0} ON {0}({1});", table.GetRuntimeName(),
                            string.Join(",", pks.Select(p => p.GetRuntimeName()))), con);
                idxCmd.CommandTimeout = Timeout;
                idxCmd.ExecuteNonQuery();
            }

            //Get an update command for each non primary key column
            foreach (var nonPk in nonPks)
            {
                var sql = GetCommand(table, pks, nonPk);

                var cmd = server.GetCommand(sql, con);
                cmd.CommandTimeout = Timeout;
                affectedRows += cmd.ExecuteNonQuery();
            }
        }

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Coalesce on table '{table}' completed ({affectedRows} rows affected)"));
    }

    private static string GetCommand(DiscoveredTable table, DiscoveredColumn[] pks, DiscoveredColumn nonPk)
    {
        var sqlLines = new List<CustomLine>
        {
            new($"(t1.{nonPk.GetRuntimeName()} is null AND t2.{nonPk.GetRuntimeName()} is not null)",
                QueryComponent.WHERE),
            new(
                $"t1.{nonPk.GetRuntimeName()} = COALESCE(t1.{nonPk.GetRuntimeName()},t2.{nonPk.GetRuntimeName()})",
                QueryComponent.SET)
        };
        sqlLines.AddRange(pks.Select(p =>
            new CustomLine(string.Format("t1.{0} = t2.{0}", p.GetRuntimeName()), QueryComponent.JoinInfoJoin)));

        var updateHelper = table.Database.Server.GetQuerySyntaxHelper().UpdateHelper;

        return updateHelper.BuildUpdate(
            table,
            table,
            sqlLines);
    }
}