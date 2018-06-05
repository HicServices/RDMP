using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CohortManagerLibrary.Execution
{
    /// <summary>
    /// An ongoing async execution of a cohort identification subquery in the CohortCompiler.  Includes the query used to fetch the cohort identifiers, the 
    /// identifiers themselves (once complete), cancellation token etc.
    /// </summary>
    public class CohortIdentificationTaskExecution
    {
        private readonly IDataAccessPoint _cacheServerIfAny;
        public int SubQueries { get; private set; }
        public int SubqueriesCached { get; private set; }

        public bool IsResultsForRootContainer { get; set; }

        public CohortIdentificationTaskExecution(IDataAccessPoint cacheServerIfAny, string countSQL, string cumulativeSQL, CancellationTokenSource cancellationTokenSource, int subQueries, int subqueriesCached, bool isResultsForRootContainer)
        {
            _cacheServerIfAny = cacheServerIfAny;
            SubQueries = subQueries;
            SubqueriesCached = subqueriesCached;
            CountSQL = countSQL;
            CumulativeSQL = cumulativeSQL;
            _cancellationTokenSource = cancellationTokenSource;
            IsResultsForRootContainer = isResultsForRootContainer;
        }

        public string CumulativeSQL { get; set; }

        /// <summary>
        /// Although this is called CountSQL it is actually a select distinct identifiers!
        /// </summary>
        public string CountSQL { get; set; }

        private CancellationTokenSource _cancellationTokenSource;
        private DbCommand _cmdCount;

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            if (_cmdCount != null && _cmdCount.Connection.State == ConnectionState.Open)
            {
                _cmdCount.Cancel();
            }
            
        }

        public DataTable Identifiers { get; private set; }
        public DataTable CumulativeIdentifiers { get; private set; }

        public bool IsExecuting { get; private set; }

        public void GetCohortAsync(IDataAccessPoint[] accessPoints, int commandTimeout)
        {
            if(Identifiers != null)
                throw new Exception("GetCohortAsync has already been called for this object");

            Identifiers = new DataTable();
            
            IsExecuting = true;

            var server = GetServerToExecuteQueryOn(accessPoints);
            
            server.EnableAsync();

            using (var con = server.GetConnection())
            {
                con.Open();
                _cmdCount = server.GetCommand(CountSQL, con);
                _cmdCount.CommandTimeout = commandTimeout;

                var identifiersReader = _cmdCount.ExecuteReaderAsync(_cancellationTokenSource.Token);

                identifiersReader.Wait(_cancellationTokenSource.Token);
                var rIds = identifiersReader.Result;
                Identifiers.Load(rIds);
                rIds.Close();
                rIds.Dispose();

                Task<DbDataReader> cumulativeIdentifiersRead = null;

                //if there is cumulative SQL happening
                if (!string.IsNullOrWhiteSpace(CumulativeSQL))
                {
                    CumulativeIdentifiers = new DataTable();

                    var cmdCountCumulative = server.GetCommand(CumulativeSQL, con);
                    cmdCountCumulative.CommandTimeout = commandTimeout;
                    cumulativeIdentifiersRead = cmdCountCumulative.ExecuteReaderAsync(_cancellationTokenSource.Token);
                }

                if (cumulativeIdentifiersRead != null)
                {
                    cumulativeIdentifiersRead.Wait(_cancellationTokenSource.Token);
                    var rCumulative = cumulativeIdentifiersRead.Result;
                    CumulativeIdentifiers.Load(rCumulative);
                    rCumulative.Close();
                    rCumulative.Dispose();
                }

                IsExecuting = false;
            }
        }

        private DiscoveredServer GetServerToExecuteQueryOn(IDataAccessPoint[] accessPoints)
        {
            //if all queries are cached then we should just execute against the cache
            if (SubQueries > 0 && SubQueries == SubqueriesCached && _cacheServerIfAny != null)
                return DataAccessPortal.GetInstance().ExpectServer(_cacheServerIfAny,DataAccessContext.InternalDataProcessing,false); 

            //if there are some cached but not all queries are cached
            if(SubqueriesCached > 0 && _cacheServerIfAny != null)
                return DataAccessPortal.GetInstance().ExpectDistinctServer(accessPoints.Union(new []{_cacheServerIfAny}).ToArray(), DataAccessContext.InternalDataProcessing, false);    

            return DataAccessPortal.GetInstance().ExpectDistinctServer(accessPoints, DataAccessContext.InternalDataProcessing, false);
        }
    }
}