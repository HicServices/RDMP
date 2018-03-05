using System;
using System.Data;
using System.Data.SqlClient;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// Static class for logging progress messages during a logged activity (See DataLoadRun / Logging.cd).
    /// </summary>
    public class ProgressLogging : IProgressLogging
    {
        private static ProgressLogging _instance;
        private static readonly object instanceLock = new object();

        private DiscoveredServer _databaseSettings ;

        internal DiscoveredServer DatabaseSettings
        {
            get { return _databaseSettings; }
        }


        object oLock = new object();

        private ProgressLogging()
        {
            
        }

        /// <summary>
        /// Get a reference to the singleton RowErrorLogging class instance, If you need to change the database settings used by this class, use DatabaseSettings property
        /// </summary>
        /// <returns></returns>
        public static ProgressLogging GetInstance()
        {
            //lock so that we do not call the constructor more than once (if multi threading)
            lock (instanceLock)
                if (_instance == null)
                    _instance = new ProgressLogging();
            
            return _instance;
        }

        public enum ProgressEventType
        {
            OnInformation,
            OnProgress,
            OnQueryCancel,
            OnTaskFailed,
            OnWarning
        }
  
        public void LogProgress(IDataLoadInfo dataLoadInfo, ProgressEventType pevent, string Source, string Description)
        {
            _databaseSettings = dataLoadInfo.DatabaseSettings;
            lock (oLock)
            {
                using (var con = (SqlConnection)_databaseSettings.GetConnection())
                using (var cmdRecordProgress = new SqlCommand("INSERT INTO ProgressLog " +
                                                                "(dataLoadRunID,eventType,source,description,time) " +
                                                                "VALUES (@dataLoadRunID,@eventType,@source,@description,@time);", con))
                {
                    con.Open();

                    cmdRecordProgress.Parameters.Add("@dataLoadRunID", SqlDbType.Int);
                    cmdRecordProgress.Parameters.Add("@eventType", SqlDbType.VarChar, 50);
                    cmdRecordProgress.Parameters.Add("@source", SqlDbType.VarChar, 100);
                    cmdRecordProgress.Parameters.Add("@description", SqlDbType.VarChar, 8000);
                    cmdRecordProgress.Parameters.Add("@time", SqlDbType.DateTime);

                    cmdRecordProgress.Parameters["@dataLoadRunID"].Value = dataLoadInfo.ID;
                    cmdRecordProgress.Parameters["@eventType"].Value = pevent.ToString();
                    cmdRecordProgress.Parameters["@source"].Value = Source;
                    cmdRecordProgress.Parameters["@description"].Value = Description;
                    cmdRecordProgress.Parameters["@time"].Value = DateTime.Now;

                    cmdRecordProgress.ExecuteNonQuery();
                }
            }
        }
    }
}
