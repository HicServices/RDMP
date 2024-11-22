// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Data load component for loading RAW tables with records read from a remote database server.
/// Fetches all table from the specified database to load all catalogues specified.
/// </summary>
public class RemoteDatabaseAttacher : RemoteAttacher
{
    [DemandsInitialization("The DataSource to connect to in order to read data.", Mandatory = true)]
    public ExternalDatabaseServer RemoteSource { get; set; }

    [DemandsInitialization(
        "The length of time in seconds to allow for data to be completely read from the destination before giving up (0 for no timeout)")]
    public int Timeout { get; set; }

    [DemandsInitialization(
        """
        Determines how columns in the remote database are fetched and used to populate RAW tables of the same name.
        True - Fetch only the default columns that appear in RAW (e.g. skip hic_ columns)
        False - Fetch all columns in the remote table.  To use this option you will need ALTER statements in RAW scripts to make table(s) match the remote schema
        """,
        DefaultValue = true)]
    public bool LoadRawColumnsOnly { get; set; }

    [DemandsInitialization(
        """
        By default RemoteDatabaseAttacher expects all tables in the load to exist in the remote database at the time the load is run.  Enabling this option will ignore missing tables:
        True - Ignore the fact that some tables do not exist and skip them
        False - Trigger an error reporting the missing table(s)

        """)]
    public bool IgnoreMissingTables { get; set; }




    public override void Check(ICheckNotifier notifier)
    {
        if (!RemoteSource.Discover(DataAccessContext.DataLoad).Exists())
            throw new Exception($"Database {RemoteSource.Database} did not exist on the remote server");

        if (HistoricalFetchDuration is not AttacherHistoricalDurations.AllTime && RemoteTableDateColumn is null)
            throw new Exception("No Remote Table Date Column is set, but a historical duration is. Add a date column to continue.");
    }

    public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (job == null)
            throw new Exception("Job is Null, we require to know the job to build a DataFlowPipeline");

        var dbFrom = RemoteSource.Discover(DataAccessContext.DataLoad);

        var remoteTables = new HashSet<string>(dbFrom.DiscoverTables(true).Select(static t => t.GetRuntimeName()),
            StringComparer.CurrentCultureIgnoreCase);
        var loadables = job.RegularTablesToLoad.Union(job.LookupTablesToLoad).ToArray();

        var syntaxFrom = dbFrom.Server.GetQuerySyntaxHelper();

        foreach (var tableInfo in loadables)
        {
            var table = tableInfo.GetRuntimeName();

            // A table in the load does not exist on the remote db
            if (!remoteTables.Contains(table))
            {
                if (!IgnoreMissingTables)
                    throw new Exception(
                        $"Loadable table {table} was NOT found on the remote DB and IgnoreMissingTables is false");

                job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"Table {table} was NOT found on the remote DB but {nameof(IgnoreMissingTables)} is enabled so table will be skipped"));

                //skip it
                continue;
            }

            string sql;
            if (LoadRawColumnsOnly)
            {
                var rawColumns = LoadRawColumnsOnly
                    ? tableInfo.GetColumnsAtStage(LoadStage.AdjustRaw)
                    : tableInfo.ColumnInfos;
                sql =
                    $"SELECT {string.Join(",", rawColumns.Select(c => syntaxFrom.EnsureWrapped(c.GetRuntimeName(LoadStage.AdjustRaw))))} FROM {syntaxFrom.EnsureWrapped(table)} {SqlHistoricalDataFilter(job.LoadMetadata, dbFrom.Server.DatabaseType)}";
            }
            else
            {
                sql = $"SELECT * FROM {syntaxFrom.EnsureWrapped(table)} {SqlHistoricalDataFilter(job.LoadMetadata,RemoteSource.DatabaseType)}";
            }
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"About to execute SQL:{Environment.NewLine}{sql}"));

            var source = new DbDataCommandDataFlowSource(sql, $"Fetch data from {dbFrom} to populate RAW table {table}",
                dbFrom.Server.Builder, Timeout == 0 ? 50000 : Timeout);

            var destination = new SqlBulkInsertDestination(_dbInfo, table, Enumerable.Empty<string>());

            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo | PipelineUsage.FixedDestination);

            var engine = new DataFlowPipelineEngine<DataTable>(context, source, destination, job);

            var loadInfo = job.DataLoadInfo.CreateTableLoadInfo($"Truncate RAW table {table}",
                $"{_dbInfo.Server.Name}.{_dbInfo.GetRuntimeName()}.{table}",
                new[]
                {
                    new DataSource(
                        $"Remote SqlServer Servername={dbFrom.Server};Database={_dbInfo.GetRuntimeName()}{(table != null ? $" Table={table}" : $" Query = {sql}")}",
                        DateTime.Now)
                }, -1);

            engine.Initialize(loadInfo);
            engine.ExecutePipeline(cancellationToken);

            if (source.TotalRowsRead == 0)
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"No rows were read from the remote table {table}."));

            job.OnNotify(this, new NotifyEventArgs(
                source.TotalRowsRead > 0 ? ProgressEventType.Information : ProgressEventType.Warning,
                $"Finished after reading {source.TotalRowsRead} rows"));

            if (SetDeltaReadingToLatestSeenDatePostLoad)
                FindMostRecentDateInLoadedData(syntaxFrom, dbFrom.Server.DatabaseType, table, job);
        }


        return ExitCodeType.Success;
    }
}