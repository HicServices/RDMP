// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.Logging.PastEvents;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Logging
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
        
        /// <summary>
        /// Event triggered every time a new <see cref="IDataLoadInfo"/> is created.
        /// </summary>
        public event DataLoadInfoHandler DataLoadInfoCreated;

        public LogManager(DiscoveredServer server)
        {
            Server = server;
        }

        public LogManager(IDataAccessPoint loggingServer) : this(DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging))
        {
            DataAccessPointIfAny = loggingServer;
        }

        public string[] ListDataTasks(bool hideTests=false)
        {
            List<string> tasks = new List<string>();

            using (var con = Server.GetConnection())
            {
                con.Open();
                using (DbCommand cmd = Server.GetCommand("SELECT * FROM DataLoadTask", con))
                {
                    using(DbDataReader r = cmd.ExecuteReader())
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
                }

                return tasks.ToArray();
            }
        }

        /// <summary>
        /// Returns logging data for the given <paramref name="filter"/>
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="topX"></param>
        /// <param name="sortDesc">True to sort descending (highest ID first).  False to sort ascending (lowest ID first)</param>
        /// <returns></returns>
        public DataTable GetTable(LogViewerFilter filter, int? topX, bool sortDesc)
        {
            string prefix = "";
            string where = filter == null ? "": filter.GetWhereSql();

            if (topX.HasValue)
                prefix = "TOP " + topX.Value;
            
            return GetAsTable(string.Format("SELECT {0} * FROM " + filter.LoggingTable + " {1} ORDER BY ID " + (sortDesc? "Desc":"Asc"), prefix, where));
        }
        
        private DataTable GetAsTable(string sql)
        {
            DataTable dt = new DataTable();
            
            using (var con = Server.GetConnection())
            {
                con.Open();

                using(DbCommand cmd = Server.GetCommand(sql, con))
                    using(DbDataAdapter da = Server.GetDataAdapter(cmd))
                        da.Fill(dt);
                
                return dt;
            }
        }

        public string[] ListDataSets()
        {
            List<string> tasks = new List<string>();

            using (var con = Server.GetConnection())
            {
                con.Open();

                using(DbCommand cmd = Server.GetCommand("SELECT * FROM DataSet", con))
                    using(DbDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            tasks.Add(r["dataSetID"].ToString());

                return tasks.ToArray();
            }
        }

        /// <summary>
        /// Returns data load audit objects which describe runs of over arching task <paramref name="dataTask"/>
        /// </summary>
        /// <param name="dataTask"></param>
        /// <param name="token"></param>
        /// <param name="specificDataLoadRunIDOnly"></param>
        /// <param name="topX"></param>
        /// <returns></returns>
        public IEnumerable<ArchivalDataLoadInfo> GetArchivalDataLoadInfos(string dataTask, CancellationToken? token = null, int? specificDataLoadRunIDOnly = null, int? topX = null)
        {
            var db = Server.GetCurrentDatabase();
            var run = db.ExpectTable("DataLoadRun");
            
            using (var con = Server.GetConnection())
            {
                con.Open();

                var dataTaskId = GetDataTaskId(dataTask,Server, con);

                string where = "";
                string top = "";

                using (var cmd = Server.GetCommand("", con))
                {
                    if (topX != null)
                        top = "TOP " + topX.Value;

                    if (specificDataLoadRunIDOnly != null)
                        where = "WHERE ID=" + specificDataLoadRunIDOnly.Value;
                    else
                    {
                        where = "WHERE dataLoadTaskID = @dataTaskId";
                        var p = cmd.CreateParameter();
                        p.ParameterName = "@dataTaskId";
                        p.Value = dataTaskId;
                        cmd.Parameters.Add(p);
                    }

                    string sql = "SELECT " + top + " *, (select top 1 1 from FatalError where dataLoadRunID = DataLoadRun.ID) hasErrors FROM " + run.GetFullyQualifiedName() +" " + where + " ORDER BY ID desc";

                    cmd.CommandText = sql;

                    DbDataReader r;
                    if (token == null)
                        r = cmd.ExecuteReader();
                    else
                    {
                        Task<DbDataReader> rTask = cmd.ExecuteReaderAsync(token.Value);
                        rTask.Wait(token.Value);

                        if (rTask.IsCompleted)
                            r = rTask.Result;
                        else
                        {
                            cmd.Cancel();
                        
                            if (rTask.IsFaulted && rTask.Exception != null)
                                throw rTask.Exception.GetExceptionIfExists<Exception>() ?? rTask.Exception;

                            yield break;
                        }
                    }

                    using(r)
                        while (r.Read())
                            yield return new ArchivalDataLoadInfo(r, db);
                }
            }
        }

        private int GetDataTaskId(string dataTask, DiscoveredServer server, DbConnection con)
        {
            using (var cmd = server.GetCommand("SELECT ID FROM DataLoadTask WHERE name = @name", con))
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@name";
                p.Value = dataTask;
                cmd.Parameters.Add(p);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        

        public IDataLoadInfo CreateDataLoadInfo(string dataLoadTaskName, string packageName, string description, string suggestedRollbackCommand, bool isTest)
        {
            var task = ListDataTasks().FirstOrDefault(t=>t.Equals(dataLoadTaskName,StringComparison.CurrentCultureIgnoreCase));
            if(task == null)
                throw new KeyNotFoundException("DataLoadTask called '" + dataLoadTaskName + "' was not found in the logging database " + Server);

            var toReturn = new DataLoadInfo(task, packageName, description, suggestedRollbackCommand, isTest, Server);

            DataLoadInfoCreated?.Invoke(this,toReturn);

            return toReturn;

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
                        "(" + id + ", @dataSetID, @dataSetID, @date, @username, 1, 0, @dataSetID)";

                    using (var cmd = Server.GetCommand(sql, conn))
                    {
                        Server.AddParameterWithValueToCommand("@date", cmd,DateTime.Now);
                        Server.AddParameterWithValueToCommand("@dataSetID",cmd,dataSetID);
                        Server.AddParameterWithValueToCommand("@username",cmd,Environment.UserName);
                    
                        cmd.ExecuteNonQuery();
                    }
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

                    using (var cmd = Server.GetCommand(sql, conn))
                    {
                        Server.AddParameterWithValueToCommand("@datasetName",cmd,datasetName);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }



        public void CreateNewLoggingTaskIfNotExists(string toCreate)
        {
            if(!ListDataSets().Contains(toCreate,StringComparer.CurrentCultureIgnoreCase))
                CreateNewDataSet(toCreate);

            if(!ListDataTasks().Contains(toCreate,StringComparer.CurrentCultureIgnoreCase))
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

                    using (var cmd = Server.GetCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                            return 0;

                        return int.Parse(result.ToString());
                    }
                }
            }
        }

        public void ResolveFatalErrors(int[] ids, DataLoadInfo.FatalErrorStates newState, string newExplanation)
        {
            using (var conn = Server.GetConnection())
            {
                conn.Open();
                {
                    var sql =
                        "UPDATE FatalError SET explanation =@explanation, statusID=@statusID where ID in (" + string.Join(",", ids) + ")";

                    int affectedRows;

                    using (var cmd = Server.GetCommand(sql, conn))
                    {
                        Server.AddParameterWithValueToCommand("@explanation", cmd, newExplanation);
                        Server.AddParameterWithValueToCommand("@statusID", cmd, Convert.ToInt32(newState));
                        affectedRows = cmd.ExecuteNonQuery();
                    }
                    
                    if(affectedRows != ids.Length)
                        throw new Exception("Query " + sql + " resulted in " + affectedRows + ", we were expecting there to be " + ids.Length + " updates because that is how many FatalError IDs that were passed to this method");
                }
            }
        }

    }
}
