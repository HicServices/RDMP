using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Attachers;
using DataLoadEngine.Job;
using Renci.SshNet.Security;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Attachers
{
    public class KVPAttacher :FlatFileAttacher, IDemandToUseAPipeline<DataTable>, IDataFlowDestination<DataTable>
    {
        [DemandsInitialization("Pipeline for reading from the flat file",Mandatory = true)]
        public Pipeline PipelineForReadingFromFlatFile { get; set; }

        List<DataTable> BatchesReadyForProcessing = new List<DataTable>();

        [DemandsInitialization("A comma separated list of column names that make up the primary key of the KVP table (in most cases this is only one column).  These must appear in the file",Mandatory = true)]
        public string PrimaryKeyColumns { get; set; }

        [DemandsInitialization("The name of the column in RAW which will store the Key component of the KeyValuePair relationship e.g. 'Key', 'Test' or 'Attribute' etc",Mandatory = true)]
        public string TargetDataTableKeyColumnName { get; set; }

        [DemandsInitialization("The name of the column in RAW which will store the Value component of the KeyValuePair relationship e.g. 'Value', 'Result' or 'Val' etc",Mandatory = true)]
        public string TargetDataTableValueColumnName { get; set; }

        #region Attacher Functionality
        protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener)
        {
            if(BatchesReadyForProcessing.Any())
                throw new NotSupportedException("There are still batches awaiting dispatch to RAW, we cannot open a new file at this time");
            
            var flatFileToLoad = new FlatFileToLoad(fileToLoad);

            //stamp out the pipeline into an instance
            var mef = ((CatalogueRepository)PipelineForReadingFromFlatFile.Repository).MEF;
            var context = GetContext();
            var factory = new DataFlowPipelineEngineFactory<DataTable>(mef, context);
            factory.ExplicitDestination = this;
            var dataFlow = factory.Create(PipelineForReadingFromFlatFile,listener);
            dataFlow.Initialize(flatFileToLoad);

            //will result in the opening and processing of the file and the passing of DataTables through the Pipeline finally arriving at the destination (us) in ProcessPipelineData
            dataFlow.ExecutePipeline(new GracefulCancellationToken());

        }

        protected override void CloseFile()
        {
            
        }

        protected override void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job)
        {

            string[] pks = GetPKs();

            //make sure the primary key columns are in all the relevant tables
            foreach (string pk in pks)
            {
                if (!loadTarget.Columns.Contains(pk))
                    throw new KeyNotFoundException("Could not find a column called " + pk + " (part of the PrimaryKey) on destination table " + TableName);

                foreach (DataTable batchTable in BatchesReadyForProcessing)
                    if (!batchTable.Columns.Contains(pk))
                        throw new KeyNotFoundException(
                            "Source Batch DataTable (read from Pipeline) was missing column " + pk +
                            " (columns in DataTable were:" +
                            string.Join(",", batchTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
            }

            if (!loadTarget.Columns.Contains(TargetDataTableKeyColumnName))
                throw new KeyNotFoundException("Target destination table " + TableName + " did not contain a column called '" + TargetDataTableKeyColumnName + "' which is where we were told to store the Keys of the Key value pairs (note that the Key the column that matches the value not the primary key columns which are separate)");

            if (!loadTarget.Columns.Contains(TargetDataTableValueColumnName))
                throw new KeyNotFoundException("Target destination table " + TableName + " did not contain a column called '" + TargetDataTableValueColumnName + "' which is where we were told to store the Value of the Key value pairs");

        }
        
        protected override int IterativelyBatchLoadDataIntoDataTable(DataTable dt, int maxBatchSize)
        {
            //there are no batches for processing
            if (!BatchesReadyForProcessing.Any())
                return 0;

            string[] pks = GetPKs();

            //handle batch 0
            var currentBatch = BatchesReadyForProcessing[0];

            int recordsGenerated = 0;

            foreach (DataRow batchRow in currentBatch.Rows)
            {
                Dictionary<string,object> pkValues = new Dictionary<string, object>();
                
                foreach(string pk in pks)
                    pkValues.Add(pk,batchRow[pk]);

                foreach (DataColumn col in currentBatch.Columns)
                {
                    if(pks.Contains(col.ColumnName))
                        continue;//it's a primary key column

                    var k = col.ColumnName;
                    var val = batchRow[k];

                    var newRow = dt.Rows.Add();
                    foreach (string pk in pks)
                        newRow[pk] = pkValues[pk];
                    newRow[TargetDataTableKeyColumnName] = k;
                    newRow[TargetDataTableValueColumnName] = val;

                    recordsGenerated++;
                }
            }

            BatchesReadyForProcessing.Remove(currentBatch);

            return recordsGenerated;
        }

        private string[] GetPKs()
        {
            if (string.IsNullOrWhiteSpace(PrimaryKeyColumns))
                return new string[0];

            return PrimaryKeyColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        #region IDataFlowDestination Members
         
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
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

        #region Pipeline Demands
        public DataFlowPipelineContext<DataTable> GetContext()
        {
            var context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.FixedDestination);
            context.MustHaveSource = typeof (IDataFlowSource<DataTable>);
            
            return context;
        }

        public IDataFlowSource<DataTable> GetFixedSourceIfAny()
        {
            return null;
        }

        public IDataFlowDestination<DataTable> GetFixedDestinationIfAny()
        {
            return this;
        }

        public List<object> GetInputObjectsForPreviewPipeline()
        {
            return new List<object>(new Object[]{new FlatFileToLoad(null)});
        }
        #endregion

        public override void Check(ICheckNotifier notifier)
        {
            base.Check(notifier);

            if (string.IsNullOrWhiteSpace(PrimaryKeyColumns))
                notifier.OnCheckPerformed(new CheckEventArgs("Argument PrimaryKeyColumns has not been set",CheckResult.Fail));

            var pks = GetPKs();
            
            if (string.IsNullOrWhiteSpace(TargetDataTableKeyColumnName))
                notifier.OnCheckPerformed(new CheckEventArgs("Argument TargetDataTableKeyColumnName has not been set", CheckResult.Fail));
            
            if (string.IsNullOrWhiteSpace(TargetDataTableValueColumnName))
                notifier.OnCheckPerformed(new CheckEventArgs("Argument TargetDataTableValueColumnName has not been set", CheckResult.Fail));

            string duplicate = pks.FirstOrDefault(s => s.Equals(TargetDataTableKeyColumnName) || s.Equals(TargetDataTableValueColumnName));

            if (duplicate != null)
                notifier.OnCheckPerformed(new CheckEventArgs("Field '" + duplicate + "' is both a PrimaryKeyColumn and a TargetDataTable column, this is not allowed.  Your fields Pk1,Pk2,Pketc,Key,Value must all be mutually exclusive", CheckResult.Fail));

            if (TargetDataTableKeyColumnName != null && TargetDataTableKeyColumnName.Equals(TargetDataTableValueColumnName))
                notifier.OnCheckPerformed(new CheckEventArgs("TargetDataTableKeyColumnName cannot be the same as TargetDataTableValueColumnName",CheckResult.Fail));

        }
    }
}
