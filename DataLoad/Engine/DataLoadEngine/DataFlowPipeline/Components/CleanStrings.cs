using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataFlowPipeline.Components
{
    [Description("Cleans all string values, currently only Trims")]
    public class CleanStrings : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<TableInfo>
    {
        private int _rowsProcessed = 0;
        private string _taskDescription;
        Stopwatch timer = new Stopwatch();

        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
            timer.Start();

            StartAgain:
            foreach (DataRow row in toProcess.Rows)
            {
                for (int i = 0; i < columnsToClean.Count; i++)
                {
                    string toClean = columnsToClean[i];
                    string val = null;
                    try
                    {
                        object o = row[toClean];

                        if(o == DBNull.Value || o == null)
                            continue;

                        if(!(o is string))
                            throw new ArgumentException("Despite being marked as a string column, object found in column " + toClean + " was of type " + o.GetType());

                        val = o as string;
                    }
                    catch (ArgumentException e)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,e.Message)); //column could not be found
                        columnsToClean.Remove(columnsToClean[i]);
                        goto StartAgain;
                    }


                    //it is empty
                    if (string.IsNullOrWhiteSpace(val))
                        row[toClean] = DBNull.Value;
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

            job.OnProgress(this,new ProgressEventArgs(_taskDescription,new ProgressMeasurement(_rowsProcessed, ProgressType.Records),timer.Elapsed));

            return toProcess;
        }

        List<string> columnsToClean = new List<string>();

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void PreInitialize(TableInfo target,IDataLoadEventListener listener)
        {
            if (target == null)
                throw new Exception("Without TableInfo we cannot figure out what columns to clean");

            _taskDescription = "Clean Strings " + target.GetRuntimeName() + ":";

            foreach (ColumnInfo col in target.ColumnInfos)
                if (col.Data_type != null && col.Data_type.Contains("char"))
                    columnsToClean.Add(col.GetRuntimeName());
            if (columnsToClean.Any())
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    "Preparing to perform clean " + columnsToClean.Count + " string columns (" +
                    string.Join(",", columnsToClean) + ") in table " + target.GetRuntimeName()));
            else
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    "Skipping CleanString on table " + target.GetRuntimeName() + " because there are no String columns in the table"));
        }

        
        public void Check(ICheckNotifier notifier)
        {
            
        }
    }
}