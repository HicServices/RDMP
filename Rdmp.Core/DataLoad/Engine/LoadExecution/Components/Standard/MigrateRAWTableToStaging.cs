// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Standard;

/// <summary>
/// Streams records from a single table in the RAW database and writes it to the corresponding table in the STAGING database during data load.  RAW is an
/// unconstrained identifiable version of the LIVE table created at the start of an RMDP data load (the RAW=>STAGING=>LIVE model).  STAGING is a constrained
/// (has primary keys / not nulls etc) version of the LIVE table.  This class uses a DataFlowPipelineEngine to stream the records and this includes (optionally)
/// any anonymisation operations (dropping columns, substituting identifiers etc) configured on the TableInfo (See BasicAnonymisationEngine).
/// </summary>
public class MigrateRAWTableToStaging : DataLoadComponent
{
    private readonly ITableInfo _tableInfo;
    private readonly bool _isLookupTable;
    private readonly HICDatabaseConfiguration _databaseConfiguration;

    public MigrateRAWTableToStaging(ITableInfo tableInfo, bool isLookupTable,
        HICDatabaseConfiguration databaseConfiguration)
    {
        _tableInfo = tableInfo;
        _isLookupTable = isLookupTable;
        _databaseConfiguration = databaseConfiguration;
    }

    private DataFlowPipelineEngine<DataTable> _pipeline;

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (_pipeline != null)
            throw new Exception("Pipeline already executed once");

        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.LoadsSingleTableInfo | PipelineUsage.FixedDestination |
                                            PipelineUsage.LogsToTableLoadInfo);

        //where we are coming from (source)
        const LoadBubble sourceConvention = LoadBubble.Raw;
        var sourceDatabase = _databaseConfiguration.DeployInfo[sourceConvention];
        var sourceTableName = _tableInfo.GetRuntimeName(sourceConvention, _databaseConfiguration.DatabaseNamer);

        //What to do if where we are coming from does not have the table existing on it
        if (!sourceDatabase.ExpectTable(sourceTableName).Exists())
            if (_isLookupTable)
            {
                job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Warning,
                        $"Lookup table {sourceTableName} did not exist on RAW so was not migrated to STAGING"));
                return ExitCodeType.Success;
            }
            else
            {
                job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Error,
                        $"Table {sourceTableName} did not exist in RAW database {sourceDatabase} when it came time to migrate RAW to STAGING (and the table is not a lookup)"));
            }


        // where we are going to (destination)
        // ignore any columns that are marked for discard
        const LoadBubble destinationConvention = LoadBubble.Staging;
        var destinationDatabase = _databaseConfiguration.DeployInfo[LoadBubble.Staging];
        var destinationTableName =
            _tableInfo.GetRuntimeName(destinationConvention, _databaseConfiguration.DatabaseNamer);

        DeleteFullyNullRecords(sourceTableName, sourceDatabase, job);

        //audit
        var tableLoadInfo = job.DataLoadInfo.CreateTableLoadInfo(
            "None required, if fails then simply drop Staging database and reload dataset",
            $"STAGING:{destinationTableName}",
            new DataSource[] { new($"RAW:{sourceTableName}", DateTime.Now) }, -1);

        var syntax = sourceDatabase.Server.GetQuerySyntaxHelper();

        //connect to source and open a reader! note that GetReaderForRAW will at this point preserve the state of the database such that any commands e.g. deletes will not have any effect even though ExecutePipeline has not been called!
        var source = new DbDataCommandDataFlowSource(
            $"Select distinct * from {syntax.EnsureWrapped(sourceTableName)}",
            $"Fetch data from {syntax.EnsureWrapped(sourceTableName)}",
            sourceDatabase.Server.Builder, 50000);

        //ignore those that are pre load discarded columns (unless they are dilution in which case they get passed through in a decrepid state instead of dumped entirely - these fields will still bein ANODump in pristene state btw)
        var columnNamesToIgnoreForBulkInsert = _tableInfo.PreLoadDiscardedColumns
            .Where(c => c.Destination != DiscardedColumnDestination.Dilute).Select(column => column.RuntimeColumnName)
            .ToList();

        //pass pre load discard
        var destination = new SqlBulkInsertDestination(destinationDatabase, destinationTableName,
            columnNamesToIgnoreForBulkInsert);
        destination.Timeout = 43200; //set max copy to 12 hours
        //engine that will move data
        _pipeline = new DataFlowPipelineEngine<DataTable>(context, source, destination, job);

        //add clean strings component
        _pipeline.ComponentObjects.Add(new CleanStrings());

        //add dropping of preload discard columns
        _pipeline.ComponentObjects.Add(new BasicAnonymisationEngine());

        _pipeline.Initialize(tableLoadInfo, _tableInfo);

        //tell it to move data
        _pipeline.ExecutePipeline(cancellationToken);

        return ExitCodeType.Success;
    }


    private void DeleteFullyNullRecords(string sourceTableName, DiscoveredDatabase dbInfo, IDataLoadJob job)
    {
        try
        {
            var cols = dbInfo.ExpectTable(sourceTableName).DiscoverColumns();

            using var con = dbInfo.Server.GetConnection();
            con.Open();
            using var cmd = dbInfo.Server.GetCommand(
                //Magical code that nukes blank/null rows - where all rows are blank/null
                $@"delete from {sourceTableName} WHERE {string.Join(" AND ",
                    cols.Select(c => $"({c} IS NULL OR {c}='')"))}", con);
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"About to delete fully null records using SQL:{cmd.CommandText}"));

            cmd.CommandTimeout = 500000;

            var affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows != 0)
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Deleted {affectedRows} fully blank/null rows from RAW database"));
        }
        catch (Exception e)
        {
            job.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    "Could not delete fully null records, this will not prevent the data load occurring", e));
        }
    }


    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }

    /*

    private DataTable CreateAnonymisedDataTable(DataTable rawTable, TableInfo tableInfo, ReflectionFactory<IAnonymisationEngine> anonymisationEngineFactory,IDataLoadJob job)
    {
        DataTable stagingTable = DatabaseOperations.CreateDataTableFromDbOnServer(_databaseConfiguration.GetInfoForStage(HICTableNamingConvention.Staging), tableInfo.GetRuntimeName());
        for (var i = 0; i < rawTable.Rows.Count; ++i)
            stagingTable.Rows.Add(stagingTable.NewRow());

        var anoEngine = anonymisationEngineFactory.Create(_loadMetadata.AnonymisationEngineClass);
        anoEngine.Initialize(tableInfo, job, null);
        stagingTable = anoEngine.ProcessPipelineData(rawTable, job);
        return stagingTable;
    }*/
}