using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Progress;
using System;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.Data;
using System.Linq;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources
{
    class BatchExtractionSource : ExecuteDatasetExtractionSource 
    {



        private bool _usedBatching = false;

        public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        {
            if (Request is ExtractDatasetCommand edc)
            {
                // have we made any progress so far?
                var progress = Request.DataExportRepository.GetAllObjectsWithParent<ExtractionProgress>(edc.SelectedDataSets).SingleOrDefault();
                Request.IsBatchResume = progress == null;


                if(progress != null)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Found existing ExtractionProgress with progress of:{progress.ProgressDate ?? (object)"Null"}"));
                //    var startTime = (DateTime)progress.GetValueAsSystemType();
                  //  _usedBatching = GetBatchPeriod(_start = startTime, out _end);
                }
                else
                {
                    GetMinDate(listener);

                    _usedBatching = false;
                }

                // if we have decided batching won't work
                if(!_usedBatching)
                {
                    return base.HackExtractionSQL(sql, listener);
                }

                // TODO : hack the query
            }

            return base.HackExtractionSQL(sql, listener);
        }

        private void GetMinDate(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public override void Dispose(IDataLoadEventListener job, Exception pipelineFailureExceptionIfAny)
        {
            base.Dispose(job, pipelineFailureExceptionIfAny);
        }
    }
}
