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
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components;

/// <summary>
/// Pipeline component which trims all strings (removes leading and trailing whitespace) and turns blank strings into proper nulls (DBNull.Value).  Columns
/// are only processed if they are destined to go into a char field in the database (According to the TableInfo being processed).
/// </summary>
public class CleanStrings : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<TableInfo>
{
    private int _rowsProcessed;
    private string _taskDescription;
    private Stopwatch timer = new();

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener job,
        GracefulCancellationToken cancellationToken)
    {
        timer.Start();
        toProcess.BeginLoadData();
        StartAgain:
        foreach (DataRow row in toProcess.Rows)
        {
            for (var i = 0; i < columnsToClean.Count; i++)
            {
                var toClean = columnsToClean[i];
                string val;
                try
                {
                    var o = row[toClean];

                    if (o == DBNull.Value || o == null)
                        continue;

                    if (o is not string s)
                        throw new ArgumentException(
                            $"Despite being marked as a string column, object found in column {toClean} was of type {o.GetType()}");

                    val = s;
                }
                catch (ArgumentException e)
                {
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning, e.Message)); //column could not be found
                    columnsToClean.Remove(columnsToClean[i]);
                    goto StartAgain;
                }


                //it is empty
                if (string.IsNullOrWhiteSpace(val))
                {
                    row[toClean] = DBNull.Value;
                }
                else
                {
                    //trim it
                    var valAfterClean = val.Trim();

                    //set it
                    if (val != valAfterClean)
                        row[toClean] = valAfterClean;
                }
            }

            _rowsProcessed++;
        }

        timer.Stop();

        job.OnProgress(this,
            new ProgressEventArgs(_taskDescription, new ProgressMeasurement(_rowsProcessed, ProgressType.Records),
                timer.Elapsed));
        toProcess.EndLoadData();
        return toProcess;
    }

    private List<string> columnsToClean = new();

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void PreInitialize(TableInfo target, IDataLoadEventListener listener)
    {
        if (target == null)
            throw new Exception("Without TableInfo we cannot figure out what columns to clean");

        _taskDescription = $"Clean Strings {target.GetRuntimeName()}:";

        foreach (var col in target.ColumnInfos)
            if (col.Data_type != null && col.Data_type.Contains("char"))
                columnsToClean.Add(col.GetRuntimeName());
        if (columnsToClean.Any())
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Preparing to perform clean {columnsToClean.Count} string columns ({string.Join(",", columnsToClean)}) in table {target.GetRuntimeName()}"));
        else
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Skipping CleanString on table {target.GetRuntimeName()} because there are no String columns in the table"));
    }


    public void Check(ICheckNotifier notifier)
    {
    }
}