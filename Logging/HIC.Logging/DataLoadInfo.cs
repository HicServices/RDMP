using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// Root object for an ongoing logged activity e.g. 'Loading Biochemistry'.  Includes the package name (exe or class name that is primarily responsible
    /// for the activity), start time, description etc.  You must call CloseAndMarkComplete once your activity is completed (whether it has failed or suceeded).
    /// 
    /// You should maintain a reference to DataLoadInfo in order to create logs of Progress (ProgressLogging) / Errors (FatalErrorLogging) / and table load audits
    /// (TableLoadInfo) (create these via the LogManager).  The ID property can be used if you want to reference this audit record e.g. when loading a live table
    /// you can store the ID of the load batch it appeared in. 
    /// </summary>
    public class DataLoadInfo : IDataLoadInfo
    {
        
        private bool _isClosed = false;
        private readonly string _packageName;
        private readonly string _userAccount;
        private readonly DateTime _startTime;
        private DateTime _endTime;
        private readonly string _description;
        private string _suggestedRollbackCommand;
        private int _id;
        private bool _isTest;



        private DiscoveredServer _server ;

        public DiscoveredServer DatabaseSettings
        {
            get
            {
                return _server;
            }
        }

        private object oLock = new object();
        
        
        #region Property setup (these throw exceptions if you try to read them after the record is closed)

       

        public string PackageName
        {
            get
            {
                return _packageName;
            }
        }

       
        public string UserAccount
        {
            get
            {
                return _userAccount;
            }
        }

        
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

       
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
        }

        
        public string Description
        {
            get
            {
                return _description;
            }
        }

        
        public string SuggestedRollbackCommand
        {
            get
            {
                return _suggestedRollbackCommand;
            }
        }

        
        public int ID
        {
            get
            {   
                return _id;
            }
        }

        public bool IsTest
        {
            get
            {
                return _isTest;
            }
        }


        public bool IsClosed
        {
            get { return _isClosed; }
        }
        #endregion

  
        /// <summary>
        /// Marks the start of a new data load in the database.  Automatically populates StartTime and UserAccount from
        /// Environment.  Also creates a new ID in the database.
        /// </summary>
        /// <param name="packageName">The SSIS package or executable that started the data load</param>
        /// <param name="description">A description of what the data load is trying to achieve</param>
        /// <param name="isTest">If true then the database record will be marked as Test=1</param>
        public DataLoadInfo(string dataLoadTaskName, string packageName, string description,string suggestedRollbackCommand,bool isTest,DiscoveredServer settings)
        {
            if(settings != null)
                _server = settings;

            _packageName = packageName;
            _userAccount = Environment.UserName;
            _description = description;
            _startTime = DateTime.Now;
            _suggestedRollbackCommand = suggestedRollbackCommand;
            
            _id = -1;
            _isTest = isTest;

            RecordNewDataLoadInDatabase(dataLoadTaskName);
        }

        private void RecordNewDataLoadInDatabase(string dataLoadTaskName)
        {
            int parentTaskID = -1;

            using (var con = (SqlConnection)_server.GetConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT ID FROM DataLoadTask WHERE name=@name", con);
                cmd.Parameters.Add("@name", SqlDbType.VarChar, 255);
                cmd.Parameters["@name"].Value = dataLoadTaskName;


                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    throw new Exception("Could not find data load task named:" + dataLoadTaskName);

                //ID can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
                parentTaskID = int.Parse(result.ToString());


                cmd = new SqlCommand(
                    @"INSERT INTO DataLoadRun (description,startTime,dataLoadTaskID,isTest,packageName,userAccount,suggestedRollbackCommand) VALUES (@description,@startTime,@dataLoadTaskID,@isTest,@packageName,@userAccount,@suggestedRollbackCommand);
SELECT SCOPE_IDENTITY();", con);

                cmd.Parameters.Add("@description", SqlDbType.VarChar, -1);
                cmd.Parameters.Add("@startTime", SqlDbType.DateTime);
                cmd.Parameters.Add("@dataLoadTaskID", SqlDbType.Int);
                cmd.Parameters.Add("@isTest", SqlDbType.Bit);
                cmd.Parameters.Add("@packageName", SqlDbType.VarChar, 100);
                cmd.Parameters.Add("@userAccount", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@suggestedRollbackCommand", SqlDbType.VarChar, -1);

                cmd.Parameters["@description"].Value = _description;
                cmd.Parameters["@startTime"].Value = _startTime;
                cmd.Parameters["@dataLoadTaskID"].Value = parentTaskID;
                cmd.Parameters["@isTest"].Value = _isTest;
                cmd.Parameters["@packageName"].Value = _packageName;
                cmd.Parameters["@userAccount"].Value = _userAccount;
                cmd.Parameters["@suggestedRollbackCommand"].Value = _suggestedRollbackCommand;

                //ID can come back as a decimal or an Int32 or an Int64 so whatever, just turn it into a string and then parse it
                _id = int.Parse(cmd.ExecuteScalar().ToString());
            }

        }

        /// <summary>
        /// Marks that the data load ended
        /// </summary>
        public void CloseAndMarkComplete()
        {
            lock (oLock)
            {
                //prevent double closing
                if (_isClosed)
                    return; 
            
                _endTime = DateTime.Now;

                using (var con = _server.BeginNewTransactedConnection())
                {
                    try
                    {

                        DbCommand cmdUpdateToClosed =
                            _server.GetCommand("UPDATE DataLoadRun SET endTime=@endTime WHERE ID=@ID",
                                con);

                        _server.AddParameterWithValueToCommand("@endTime", cmdUpdateToClosed,DateTime.Now);
                        _server.AddParameterWithValueToCommand("@ID", cmdUpdateToClosed, ID);
                        
                        int rowsAffected = cmdUpdateToClosed.ExecuteNonQuery();

                        if (rowsAffected != 1)
                            throw new Exception(
                                "Error closing off DataLoad in database, the update command resulted in " +
                                rowsAffected + " rows being affected (expected 1) - will try to rollback");

                        con.ManagedTransaction.CommitAndCloseConnection();

                        _isClosed = true;
                    }
                    catch (Exception)
                    {
                        //if something goes wrong with the update, roll it back
                        con.ManagedTransaction.AbandonAndCloseConnection();

                        throw;
                    }

                    //once a record has been commited to the database it is redundant and no further attempts to read/change it should be made by anyone
                    foreach (TableLoadInfo t in this.TableLoads.Values)
                    {
                        //close any table loads that have not yet completed
                        if (!t.IsClosed)
                            t.CloseAndArchive();
                    }
                }
            }
        }


        private Dictionary<int, TableLoadInfo> _TableLoads = new Dictionary<int, TableLoadInfo>();

        public Dictionary<int, TableLoadInfo> TableLoads
        {
            get
            {
                return _TableLoads;
            }
        }

        public ITableLoadInfo CreateTableLoadInfo(string suggestedRollbackCommand, string destinationTable, DataSource[] sources, int expectedInserts)
        {
            return new TableLoadInfo(this, suggestedRollbackCommand, destinationTable, sources, expectedInserts);
        }

        public static DataLoadInfo Empty= new DataLoadInfo();

        private DataLoadInfo()
        {
        }
    }
}
