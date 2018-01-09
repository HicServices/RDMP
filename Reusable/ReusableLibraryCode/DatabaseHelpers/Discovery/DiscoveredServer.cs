using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Threading;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type reference to a database server.  Allows you to get connections, create commands, list databases etc.
    /// </summary>
    public class DiscoveredServer : IMightNotExist
    {
        public DbConnectionStringBuilder Builder { get; set; }
        public IDiscoveredServerHelper Helper { get; set; }
        public DatabaseType DatabaseType { get { return Helper.DatabaseType; }}

        public DiscoveredServer(DbConnectionStringBuilder builder)
        {
            Helper = new DatabaseHelperFactory(builder).CreateInstance();

            //give helper a chance to mutilate the builder if he wants (also gives us a new copy of the builder incase anyone external modifies the old reference)
            Builder = Helper.GetConnectionStringBuilder(builder.ConnectionString);
        }
        
        public DbConnection GetConnection(IManagedTransaction transaction = null)
        {
            if (transaction != null)
                return transaction.Connection;

            return Helper.GetConnection(Builder);
        }

        public DbCommand GetCommand(string sql, IManagedConnection managedConnection)
        {
            var cmd = Helper.GetCommand(sql, managedConnection.Connection);
            cmd.Transaction = managedConnection.Transaction;
            return cmd;
        }

        public DbCommand GetCommand(string sql, DbConnection con, IManagedTransaction transaction = null)
        {
            var cmd = Helper.GetCommand(sql, con);

            if (transaction != null)
                cmd.Transaction = transaction.Transaction;

            return cmd;
        }

        public DbParameter GetParameter(string parameterName, DbCommand forCommand)
        {
            return Helper.GetParameter(parameterName);
        }

        public void AddParameterWithValueToCommand(string parameterName, DbCommand command, object valueForParameter)
        {
            DbParameter dbParameter = GetParameter(parameterName, command);
            dbParameter.Value = valueForParameter;
            command.Parameters.Add(dbParameter);
        }

        public DiscoveredDatabase ExpectDatabase(string database)
        {
            var builder = Helper.ChangeDatabase(Builder, database);
            var server = new DiscoveredServer(builder);
            return new DiscoveredDatabase(server, database, Helper.GetQuerySyntaxHelper());
        }

        public void TestConnection(int timeoutInMillis = 3000)
        {
            using (var con = Helper.GetConnection(Builder))
            { 
                var openTask = con.OpenAsync(new CancellationTokenSource(timeoutInMillis).Token);
                try
                {
                    openTask.Wait();
                }
                catch (AggregateException e)
                {
                    if(openTask.IsCanceled)
                        throw new TimeoutException(string.Format("Could not connect to server '"+Name+"' after timeout of {0} milliseconds)", timeoutInMillis),e);

                    throw;
                }
                    

                con.Close();
            }
        }

        public DiscoveredDatabase[] DiscoverDatabases()
        {
            var toreturn = new List<DiscoveredDatabase>();

            foreach (string database in Helper.ListDatabases(Builder))
                toreturn.Add(new DiscoveredDatabase(this, database,Helper.GetQuerySyntaxHelper()));
            

            return toreturn.ToArray();
        }

        public bool Exists(IManagedTransaction transaction = null)
        {
            if (transaction != null)
                return true;

            try
            {
                TestConnection();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return Helper.GetServerName(Builder); 
            }
        }

        public string ExplicitUsernameIfAny { get { return Helper.GetExplicitUsernameIfAny(Builder); }}
        public string ExplicitPasswordIfAny { get { return Helper.GetExplicitPasswordIfAny(Builder); }}

        public DbDataAdapter GetDataAdapter(DbCommand cmd)
        {
            return Helper.GetDataAdapter(cmd);
        }

        public DiscoveredDatabase GetCurrentDatabase()
        {
            return new DiscoveredDatabase(this,Helper.GetCurrentDatabase(Builder),Helper.GetQuerySyntaxHelper());
        }

        public void EnableAsync()
        {
            Builder = Helper.EnableAsync(Builder);
        }

        public void ChangeDatabase(string newDatabase)
        {
            Builder = Helper.ChangeDatabase(Builder,newDatabase);
        }

        public override string ToString()
        {
            return Name ;
        }

        public void CreateDatabase(string newDatabaseName)
        {
            //the database we will create - it's ok DiscoveredDatabase is IMightNotExist
            DiscoveredDatabase db = ExpectDatabase(newDatabaseName);

            Helper.CreateDatabase(Builder, db);
            
            if(!db.Exists())
                throw new Exception("Helper tried to create database " + newDatabaseName + " but the database didn't exist after the creation attempt");
        }

        public IManagedConnection BeginNewTransactedConnection()
        {
            return new ManagedConnection(this, Helper.BeginTransaction(Builder));
        }

        /// <summary>
        /// Gets a new ALREADY OPENED connection to the server OR if you specify an existing transaction just wrapps it's connection object
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        internal IManagedConnection GetManagedConnection(IManagedTransaction transaction = null)
        {
            return new ManagedConnection(this, transaction);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return Helper.GetQuerySyntaxHelper();
        }

        public IDiscoveredTableHelper GetTableHelper()
        {
            return Helper.GetDatabaseHelper().GetTableHelper();
        }

        public Dictionary<string, string> DescribeServer()
        {
            return Helper.DescribeServer(Builder);
        }

        public bool RespondsWithinTime(int timeoutInSeconds, out Exception exception)
        {
            return Helper.RespondsWithinTime(Builder,timeoutInSeconds, out exception);
        }

        public DbDataAdapter GetDataAdapter(string command, DbConnection con)
        {
            return GetDataAdapter(GetCommand(command, con));
        }
    }
}
