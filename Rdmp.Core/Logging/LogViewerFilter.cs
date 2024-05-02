// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text;

namespace Rdmp.Core.Logging;

/// <summary>
///     Decides which records to fetch from the hierarchical logging database including row filter for specific
///     run, table loaded etc.
/// </summary>
public class LogViewerFilter
{
    public LogViewerFilter(LoggingTables loggingTable)
    {
        LoggingTable = loggingTable;
    }

    /// <summary>
    ///     Creates a new filter showing records in the <paramref name="loggingTable" /> that belong to a parent Type (share
    ///     foreign key
    ///     <paramref name="id" /> e.g. applying a filter on <see cref="LoggingTables.ProgressLog" /> will show all log entries
    ///     for
    ///     the parent <see cref="LoggingTables.TableLoadRun" /> with that <paramref name="id" />.  Pass null to not filter.
    /// </summary>
    /// <param name="loggingTable"></param>
    /// <param name="id">ID of the parent object for which to extract a matching row collection</param>
    public LogViewerFilter(LoggingTables loggingTable, int? id)
    {
        LoggingTable = loggingTable;
        switch (loggingTable)
        {
            case LoggingTables.DataLoadTask:
                Task = id;
                break;
            case LoggingTables.DataLoadRun:
                Task = id;
                break;
            case LoggingTables.ProgressLog:
            case LoggingTables.FatalError:
            case LoggingTables.TableLoadRun:
                Run = id;
                break;
            case LoggingTables.DataSource:
                Table = id;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loggingTable), loggingTable, null);
        }
    }

    public LoggingTables LoggingTable { get; set; }

    public bool IsEmpty => Run == null && Table == null && Task == null;

    public int? Task { get; set; }

    public int? Run { get; set; }

    public int? Table { get; set; }

    /// <summary>
    ///     An object that contains data about the filter or supplementary information.  The default is null.
    /// </summary>
    public object Tag { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Task != null)
            sb.Append($"DataLoadTask={Task}");

        if (Run != null)
            sb.Append($"DataLoadRun={Run}");

        if (Table != null)
            sb.Append($"TableLoadRun={Table}");

        return sb.Length == 0 ? "No filter" : sb.ToString();
    }

    public string GetWhereSql()
    {
        return LoggingTable switch
        {
            LoggingTables.None => "",
            LoggingTables.DataLoadTask => Task.HasValue ? $"WHERE ID ={Task.Value}" : "",
            LoggingTables.DataLoadRun => Run.HasValue ? $"WHERE ID ={Run.Value}" :
                Task.HasValue ? $"WHERE dataLoadTaskID = {Task.Value}" : "",
            LoggingTables.ProgressLog => Run.HasValue ? $"WHERE dataLoadRunID ={Run.Value}" : "",
            LoggingTables.FatalError => Run.HasValue ? $"WHERE dataLoadRunID ={Run.Value}" : "",
            LoggingTables.TableLoadRun => Run.HasValue ? $"WHERE dataLoadRunID ={Run.Value}" : "",
            LoggingTables.DataSource => Table.HasValue ? $"WHERE tableLoadRunID ={Table.Value}" : "",
            _ => throw new ArgumentOutOfRangeException(nameof(LoggingTable))
        };
    }
}