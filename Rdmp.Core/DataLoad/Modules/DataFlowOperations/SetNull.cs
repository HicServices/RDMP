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
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

class SetNull : IPluginDataFlowComponent<DataTable>
{
    [DemandsInitialization("Looks for a column with exactly this name", Mandatory = true)]
    public string ColumnNameToFind { get; set; }

    [DemandsInitialization("Deletes all rows where the values in the specified ColumnNameToFind match the StandardRegex")]
    public StandardRegex NullCellsWhereValuesMatchStandard { get; set; }

    [DemandsInitialization("Deletes all rows where the values in the specified ColumnNameToFind match the Regex")]
    public Regex NullCellsWhereValuesMatch { get; set; }

    private int _changes;
    private Stopwatch _sw = new Stopwatch();

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        _sw.Start();
            
        Regex regex = NullCellsWhereValuesMatch ?? new Regex(NullCellsWhereValuesMatchStandard.Regex);

        foreach (DataRow row in toProcess.Rows)
        {
            var val = row[ColumnNameToFind];

            //keep nulls, dbnulls or anything where ToString doesn't match the regex
            if (val != null && val != DBNull.Value && regex.IsMatch(val.ToString()))
            {
                row[ColumnNameToFind] = DBNull.Value;
                _changes++;
            }
                
        }

        listener.OnProgress(this,new ProgressEventArgs("SetNull Rows",new ProgressMeasurement(_changes,ProgressType.Records),_sw.Elapsed ));

        _sw.Stop();
        return toProcess;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {

        listener.OnNotify(this,new NotifyEventArgs(_changes > 0 ? ProgressEventType.Warning : ProgressEventType.Information,$"Total SetNull operations for ColumnNameToFind '{ColumnNameToFind}' was {_changes}"));
    }

    public void Abort(IDataLoadEventListener listener)
    {
            
    }

    public void Check(ICheckNotifier notifier)
    {
        if (NullCellsWhereValuesMatch == null && NullCellsWhereValuesMatchStandard == null)
            notifier.OnCheckPerformed(new CheckEventArgs("You must specify a Regex for value selection", CheckResult.Fail));
    }
}