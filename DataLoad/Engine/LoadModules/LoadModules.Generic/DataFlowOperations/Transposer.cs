using System;
using System.ComponentModel;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using LoadModules.Generic.DataFlowSources;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowOperations
{
    [Description("Transposes a DataTable such that column 1 values replacing existing headers, only use this if you have been given a file in which proper headers are vertical down the first column and records are subsequent columns (i.e. adding new records results in the DataTable growing horizontally - some users of Excel think this is ideal way to create DataTables).  IMPORTANT: Only works with a single load batch if you have a chunked pipeline you cannot use this component unless you set the chunk size large enough to read the entire file in one go")]
    public class Transposer : IPluginDataFlowComponent<DataTable>
    {
        private bool _haveServedResult = false;

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.MakeHeaderNamesSane_DemandDescription,DemandType.Unspecified,true)]
        public bool MakeHeaderNamesSane { get; set; }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (toProcess == null)
                return null;
            
            if(_haveServedResult)
                throw new NotSupportedException("Error, we received multiple batches, Transposer only works when all the data arrives in a single DataTable");
            
            if(toProcess.Rows.Count == 0 || toProcess.Columns.Count == 0)
                throw new NotSupportedException("DataTable toProcess had " + toProcess.Rows.Count + " rows and " + toProcess.Columns.Count + " columns, thus it cannot be transposed");

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
            DataTable outputTable = new DataTable();

            // Add columns by looping rows

            // Header row's first column is same as in inputTable
            outputTable.Columns.Add(inputTable.Columns[0].ColumnName);

            // Header row's second column onwards, 'inputTable's first column taken
            foreach (DataRow inRow in inputTable.Rows)
            {
                string newColName = inRow[0].ToString();

                if (MakeHeaderNamesSane)
                    newColName = QuerySyntaxHelper.MakeHeaderNameSane(newColName);

                outputTable.Columns.Add(newColName);
            }

            // Add rows by looping columns        
            for (int rCount = 1; rCount <= inputTable.Columns.Count - 1; rCount++)
            {
                DataRow newRow = outputTable.NewRow();

                // First column is inputTable's Header row's second column
                newRow[0] = inputTable.Columns[rCount].ColumnName;
                for (int cCount = 0; cCount <= inputTable.Rows.Count - 1; cCount++)
                {
                    string colValue = inputTable.Rows[cCount][rCount].ToString();
                    newRow[cCount + 1] = colValue;
                }
                outputTable.Rows.Add(newRow);
            }

            return outputTable;
        }
    }
}