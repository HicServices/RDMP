// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

internal class RowDeleter : IPluginDataFlowComponent<DataTable>
{
    [DemandsInitialization("Looks for a column with exactly this name", Mandatory = true)]
    public string ColumnNameToFind { get; set; }

    [DemandsInitialization(
        "Deletes all rows where the values in the specified ColumnNameToFind match the StandardRegex")]
    public StandardRegex DeleteRowsWhereValuesMatchStandard { get; set; }

    [DemandsInitialization("Deletes all rows where the values in the specified ColumnNameToFind match the Regex")]
    public Regex DeleteRowsWhereValuesMatch { get; set; }

    private int _deleted;
    private readonly Stopwatch _sw = new();

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        _sw.Start();
        var outputTable = new DataTable();

        foreach (DataColumn dataColumn in toProcess.Columns)
            outputTable.Columns.Add(dataColumn.ColumnName, dataColumn.DataType);

        var regex = DeleteRowsWhereValuesMatch ?? new Regex(DeleteRowsWhereValuesMatchStandard.Regex);

        foreach (DataRow row in toProcess.Rows)
        {
            var val = row[ColumnNameToFind];

            //keep nulls, dbnulls or anything where ToString doesn't match the regex
            if (val == null || val == DBNull.Value || !regex.IsMatch(val.ToString()))
                outputTable.ImportRow(row);
            else
                _deleted++;
        }

        listener.OnProgress(this,
            new ProgressEventArgs("Deleting Rows", new ProgressMeasurement(_deleted, ProgressType.Records),
                _sw.Elapsed));

        _sw.Stop();
        return outputTable;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        listener.OnNotify(this,
            new NotifyEventArgs(_deleted > 0 ? ProgressEventType.Warning : ProgressEventType.Information,
                $"Total RowDeleted operations for ColumnNameToFind '{ColumnNameToFind}' was {_deleted}"));
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        if (DeleteRowsWhereValuesMatch == null && DeleteRowsWhereValuesMatchStandard == null)
            notifier.OnCheckPerformed(new CheckEventArgs("You must specify a Regex for deletion", CheckResult.Fail));
    }
}