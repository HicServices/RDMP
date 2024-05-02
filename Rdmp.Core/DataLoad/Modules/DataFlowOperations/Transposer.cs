// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

/// <summary>
///     Pipeline component which rotates DataTables flowing through it by 90 degrees such that the first column becomes the
///     new headers.  Only use this if you have
///     been given a file in which proper headers are vertical down the first column and records are subsequent columns
///     (i.e. adding new records results in the
///     DataTable growing horizontally).
///     <para>
///         IMPORTANT: Only works with a single load batch if you have a chunked pipeline you cannot use this component
///         unless you set the chunk size large enough
///         to read the entire file in one go
///     </para>
/// </summary>
public class Transposer : IPluginDataFlowComponent<DataTable>
{
    private bool _haveServedResult;

    [DemandsInitialization(DelimitedFlatFileDataFlowSource.MakeHeaderNamesSane_DemandDescription,
        DemandType.Unspecified, true)]
    public bool MakeHeaderNamesSane { get; set; }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (toProcess == null)
            return null;

        if (_haveServedResult)
            throw new NotSupportedException(
                "Error, we received multiple batches, Transposer only works when all the data arrives in a single DataTable");

        if (toProcess.Rows.Count == 0 || toProcess.Columns.Count == 0)
            throw new NotSupportedException(
                $"DataTable toProcess had {toProcess.Rows.Count} rows and {toProcess.Columns.Count} columns, thus it cannot be transposed");

        _haveServedResult = true;

        return GenerateTransposedTable(toProcess);
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
    }

    private DataTable GenerateTransposedTable(DataTable inputTable)
    {
        var outputTable = new DataTable();
        outputTable.BeginLoadData();
        // Add columns by looping rows

        // Header row's first column is same as in inputTable
        outputTable.Columns.Add(inputTable.Columns[0].ColumnName);

        // Header row's second column onwards, 'inputTable's first column taken
        foreach (DataRow inRow in inputTable.Rows)
        {
            var newColName = inRow[0].ToString();

            if (MakeHeaderNamesSane)
                newColName = QuerySyntaxHelper.MakeHeaderNameSensible(newColName);

            outputTable.Columns.Add(newColName);
        }

        // Add rows by looping columns
        for (var rCount = 1; rCount <= inputTable.Columns.Count - 1; rCount++)
        {
            var newRow = outputTable.NewRow();

            // First column is inputTable's Header row's second column
            newRow[0] = inputTable.Columns[rCount].ColumnName;
            for (var cCount = 0; cCount <= inputTable.Rows.Count - 1; cCount++)
            {
                var colValue = inputTable.Rows[cCount][rCount].ToString();
                newRow[cCount + 1] = colValue;
            }

            outputTable.Rows.Add(newRow);
        }

        outputTable.EndLoadData();
        return outputTable;
    }
}