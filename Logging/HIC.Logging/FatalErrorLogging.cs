using System;
using System.Data;
using System.Data.SqlClient;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// Static class for logging fatal exceptions that have crashed / ended a logged activity (See DataLoadRun / Logging.cd)
    /// </summary>
    public class FatalErrorLogging
    {
        private static FatalErrorLogging _instance;
        private static readonly object instanceLock = new object();

        private DiscoveredServer _databaseSettings;

        internal DiscoveredServer DatabaseSettings
        {
            get { return _databaseSettings; }
        }


        private object oLock = new object();

        private FatalErrorLogging()
        {
        }

        /// <summary>
        /// Get a reference to the singleton RowErrorLogging class instance, If you need to change the database settings used by this class, use DatabaseSettings property
        /// </summary>
        /// <returns></returns>
        public static FatalErrorLogging GetInstance()
        {
            //lock so that we do not call the constructor more than once (if multi threading)
            lock (instanceLock)
                if (_instance == null)
                    _instance = new FatalErrorLogging();
            
            return _instance;
        }

        public enum FatalErrorStates
        {
            Outstanding = 1,
            Resolved = 2,
            Blocked = 3
        }

        

        /// <summary>
        /// Terminates the current DataLoadInfo and records that it resulted in a fatal error
        /// </summary>
        /// <param name="dataLoadInfo">The data load that has just failed</param>
        /// <param name="errorSource">The component that generated the failure(in SSIS try System::SourceName)</param>
        /// <param name="errorDescription">A description of the error (in SSIS try System::ErrorDescription)</param>
        public void LogFatalError(IDataLoadInfo dataLoadInfo, string errorSource, string errorDescription)
        {
            _databaseSettings = dataLoadInfo.DatabaseSettings;

            lock (oLock)
            {
                using (var con = _databaseSettings.GetConnection())
                {
                    con.Open();

                    //look up the fatal error ID (get hte name of the Enum so that we can refactor if nessesary without breaking the code looking for a constant string)
                    string initialErrorStatus = Enum.GetName(typeof(FatalErrorStates), FatalErrorStates.Outstanding);

                    SqlCommand cmdLookupStatusID = new SqlCommand("SELECT ID from z_FatalErrorStatus WHERE status=@status", (SqlConnection) con);
                    cmdLookupStatusID.Parameters.Add("@status", SqlDbType.NChar, 20);
                    cmdLookupStatusID.Parameters["@status"].Value = initialErrorStatus;

                    int statusID = int.Parse(cmdLookupStatusID.ExecuteScalar().ToString());

                    SqlCommand cmdRecordFatalError = new SqlCommand(
        @"INSERT INTO FatalError (time,source,description,statusID,dataLoadRunID) VALUES (@time,@source,@description,@statusID,@dataLoadRunID);", (SqlConnection)con);
                    cmdRecordFatalError.Parameters.Add("@time", SqlDbType.DateTime);
                    cmdRecordFatalError.Parameters.Add("@source", SqlDbType.VarChar, 50);
                    cmdRecordFatalError.Parameters.Add("@description", SqlDbType.VarChar, -1);
                    cmdRecordFatalError.Parameters.Add("@statusID", SqlDbType.Int);
                    cmdRecordFatalError.Parameters.Add("@dataLoadRunID", SqlDbType.Int);

                    cmdRecordFatalError.Parameters["@time"].Value = DateTime.Now;
                    cmdRecordFatalError.Parameters["@source"].Value = errorSource;
                    cmdRecordFatalError.Parameters["@description"].Value = errorDescription;
                    cmdRecordFatalError.Parameters["@statusID"].Value = statusID;
                    cmdRecordFatalError.Parameters["@dataLoadRunID"].Value = dataLoadInfo.ID;

                    cmdRecordFatalError.ExecuteNonQuery();

                    //this might get called multiple times (many errors in rapid succession as the program crashes) but only close the dataLoadInfo once
                    if (!dataLoadInfo.IsClosed)
                        dataLoadInfo.CloseAndMarkComplete();

                }
            }
        }
    }
}
