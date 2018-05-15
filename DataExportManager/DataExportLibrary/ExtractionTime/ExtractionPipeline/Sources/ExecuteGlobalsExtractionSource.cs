using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataLoadEngine.DataFlowPipeline.Sources;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    public class ExecuteGlobalsExtractionSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<IExtractCommand>
    {
        private DbDataCommandDataFlowSource _hostedSource;
        private bool _testMode;
        private int _totalSqlChunks;
        private int _currentSqlChunk = 0;

        public ExtractGlobalsCommand Request { get; set; }

        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (Request == null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Component has not been initialized before being asked to GetChunk(s)"));
            
            Debug.Assert(Request != null, "Request != null");
            var sql = Request.Globals.SupportingSQL[_currentSqlChunk];
            //unless we are checking, start auditing
            // TODO: Setup auditing for Globals
            //if (!_testMode)
            //    StartAudit(Request.Globals.SupportingSQL[_currentSqlChunk]);

            //if (Request.DatasetBundle.DataSet.DisableExtraction)
            //    throw new Exception("Cannot extract " + Request.DatasetBundle.DataSet + " because DisableExtraction is set to true");

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "/*Running Globals SQL:*/" + Environment.NewLine + sql));

            _hostedSource = new DbDataCommandDataFlowSource(sql.SQL, "ExecuteGlobalsExtraction " + _currentSqlChunk, sql.GetServer().Builder, _testMode ? 30 : 50000);

            _hostedSource.AllowEmptyResultSets = false;
            _hostedSource.BatchSize = Int32.MaxValue;

            DataTable chunk = null;

            Thread t = new Thread(() =>
            {
                try
                {
                    chunk = _hostedSource.GetChunk(listener, cancellationToken);
                }
                catch (Exception e)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Read from source failed", e));
                }
            });
            t.Start();

            bool haveAttemptedCancelling = false;
            while (t.IsAlive)
            {
                if (cancellationToken.IsCancellationRequested && _hostedSource.cmd != null && !haveAttemptedCancelling)
                {
                    _hostedSource.cmd.Cancel(); //cancel the database command
                    haveAttemptedCancelling = true;
                }
                Thread.Sleep(100);
            }

            if (cancellationToken.IsCancellationRequested)
                throw new Exception("Data read cancelled because our cancellationToken was set, aborting data reading");

            //if the first chunk is null
            if (chunk == null)
                throw new Exception("There is no data to load, query returned no rows, query was:" + Environment.NewLine + sql.SQL);

            //data exhausted?
            _currentSqlChunk++;
            if (_currentSqlChunk == _totalSqlChunks)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, 
                                        "Globals Chunks: " + _currentSqlChunk + " done!"));
                return null;
            }
            
            //if it is test mode reset the host so it is ready to go again if called a second time
            if (_testMode)
                _hostedSource = null;
            
            return chunk;
        }

        private void Initialize(ExtractGlobalsCommand request)
        {
            Request = request;
            _totalSqlChunks = request.Globals.SupportingSQL.Count;
        }


        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public DataTable TryGetPreview()
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
        {
            if (value is ExtractGlobalsCommand)
                Initialize(value as ExtractGlobalsCommand);
        }
    }
}