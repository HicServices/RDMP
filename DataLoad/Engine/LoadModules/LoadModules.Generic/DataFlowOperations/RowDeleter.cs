using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowOperations
{
    class RowDeleter: IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Looks for a column with exactly this name", Mandatory = true)]
        public string ColumnNameToFind { get; set; }

        [DemandsInitialization("Deletes all rows where the values in the specified ColumnNameToFind match the StandardRegex")]
        public StandardRegex DeleteRowsWhereValuesMatchStandard { get; set; }

        [DemandsInitialization("Deletes all rows where the values in the specified ColumnNameToFind match the Regex")]
        public Regex DeleteRowsWhereValuesMatch { get; set; }

        private int _deleted;
        private Stopwatch _sw = new Stopwatch();

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            _sw.Start();
            DataTable outputTable = new DataTable();

            foreach (DataColumn dataColumn in toProcess.Columns)
                outputTable.Columns.Add(dataColumn.ColumnName, dataColumn.DataType);

            Regex regex = DeleteRowsWhereValuesMatch ?? new Regex(DeleteRowsWhereValuesMatchStandard.Regex);

            foreach (DataRow row in toProcess.Rows)
            {
                var val = row[ColumnNameToFind];

                //keep nulls, dbnulls or anything where ToString doesn't match the regex
                if (val == null || val == DBNull.Value || !regex.IsMatch(val.ToString()))
                    outputTable.ImportRow(row);
                else
                    _deleted++;
            }

            listener.OnProgress(this,new ProgressEventArgs("Deleting Rows",new ProgressMeasurement(_deleted,ProgressType.Records),_sw.Elapsed ));

            _sw.Stop();
            return outputTable;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
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
}
