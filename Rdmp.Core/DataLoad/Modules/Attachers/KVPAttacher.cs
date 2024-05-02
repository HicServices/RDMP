// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     Data load component for loading very wide files into RAW tables by translating columns into key value pairs.
///     Relies on a user configured pipeline for
///     reading from the file (so it can support csv, fixed width, excel etc).  Once the user configured pipeline has read
///     a DataTable from the file (which is
///     expected to have lots of columns which might be sparsely populated or otherwise suitable for key value pair
///     representation rather than traditional
///     relational/flat format.
///     <para>
///         Component converts each DataTable row into one or more rows in the format pk,key,value where pk are the
///         column(s) which uniquely identify the source
///         row (e.g. Labnumber).  See KVPAttacher.docx for a full explanation.
///     </para>
/// </summary>
public class KVPAttacher : FlatFileAttacher, IDemandToUseAPipeline, IDataFlowDestination<DataTable>
{
    [DemandsInitialization("Pipeline for reading from the flat file", Mandatory = true)]
    public Pipeline PipelineForReadingFromFlatFile { get; set; }

    private readonly List<DataTable> BatchesReadyForProcessing = new();

    [DemandsInitialization(
        "A comma separated list of column names that make up the primary key of the KVP table (in most cases this is only one column).  These must appear in the file",
        Mandatory = true)]
    public string PrimaryKeyColumns { get; set; }

    [DemandsInitialization(
        "The name of the column in RAW which will store the Key component of the KeyValuePair relationship e.g. 'Key', 'Test' or 'Attribute' etc",
        Mandatory = true)]
    public string TargetDataTableKeyColumnName { get; set; }

    [DemandsInitialization(
        "The name of the column in RAW which will store the Value component of the KeyValuePair relationship e.g. 'Value', 'Result' or 'Val' etc",
        Mandatory = true)]
    public string TargetDataTableValueColumnName { get; set; }

    #region Attacher Functionality

    protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (BatchesReadyForProcessing.Any())
            throw new NotSupportedException(
                "There are still batches awaiting dispatch to RAW, we cannot open a new file at this time");

        var flatFileToLoad = new FlatFileToLoad(fileToLoad);

        //stamp out the pipeline into an instance
        var dataFlow =
            new KVPAttacherPipelineUseCase(this, flatFileToLoad).GetEngine(PipelineForReadingFromFlatFile, listener);

        //will result in the opening and processing of the file and the passing of DataTables through the Pipeline finally arriving at the destination (us) in ProcessPipelineData
        dataFlow.ExecutePipeline(cancellationToken);
    }

    protected override void CloseFile()
    {
    }

    protected override void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job)
    {
        var pks = GetPKs();

        //make sure the primary key columns are in all the relevant tables
        foreach (var pk in pks)
        {
            if (!loadTarget.Columns.Contains(pk))
                throw new KeyNotFoundException(
                    $"Could not find a column called {pk} (part of the PrimaryKey) on destination table {TableName}");

            foreach (var batchTable in BatchesReadyForProcessing)
                if (!batchTable.Columns.Contains(pk))
                    throw new KeyNotFoundException(
                        $"Source Batch DataTable (read from Pipeline) was missing column {pk} (columns in DataTable were:{string.Join(",", batchTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");
        }

        if (!loadTarget.Columns.Contains(TargetDataTableKeyColumnName))
            throw new KeyNotFoundException(
                $"Target destination table {TableName} did not contain a column called '{TargetDataTableKeyColumnName}' which is where we were told to store the Keys of the Key value pairs (note that the Key the column that matches the value not the primary key columns which are separate)");

        if (!loadTarget.Columns.Contains(TargetDataTableValueColumnName))
            throw new KeyNotFoundException(
                $"Target destination table {TableName} did not contain a column called '{TargetDataTableValueColumnName}' which is where we were told to store the Value of the Key value pairs");
    }

    protected override int IterativelyBatchLoadDataIntoDataTable(DataTable dt, int maxBatchSize,
        GracefulCancellationToken cancellationToken)
    {
        //there are no batches for processing
        if (!BatchesReadyForProcessing.Any())
            return 0;

        var pks = GetPKs();

        //handle batch 0
        var currentBatch = BatchesReadyForProcessing[0];

        var recordsGenerated = 0;
        dt.BeginLoadData();
        foreach (DataRow batchRow in currentBatch.Rows)
        {
            var pkValues = new Dictionary<string, object>();

            foreach (var pk in pks)
                pkValues.Add(pk, batchRow[pk]);

            foreach (DataColumn col in currentBatch.Columns)
            {
                if (pks.Contains(col.ColumnName))
                    continue; //it's a primary key column

                var k = col.ColumnName;
                var val = batchRow[k];

                var newRow = dt.Rows.Add();
                foreach (var pk in pks)
                    newRow[pk] = pkValues[pk];
                newRow[TargetDataTableKeyColumnName] = k;
                newRow[TargetDataTableValueColumnName] = val;

                recordsGenerated++;
            }
        }

        dt.EndLoadData();
        BatchesReadyForProcessing.Remove(currentBatch);

        return recordsGenerated;
    }

    private string[] GetPKs()
    {
        return string.IsNullOrWhiteSpace(PrimaryKeyColumns)
            ? Array.Empty<string>()
            : PrimaryKeyColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    }

    #endregion

    #region IDataFlowDestination Members

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        BatchesReadyForProcessing.Add(toProcess.Copy());
        return null;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    #endregion


    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        if (string.IsNullOrWhiteSpace(PrimaryKeyColumns))
            notifier.OnCheckPerformed(new CheckEventArgs("Argument PrimaryKeyColumns has not been set",
                CheckResult.Fail));

        var pks = GetPKs();

        if (string.IsNullOrWhiteSpace(TargetDataTableKeyColumnName))
            notifier.OnCheckPerformed(new CheckEventArgs("Argument TargetDataTableKeyColumnName has not been set",
                CheckResult.Fail));

        if (string.IsNullOrWhiteSpace(TargetDataTableValueColumnName))
            notifier.OnCheckPerformed(new CheckEventArgs("Argument TargetDataTableValueColumnName has not been set",
                CheckResult.Fail));

        var duplicate = pks.FirstOrDefault(s =>
            s.Equals(TargetDataTableKeyColumnName) || s.Equals(TargetDataTableValueColumnName));

        if (duplicate != null)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Field '{duplicate}' is both a PrimaryKeyColumn and a TargetDataTable column, this is not allowed.  Your fields Pk1,Pk2,Pketc,Key,Value must all be mutually exclusive",
                CheckResult.Fail));

        if (TargetDataTableKeyColumnName != null && TargetDataTableKeyColumnName.Equals(TargetDataTableValueColumnName))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "TargetDataTableKeyColumnName cannot be the same as TargetDataTableValueColumnName", CheckResult.Fail));
    }

    public IPipelineUseCase GetDesignTimePipelineUseCase(RequiredPropertyInfo property)
    {
        return new KVPAttacherPipelineUseCase(this, new FlatFileToLoad(null));
    }
}