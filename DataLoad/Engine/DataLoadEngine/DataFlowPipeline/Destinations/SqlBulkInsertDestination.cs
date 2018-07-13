using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Migration;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataFlowPipeline.Destinations
{
    /// <summary>
    /// Bulk inserts data into an (already existing) table, one chunk at a time
    /// </summary>
    public class SqlBulkInsertDestination : IDataFlowDestination<DataTable>, IPipelineRequirement<ITableLoadInfo>
    {
        protected readonly string Table;
        private readonly List<string> _columnNamesToIgnore;
        private string _taskBeingPerformed;

        public const int Timeout = 5000;

        private IBulkCopy _copy = null;
        private Stopwatch _timer = new Stopwatch();

        protected ITableLoadInfo TableLoadInfo;
        private DiscoveredDatabase _dbInfo;

        private DiscoveredTable _table;

        public SqlBulkInsertDestination(DiscoveredDatabase dbInfo, string tableName, IEnumerable<string> columnNamesToIgnore)
        {
            _dbInfo = dbInfo;
            
            if(string.IsNullOrWhiteSpace(tableName))
                throw new Exception("Parameter tableName is not specified for SqlBulkInsertDestination");

            Table = RDMPQuerySyntaxHelper.EnsureValueIsWrapped(tableName);
            _table = _dbInfo.ExpectTable(tableName);
            _columnNamesToIgnore = columnNamesToIgnore.ToList();
            _taskBeingPerformed = "Bulk insert into " + Table + "(server=" + _dbInfo.Server + ",database=" + _dbInfo.GetRuntimeName() + ")";
        }


        private int _recordsWritten = 0;

        public virtual void SubmitChunk(DataTable chunk, IDataLoadEventListener job)
        {
            _timer.Start();
            if(_copy == null)
            {
                _copy = InitializeBulkCopy(chunk,job);
                AssessMissingAndIgnoredColumns(chunk,job);
            }

            _copy.Upload(chunk);
       
            _timer.Stop();
            RaiseEvents(chunk,job);
        }

        protected void AssessMissingAndIgnoredColumns(DataTable chunk, IDataLoadEventListener job)
        {
            DiscoveredColumn[] listColumns = _dbInfo.ExpectTable(Table).DiscoverColumns();
            bool problemsWithColumnSets = false;

            foreach (DataColumn colInSource in chunk.Columns)
                if (!listColumns.Any(c=>c.GetRuntimeName().Equals(colInSource.ColumnName,StringComparison.CurrentCultureIgnoreCase)))//there is something wicked this way coming, down the pipeline but not in the target table
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                        "Column " + colInSource.ColumnName + " appears in pipeline but not destination table (" + Table + ") which is on (Database=" + _dbInfo.GetRuntimeName() + ",Server=" + _dbInfo.Server + ")"));

                    problemsWithColumnSets = true;
                }
            foreach (DiscoveredColumn columnInDestination in listColumns)
                if (columnInDestination.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID) ||
                    columnInDestination.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom))
                    //its fine if validFrom/DataLoadRunID columns are missing
                    continue;//its fine
                else
                    if (!chunk.Columns.Contains(columnInDestination.GetRuntimeName()))//its not fine if there are other columns missing (at the very least we should warn the user.
                    {
                        bool isBigProblem = !columnInDestination.GetRuntimeName().StartsWith("hic_");

                        job.OnNotify(this, 
                            new NotifyEventArgs(isBigProblem?ProgressEventType.Error:ProgressEventType.Warning, //hic_ columns could be ok if missing so only warning, otherwise go error
                                "Column " + columnInDestination.GetRuntimeName() + " appears in destination table ("+Table+") but is not in the pipeline (will probably be left as NULL)"));

                        if (isBigProblem)
                            problemsWithColumnSets = true;
                    }

            if(problemsWithColumnSets)
                throw new Exception("There was a mismatch between the columns in the pipeline and the destination table, check earlier progress messages for details on the missing columns");
        }

        private void RaiseEvents(DataTable chunk, IDataLoadEventListener job)
        {
            if (chunk != null)
            {

                _recordsWritten += chunk.Rows.Count;
                
                if (TableLoadInfo != null)
                    TableLoadInfo.Inserts = _recordsWritten;
            }

            job.OnProgress(this,new ProgressEventArgs(_taskBeingPerformed,new ProgressMeasurement(_recordsWritten,ProgressType.Records),_timer.Elapsed));
        }

        private IBulkCopy InitializeBulkCopy(DataTable dt, IDataLoadEventListener job)
        {
            var insert = _table.BeginBulkInsert();
            insert.Timeout = Timeout;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                "SqlBulkCopy to " + _dbInfo.Server + ", " + _dbInfo.GetRuntimeName() + ".." + Table + " initialised for " + dt.Columns.Count + " columns, with a timeout of " + Timeout + ".  First chunk received had rowcount of " + dt.Rows.Count));

            return insert;
        }


        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            CloseConnection(listener);
        }

        public void Abort(IDataLoadEventListener listener)
        {
            CloseConnection(listener);
        }

        private bool _isDisposed = false;
        

        private void CloseConnection(IDataLoadEventListener listener)
        {
            if(_isDisposed)
                return;

            _isDisposed = true;
            try
            {
                if (TableLoadInfo != null)
                    TableLoadInfo.CloseAndArchive();

                if (_copy != null)
                    _copy.Dispose();

                if (_recordsWritten == 0)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Warning, 0 records written by SqlBulkInsertDestination (" + _dbInfo.Server + "," + _dbInfo.GetRuntimeName()+ ")"));
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Could not close connection to server", e));
            }

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "SqlBulkCopy closed after writing " + _recordsWritten + " rows to the server.  Total time spent writing to server:" + _timer.Elapsed));
        }

        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            SubmitChunk(toProcess,listener);
            return null;
        }

        public void PreInitialize(ITableLoadInfo value, IDataLoadEventListener listener)
        {
            this.TableLoadInfo = value;
        }
    }
}
