using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using HIC.Logging.PastEvents;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// Entry point for the RDMP relational logging database.  This class requires to be pointed at an existing logging database with the correct schema (Defined 
    /// in HIC.Logging.Database - See DatabaseCreation.exe for how to do this). See Logging.cd for the full hierarchy of concepts.
    /// 
    /// <para>You can both create new logging records and fetch old ones.  New logging objects are generally maintained for future use e.g. when you want to record
    /// that a new table is being loaded during a given load (DataLoadInfo) you must pass the load log object (DataLoadInfo).  Live logging objects generally
    /// must be closed to indicate that they are completed (succesfully or otherwise), if you do not close a logging object then the EndTime will be left
    /// blank and it will be unclear if a process blue screened or if it all went fine (other than the ongoing accumulation of log events, errors etc).</para>
    /// 
    /// <para>Fetching old records is done based on ID, Task Name etc and is also handled by this class. The objects returned will be ArchivalDataLoadInfo objects
    /// which are immutable and include the full hierarchy of sub concepts (errors, progress messages, which tables were loaded with how many records etc - 
    /// See Logging.cd).</para>
    /// </summary>
    public class LogManager : ILogManager
    {
        public DiscoveredServer Server { get; private set; }

        /// <summary>
        /// If the Server was set from a persistent database reference this property will store it e.g. a logging ExternalDatabaseServer
        /// </summary>
        public IDataAccessPoint DataAccessPointIfAny { get; private set; }

        public ProgressLogging ProgressLogging { get; private set; }
        private readonly FatalErrorLogging _fatalErrorLogging;
        private readonly RowErrorLogging _rowErrorLogging;
        
        public LogManager(DiscoveredServer server, ProgressLogging progressLogging = null, FatalErrorLogging fatalErrorLogging = null, RowErrorLogging rowErrorLogging = null)
        {
            Server = server;
            ProgressLogging = progressLogging ?? ProgressLogging.GetInstance();
            _fatalErrorLogging = fatalErrorLogging?? FatalErrorLogging.GetInstance();
            _rowErrorLogging = rowErrorLogging ?? RowErrorLogging.GetInstance();
        }

        public LogManager(IDataAccessPoint loggingServer) : this(DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging))
        {
            DataAccessPointIfAny = loggingServer;
        }

        public string[] ListDataTasks(bool hideTests=false)
        {
            using (var con = Server.GetConnection())
            {
                con.Open();
                DbCommand cmd = Server.GetCommand("SELECT * FROM DataLoadTask",con);

                DbDataReader r = cmd.ExecuteReader();

                List<string> tasks = new List<string>();
                while (r.Read())
                {
                    if (hideTests)
                    {
                        if(!(bool)r["isTest"])
                            tasks.Add(r["name"].ToString());
                        //else it is a test, and we are hidding them
                    }
                    else
                        tasks.Add(r["name"].ToString()); //we are not hiding tests
                }

                return tasks.ToArray();
            }
        }
        
        public DataTable GetTable(LoggingTables table, int? parentId, int? topX)
        {
            string prefix = "";
            string where = "";

            if (topX.HasValue)
                prefix = "TOP " + topX.Value;

            if(parentId.HasValue)
                switch (table)
                {
                    case LoggingTables.DataLoadTask:
                        break;
                    case LoggingTables.DataLoadRun:
                        where = "where dataLoadTaskID=" + parentId.Value;
                        break;

                     //all these have the parent of dataLoadRun
                    case LoggingTables.ProgressLog:
                    case LoggingTables.FatalError:
                    case LoggingTables.TableLoadRun:
                        where = "where dataLoadRunID=" + parentId.Value;
                        break;
                    case LoggingTables.DataSource:
                        where = "where tableLoadRunID=" + parentId.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("table");
                }

            return GetAsTable(string.Format("SELECT {0} * FROM " + table + " {1}", prefix, where));
        }
        
        private DataTable GetAsTable(string sql)
        {
            
            DataTable dt = new DataTable();

            using (var con = Server.GetConnection())
            {
                con.Open();

                DbCommand cmd = Server.GetCommand(sql, con);

                DbDataAdapter da = Server.GetDataAdapter(cmd);
                da.Fill(dt);
                
                return dt;
            }
        }


        public string[] ListDataSets()
        {
            using (var con = Server.GetConnection())
            {
                con.Open();

                DbCommand cmd = Server.GetCommand("SELECT * FROM DataSet", con);

                DbDataReader r = cmd.ExecuteReader();

                List<string> tasks = new List<string>();
                
                while (r.Read())
                    tasks.Add(r["dataSetID"].ToString());

                return tasks.ToArray();
            }
        }

        public DateTime? GetDateOfLastLoadAttemptForTask(string nameOfTask, bool onlyIfSuccessful)
        {
            var mostRecent = ArchivalDataLoadInfo.GetLoadHistoryForTask(nameOfTask, Server, true).SingleOrDefault();

            //never been loaded
            if (mostRecent == null)
                return null;
            
            //has unresolved errors
            if (mostRecent.HasUnresolvedErrors && onlyIfSuccessful)
                return null;

            return mostRecent.EndTime?? mostRecent.StartTime;
        }

        public event DataLoadInfoHandler DataLoadInfoCreated;

        public IDataLoadInfo CreateDataLoadInfo(string dataLoadTaskName, string packageName, string description, string suggestedRollbackCommand, bool isTest)
        {
            if(!ListDataTasks().Contains(dataLoadTaskName))
                throw new KeyNotFoundException("DataLoadTask called '" + dataLoadTaskName + "' was not found in the logging database " + Server);

            var toReturn = new DataLoadInfo(dataLoadTaskName, packageName, description, suggestedRollbackCommand, isTest, Server);

            if (DataLoadInfoCreated != null)
                DataLoadInfoCreated(this,toReturn);

            return toReturn;

        }
        /// <summary>
        /// Added so calling code is not dependent on the ProgressLogging singleton, which could later be factored into an injectable service
        /// </summary>
        public void LogProgress(IDataLoadInfo dataLoadInfo, ProgressLogging.ProgressEventType eventType, string source, string description)
        {
            ProgressLogging.LogProgress(dataLoadInfo, eventType, source, description);
        }

        /// <summary>
        /// Added so calling code is not dependent on the FatalErrorLogging singleton, which could later be factored into an injectable service
        /// </summary>
        public void LogFatalError(IDataLoadInfo dataLoadInfo, string errorSource, string errorDescription)
        {
            _fatalErrorLogging.LogFatalError(dataLoadInfo, errorSource, errorDescription);
        }

        /// <summary>
        /// Added so calling code is not dependent on the RowErrorLogging singleton, which could later be factored into an injectable service
        /// </summary>
        public void LogRowError(ITableLoadInfo tableLoadInfo, RowErrorLogging.RowErrorType typeOfError, string description, string locationOfRow, bool requiresReloading, string columnName = null)
        {
            _rowErrorLogging.LogRowError(tableLoadInfo, typeOfError, description, locationOfRow, requiresReloading, columnName);
        }

        public ArchivalDataLoadInfo GetLoadStatusOf(PastEventType mostRecent, string newLoggingDataTask)
        {
            return ArchivalDataLoadInfo.GetLoadStatusOf(mostRecent, newLoggingDataTask, Server);
        }

        public ArchivalDataLoadInfo GetLoadStatusOf(int dataLoadInfoID)
        {
            return ArchivalDataLoadInfo.GetLoadHistoryForTask(null,Server,false,null,dataLoadInfoID).SingleOrDefault();
        }

        public Dictionary<string, int?> GetIDOfLatestDataLoadRunForTasks()
        {
            var toReturn = new Dictionary<string, int?>();
            using (var con = Server.GetConnection())
            {
                con.Open();

                var r = Server.GetCommand(@"
 SELECT 
t.name,
max(r.ID) latestRunID
FROM
[DataLoadTask] t
left join
DataLoadRun  r on t.ID = r.dataLoadTaskID
group by
t.name", con).ExecuteReader();
                while (r.Read())
                    toReturn.Add(r["name"].ToString(), ObjectToNullableInt(r["latestRunID"]));
            }
            return toReturn;
        }

        public IEnumerable<ArchivalDataLoadInfo> GetArchivalLoadInfoFor(string task, CancellationToken token)
        {
            return ArchivalDataLoadInfo.GetLoadHistoryForTask(task, Server,false,token);
        }
        /// <summary>
        /// Creates a new data load task for the given dataset (datasetID which is the name of the dataset).  The loading task will be called the same as the dataset is called.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataSetID"></param>
        public void CreateNewLoggingTask(int id, string dataSetID)
        {
            using (var conn = Server.GetConnection())
            {
                conn.Open();
                {
                    var sql =
                        "INSERT INTO DataLoadTask (ID, description, name, createTime, userAccount, statusID, isTest, dataSetID) " +
                        "VALUES " +
                        "(" + id + ", @dataSetID, @dataSetID, GetDate(), @username, 1, 0, @dataSetID)";

                    var cmd = Server.GetCommand(sql, conn);
                    Server.AddParameterWithValueToCommand("@dataSetID",cmd,dataSetID);
                    Server.AddParameterWithValueToCommand("@username",cmd,Environment.UserName);
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CreateNewDataSet(string datasetName)
        {
            using (var conn = Server.GetConnection())
            {
                conn.Open();
                {
                    var sql =
                        "INSERT INTO DataSet (dataSetID,name) " +
                        "VALUES " +
                        "(@datasetName,@datasetName)";


                    var cmd =Server.GetCommand(sql, conn);
                    Server.AddParameterWithValueToCommand("@datasetName",cmd,datasetName);
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public void CreateNewLoggingTaskIfNotExists(string toCreate)
        {
            if(!ListDataSets().Contains(toCreate))
                CreateNewDataSet(toCreate);

            if(!ListDataTasks().Contains(toCreate))
                CreateNewLoggingTask(GetMaxTaskID()+1,toCreate);
        }

        private int GetMaxTaskID()
        {
            using (var conn = Server.GetConnection())
            {
                conn.Open();
                {
                    var sql =
                        "SELECT MAX(ID) FROM DataLoadTask";

                    var result = Server.GetCommand(sql, conn).ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return 0;

                    return int.Parse(result.ToString());
                }
            }
        }


        public void ResolveFatalErrors(int[] ids, FatalErrorLogging.FatalErrorStates newState, string newExplanation)
        {
            using (var conn = Server.GetConnection())
            {
                conn.Open();
                {
                    var sql =
                        "UPDATE [FatalError] SET explanation =@explanation, statusID=@statusID where ID in (" + string.Join(",", ids) + ")";

                    var cmd = Server.GetCommand(sql, conn);
                    Server.AddParameterWithValueToCommand("@explanation", cmd, newExplanation);
                    Server.AddParameterWithValueToCommand("@statusID", cmd, Convert.ToInt32(newState));
                    
                    int affectedRows = cmd.ExecuteNonQuery();

                    if(affectedRows != ids.Length)
                        throw new Exception("Query " + sql + " resulted in " + affectedRows + ", we were expecting there to be " + ids.Length + " updates because that is how many FatalError IDs that were passed to this method");
                }
            }
        }


        public void GetProgressMessageIDs(int message, out int task, out int run)
        {
            var dt = GetAsTable(@"select 
task.ID taskID,
run.ID runID
  FROM ProgressLog msg
  left join DataLoadRun run on run.ID = msg.dataLoadRunID
  left join DataLoadTask task on task.ID = run.dataLoadTaskID
  where msg.ID = " + message);

            if (dt.Rows.Count != 1)
                throw new Exception("Found " + dt.Rows.Count + " rows of IDs matching ProgressLog " + message);

            task = (int)dt.Rows[0]["taskID"];
            run = (int)dt.Rows[0]["runID"];
        }
        public void GetErrorIDs(int error, out int task, out int run)
        {
            var dt = GetAsTable(@"select 
task.ID taskID,
run.ID runID
  FROM FatalError err
  left join DataLoadRun run on run.ID = err.dataLoadRunID
  left join DataLoadTask task on task.ID = run.dataLoadTaskID
  where err.ID = " + error);

            if (dt.Rows.Count != 1)
                throw new Exception("Found " + dt.Rows.Count + " rows of IDs matching FatalError " + error);

            task = (int)dt.Rows[0]["taskID"];
            run = (int)dt.Rows[0]["runID"];
        }

        public void GetRunIDs(int run, out int task)
        {
            var dt = GetAsTable(@"select 
dataLoadTaskID  
FROM  DataLoadRun 
  where DataLoadRun.ID = " + run);

            if (dt.Rows.Count != 1)
                throw new Exception("Found " + dt.Rows.Count + " rows of IDs matching DataLoadRun " + run);

            task = (int)dt.Rows[0]["dataLoadTaskID"];

        }

        public void GetTableIDs(int table, out int task, out int run)
        {
                    var dt = GetAsTable(@"select 
task.ID taskID,
run.ID runID
  FROM TableLoadRun tbl
  left join DataLoadRun run on run.ID = tbl.dataLoadRunID
  left join DataLoadTask task on task.ID = run.dataLoadTaskID
  where tbl.ID = " + table);

            if(dt.Rows.Count != 1)
                throw new Exception("Found " + dt.Rows.Count + " rows of IDs matching TableLoadRun " + table);

            task = (int) dt.Rows[0]["taskID"];
            run = (int)dt.Rows[0]["runID"];
        
        }
        public void GetDataSourceIDs(int dataSource, out int task,out int run, out int table)
        {
            var dt = GetAsTable(@"select 
task.ID taskID,
run.ID runID,
tbl.ID tableID
  FROM [DataSource] ds
  left join TableLoadRun tbl on ds.tableLoadRunID = tbl.ID
  left join DataLoadRun run on run.ID = tbl.dataLoadRunID
  left join DataLoadTask task on task.ID = run.dataLoadTaskID
  where ds.ID = " + dataSource);

            if(dt.Rows.Count != 1)
                throw new Exception("Found " + dt.Rows.Count + " rows of IDs matching DataSource " + dataSource);

            task = (int) dt.Rows[0]["taskID"];
            run = (int)dt.Rows[0]["runID"];
            table = (int)dt.Rows[0]["tableID"];
        }

        public int? ObjectToNullableInt(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return int.Parse(o.ToString());
        }
    }
}
