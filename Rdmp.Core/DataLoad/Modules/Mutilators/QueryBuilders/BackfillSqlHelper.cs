// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.DataLoad.Modules.Mutilators.QueryBuilders;

/// <summary>
///     Helps generate sql queries for reverting/deleting STAGING based on records in LIVE during a backfill data load (See
///     StagingBackfillMutilator).
/// </summary>
public class BackfillSqlHelper
{
    private readonly ColumnInfo _timePeriodicityField;
    private readonly DiscoveredDatabase _stagingDbInfo;
    private readonly DiscoveredDatabase _liveDbInfo;
    private readonly TableInfo _tiWithTimeColumn;

    public BackfillSqlHelper(ColumnInfo timePeriodicityField, DiscoveredDatabase stagingDbInfo,
        DiscoveredDatabase liveDbInfo)
    {
        _timePeriodicityField = timePeriodicityField;
        _stagingDbInfo = stagingDbInfo;
        _liveDbInfo = liveDbInfo;
        _tiWithTimeColumn = _timePeriodicityField.TableInfo;
    }

    /// <summary>
    ///     Composes the SQL which joins the supplied table back up or down to the TimePeriodicity table, so we can assign the
    ///     rows an effective load date
    /// </summary>
    /// <param name="tableAlias"></param>
    /// <param name="tableInfo"></param>
    /// <param name="timePeriodTableAlias"></param>
    /// <param name="dbInfo"></param>
    /// <param name="joinPath"></param>
    /// <returns></returns>
    public string CreateSqlForJoinToTimePeriodicityTable(string tableAlias, ITableInfo tableInfo,
        string timePeriodTableAlias, DiscoveredDatabase dbInfo, List<JoinInfo> joinPath)
    {
        if (tableInfo.ID == _timePeriodicityField.TableInfo_ID && joinPath.Count > 0)
            throw new InvalidOperationException(
                "You have asked for a join where the original table *is* the TimePeriodicityTable but a non-empty join path has been provided. There should be no path when dealing directly with the TimePeriodicity table");

        // Simple case, there is no join so we are just selecting the row and aliasing the TimePeriodicityField for the provided table
        if (!joinPath.Any())
            return string.Format(@"SELECT {0}.*, {0}.{1} AS TimePeriodicityField FROM {2} {0}",
                tableAlias, _timePeriodicityField.GetRuntimeName(),
                $"[{dbInfo.GetRuntimeName()}]..[{tableInfo.GetRuntimeName()}]");

        // Ensure that the TimePeriodicityTable is at the end of the path (to make constructing the join a bit easier)
        if (joinPath[0].ForeignKey.TableInfo_ID == _tiWithTimeColumn.ID ||
            joinPath[0].PrimaryKey.TableInfo_ID == _tiWithTimeColumn.ID)
            joinPath.Reverse();

        if (joinPath[^1].ForeignKey.TableInfo_ID != _tiWithTimeColumn.ID &&
            joinPath[^1].PrimaryKey.TableInfo_ID != _tiWithTimeColumn.ID)
            throw new InvalidOperationException(
                "The TimePeriodicity table is not at the beginning or end of the join path.");

        var sql =
            $@"SELECT {tableAlias}.*, {timePeriodTableAlias}.{_timePeriodicityField.GetRuntimeName()} AS TimePeriodicityField 
FROM {$"[{dbInfo.GetRuntimeName()}]..[{tableInfo.GetRuntimeName()}]"} {tableAlias}";

        // Is our table a parent or child? The join is composed differently.
        var ascending = tableInfo.ID == joinPath[0].ForeignKey.TableInfo_ID;

        for (var i = 0; i < joinPath.Count; i++)
        {
            var join = joinPath[i];

            if (ascending)
            {
                var parentTable = join.PrimaryKey.TableInfo;
                var childTableAlias = i == 0 ? tableAlias : $"j{i}";
                var parentTableAlias = i == joinPath.Count - 1 ? timePeriodTableAlias : $"j{i + 1}";

                sql += string.Format(@"
LEFT JOIN {0} {1} ON {1}.{3} = {2}.{4}",
                    $"[{dbInfo.GetRuntimeName()}]..[{parentTable.GetRuntimeName()}]",
                    parentTableAlias,
                    childTableAlias,
                    join.PrimaryKey.GetRuntimeName(),
                    join.ForeignKey.GetRuntimeName());
            }
            else
            {
                var childTable = join.ForeignKey.TableInfo;
                var parentTableAlias = i == 0 ? tableAlias : $"j{i + 1}";
                var childTableAlias = i == joinPath.Count - 1 ? timePeriodTableAlias : $"j{i}";

                sql += string.Format(@"
LEFT JOIN {0} {1} ON {2}.{3} = {1}.{4}",
                    $"[{dbInfo.GetRuntimeName()}]..[{childTable.GetRuntimeName()}]",
                    childTableAlias,
                    parentTableAlias,
                    join.PrimaryKey.GetRuntimeName(),
                    join.ForeignKey.GetRuntimeName());
            }
        }

        return sql;
    }


    public string GetSQLComparingStagingAndLiveTables(ITableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable)
    {
        // All rows in STAGING tiCurrent + the time from the TimePeriodicity table
        var toLoadWithTimeSQL =
            $"({CreateSqlForJoinToTimePeriodicityTable("CurrentTable", tiCurrent, "TimePeriodicityTable", _stagingDbInfo, joinPathToTimeTable)}) AS ToLoadWithTime";

        // All rows in LIVE tiCurrent + the time from the TimePeriodicity table
        var loadedWithTimeSQL =
            $"({CreateSqlForJoinToTimePeriodicityTable("LiveCurrentTable", tiCurrent, "LiveTimePeriodicityTable", _liveDbInfo, joinPathToTimeTable)}) AS LoadedWithTime";

        var pksForCurrent = tiCurrent.ColumnInfos.Where(info => info.IsPrimaryKey);
        var pkEquality = string.Join(" AND ",
            pksForCurrent.Select(
                info => $"ToLoadWithTime.{info.GetRuntimeName()} = LoadedWithTime.{info.GetRuntimeName()}"));

        // Join to leave valid STAGING rows which are stale, or whose relation with the TimePeriodicity field has been severed and as such should be deleted anyway (since we can't assign a date to the record)
        var cte = $@"
{toLoadWithTimeSQL} 
RIGHT JOIN 
{loadedWithTimeSQL} 
ON {pkEquality} 
WHERE ToLoadWithTime.ID IS NOT NULL 
    AND (ToLoadWithTime.TimePeriodicityField <= LoadedWithTime.TimePeriodicityField OR ToLoadWithTime.TimePeriodicityField IS NULL) 
)";
        return cte;
    }
}