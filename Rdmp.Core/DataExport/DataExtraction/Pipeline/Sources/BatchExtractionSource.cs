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

        [DemandsInitialization("The period to execute extraction for on one go.  See also PeriodValue")]
        public BatchPeriod PeriodUnit { get; set; }
        [DemandsInitialization("The period to execute extraction for", DefaultValue = 1)]
        public int PeriodValue { get; set; } = 1;

        public enum BatchPeriod
        {
            Day,
            Month,
            Year
        }

        private bool _usedBatching = false;
        private DateTime _start;
        private DateTime _end;


        public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        {
            if (Request is ExtractDatasetCommand edc)
            {
                // have we made any progress so far?
                var progress = Request.DataExportRepository.GetAllObjectsWithParent<ExtractionProgress>(edc.SelectedDataSets).SingleOrDefault();
                Request.IsBatchResume = progress == null;


                if(progress != null)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Found existing ExtractionProgress with progress of:{progress.Progress ?? (object)"Null"}"));
                //    var startTime = (DateTime)progress.GetValueAsSystemType();
                  //  _usedBatching = GetBatchPeriod(_start = startTime, out _end);
                }
                else
                {
                    GetMinDate(listener);

                    _usedBatching = GetBatchPeriod(out _start, out _end);
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

        /// <summary>
        /// Returns the first batch period that should be run based on system state.  If you have
        /// already loaded some data and know where to restart from then use the overload
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool GetBatchPeriod(out DateTime start, out DateTime end)
        {
            start = DateTime.MinValue;
            return GetBatchPeriod(start, out end);
        }

        /// <summary>
        /// Calculates based on an inclusive <paramref name="start"/> date what the exclusive end date
        /// should be for the batch <paramref name="end"/>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool GetBatchPeriod(DateTime start, out DateTime end)
        {
            switch (PeriodUnit)
            {
                case BatchPeriod.Day:
                    end = start.AddDays(PeriodValue);
                    break;
                case BatchPeriod.Month:
                    end = start.AddMonths(PeriodValue);
                    break;
                case BatchPeriod.Year:
                    end = start.AddYears(PeriodValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }
        public override void Dispose(IDataLoadEventListener job, Exception pipelineFailureExceptionIfAny)
        {
            base.Dispose(job, pipelineFailureExceptionIfAny);
        }
    }
}
