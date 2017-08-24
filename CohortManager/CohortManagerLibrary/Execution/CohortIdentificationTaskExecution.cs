using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CohortManagerLibrary.Execution
{
    public class CohortIdentificationTaskExecution
    {
        public int SubQueries { get; private set; }
        public int SubqueriesCached { get; private set; }

        public bool IsResultsForRootContainer { get; set; }

        public CohortIdentificationTaskExecution(string countSQL, string sampleSQL, string cumulativeSQL, CancellationTokenSource cancellationTokenSource, int subQueries, int subqueriesCached, bool isResultsForRootContainer)
        {
            SubQueries = subQueries;
            SubqueriesCached = subqueriesCached;
            CountSQL = countSQL;
            SampleSQL = sampleSQL;
            CumulativeSQL = cumulativeSQL;
            CancellationTokenSource = cancellationTokenSource;
            IsResultsForRootContainer = isResultsForRootContainer;
        }

        public string CumulativeSQL { get; set; }

        /// <summary>
        /// Although this is called CountSQL it is actually a select distinct identifiers!
        /// </summary>
        public string CountSQL { get; set; }
        public string SampleSQL { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public DataTable Preview { get; private set; }
        public DataTable Identifiers { get; private set; }
        public DataTable CumulativeIdentifiers { get; private set; }

        public bool IsExecuting { get; set; }

        public void GetCohortAsync(IDataAccessPoint[] accessPoints, int commandTimeout)
        {
            if(Identifiers != null)
                throw new Exception("GetCohortAsync has already been called for this object");

            Identifiers = new DataTable();
            
            var server = DataAccessPortal.GetInstance().ExpectDistinctServer(accessPoints, DataAccessContext.InternalDataProcessing, false);

            server.EnableAsync();

            using (var con = server.GetConnection())
            {
                con.Open();
                var cmdCount = server.GetCommand(CountSQL, con);
                cmdCount.CommandTimeout = commandTimeout;
                
                Task<DbDataReader> cumulativeIdentifiersRead = null;
                
                var identifiersReader =  cmdCount.ExecuteReaderAsync(CancellationTokenSource.Token);

                //if there is cumulative SQL happening
                if (!string.IsNullOrWhiteSpace(CumulativeSQL))
                {
                    CumulativeIdentifiers = new DataTable();

                    var cmdCountCumulative = server.GetCommand(CumulativeSQL, con);
                    cmdCountCumulative.CommandTimeout = commandTimeout;
                    cumulativeIdentifiersRead = cmdCountCumulative.ExecuteReaderAsync(CancellationTokenSource.Token);
                }

                if(!string.IsNullOrWhiteSpace(SampleSQL))
                {
                    var cmdPreview = server.GetCommand(SampleSQL, con);
                    cmdPreview.CommandTimeout = commandTimeout;

                    var datasetSampleReader = cmdPreview.ExecuteReaderAsync(CancellationTokenSource.Token);
                    IsExecuting = true;
                    datasetSampleReader.Wait(CancellationTokenSource.Token);
                    
                    Preview = new DataTable();
                    Preview.Load(datasetSampleReader.Result);
                }

                identifiersReader.Wait(CancellationTokenSource.Token);
                Identifiers.Load(identifiersReader.Result);

                if (cumulativeIdentifiersRead != null)
                {
                    cumulativeIdentifiersRead.Wait(CancellationTokenSource.Token);
                    CumulativeIdentifiers.Load(cumulativeIdentifiersRead.Result);
                }

                IsExecuting = false;
            }
        }
    }
}