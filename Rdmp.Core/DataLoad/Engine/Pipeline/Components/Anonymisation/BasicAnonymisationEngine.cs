// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;

/// <summary>
///     Pipeline component for anonymising DataTable batches in memory according to the configuration of ANOTables /
///     PreLoadDiscardedColumn(s) in the TableInfo.
///     Actual functionality is implemented in IdentifierDumper and ANOTransformer(s).
/// </summary>
public class BasicAnonymisationEngine : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<TableInfo>
{
    private bool _bInitialized;

    private readonly Dictionary<string, ANOTransformer> columnsToAnonymise = new();

    private IdentifierDumper _dumper;

    public TableInfo TableToLoad { get; set; }

    public void PreInitialize(TableInfo target, IDataLoadEventListener listener)
    {
        TableToLoad = target;
        _bInitialized = true;

        _dumper = new IdentifierDumper(TableToLoad);
        _dumper.CreateSTAGINGTable();

        //columns we expect to ANO
        foreach (var columnInfo in target.ColumnInfos)
        {
            var columnName = columnInfo.GetRuntimeName();

            if (columnInfo.ANOTable_ID != null)
            {
                //The metadata says this column should be ANOd
                if (!columnName.StartsWith(ANOTable.ANOPrefix))
                    throw new Exception(
                        $"ColumnInfo  {columnName} does not start with ANO but is marked as an ANO column (ID={columnInfo.ID})");

                //if the column is ANOGp then look for column Gp in the input columns (DataTable toProcess)
                columnName = columnName[ANOTable.ANOPrefix.Length..];
                columnsToAnonymise.Add(columnName, new ANOTransformer(columnInfo.ANOTable));
            }
        }
    }

    private int recordsProcessedSoFar;

    private readonly Stopwatch stopwatch_TimeSpentTransforming = new();
    private readonly Stopwatch stopwatch_TimeSpentDumping = new();

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var didAno = false;

        stopwatch_TimeSpentTransforming.Start();

        if (!_bInitialized)
            throw new Exception("Not Initialized yet");

        recordsProcessedSoFar += toProcess.Rows.Count;

        var missingColumns = columnsToAnonymise.Keys
            .Where(k => !toProcess.Columns.Cast<DataColumn>().Any(c => c.ColumnName.Equals(k))).ToArray();

        if (missingColumns.Any())
            throw new KeyNotFoundException(
                $"The following columns (which have ANO Transforms on them) were missing from the DataTable:{Environment.NewLine}{string.Join(Environment.NewLine, missingColumns)}{Environment.NewLine}The columns found in the DataTable were:{Environment.NewLine}{string.Join(Environment.NewLine, toProcess.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");

        //Dump Identifiers
        stopwatch_TimeSpentDumping.Start();
        _dumper.DumpAllIdentifiersInTable(
            toProcess); //do the dumping of all the rest of the columns (those that must disappear from pipeline as opposed to those above which were substituted for ANO versions)
        stopwatch_TimeSpentDumping.Stop();

        if (_dumper.HaveDumpedRecords)
            listener.OnProgress(this,
                new ProgressEventArgs("Dump Identifiers",
                    new ProgressMeasurement(recordsProcessedSoFar, ProgressType.Records),
                    stopwatch_TimeSpentDumping.Elapsed)); //time taken to dump identifiers

        //Process ANO Identifier Substitutions
        //for each column with an ANOTransformer
        foreach (var (column, transformer) in columnsToAnonymise)
        {
            didAno = true;

            //add an ANO version
            var ANOColumn = new DataColumn($"{ANOTable.ANOPrefix}{column}");
            toProcess.Columns.Add(ANOColumn);

            //populate ANO version
            transformer.Transform(toProcess, toProcess.Columns[column], ANOColumn);

            //drop the non ANO version
            toProcess.Columns.Remove(column);
        }

        stopwatch_TimeSpentTransforming.Stop();

        if (didAno)
            listener.OnProgress(this,
                new ProgressEventArgs("Anonymise Identifiers",
                    new ProgressMeasurement(recordsProcessedSoFar, ProgressType.Records),
                    stopwatch_TimeSpentTransforming.Elapsed)); //time taken to swap ANO identifiers

        return toProcess;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        _dumper.DropStaging();
    }

    public void Abort(IDataLoadEventListener listener)
    {
        _dumper.DropStaging();
    }

    public bool SilentRunning { get; set; }

    public void Check(ICheckNotifier notifier)
    {
    }
}