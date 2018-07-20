using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using ReusableLibraryCode.DatabaseHelpers.Discovery.ConnectionStringDefaults;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class DiscoveredServerHelper:IDiscoveredServerHelper
    {
        public static Dictionary<DatabaseType,ConnectionStringKeywordAccumulator> ConnectionStringKeywordAccumulators = new Dictionary<DatabaseType, ConnectionStringKeywordAccumulator>();

        public abstract DbCommand GetCommand(string s, DbConnection con, DbTransaction transaction = null);
        public abstract DbDataAdapter GetDataAdapter(DbCommand cmd);
        public abstract DbCommandBuilder GetCommandBuilder(DbCommand cmd);
        public abstract DbParameter GetParameter(string parameterName);
        
        public abstract DbConnection GetConnection(DbConnectionStringBuilder builder);

        public DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            var builder = GetConnectionStringBuilderImpl(connectionString);
            ConnectionStringKeywordAccumulators[DatabaseType].EnforceOptions(builder);
            return builder;
        }

        public DbConnectionStringBuilder GetConnectionStringBuilder(string server, string database,
            string username, string password)
        {
            var builder = GetConnectionStringBuilderImpl(server,database,username,password);
            ConnectionStringKeywordAccumulators[DatabaseType].EnforceOptions(builder);
            return builder;
        }

        protected abstract DbConnectionStringBuilder GetConnectionStringBuilderImpl(string connectionString, string database, string username, string password);
        protected abstract DbConnectionStringBuilder GetConnectionStringBuilderImpl(string connectionString);
        
        

        protected abstract string ServerKeyName { get; }
        protected abstract string DatabaseKeyName { get; }
        protected virtual string ConnectionTimeoutKeyName { get { return "ConnectionTimeout"; } }

        public string GetServerName(DbConnectionStringBuilder builder)
        {
            return (string) builder[ServerKeyName];
        }

        public DbConnectionStringBuilder ChangeServer(DbConnectionStringBuilder builder, string newServer)
        {
            builder[ServerKeyName] = newServer;
            return builder;
        }

        public string GetCurrentDatabase(DbConnectionStringBuilder builder)
        {
            return (string) builder[DatabaseKeyName];
        }

        public virtual DbConnectionStringBuilder ChangeDatabase(DbConnectionStringBuilder builder, string newDatabase)
        {
            var newBuilder = GetConnectionStringBuilder(builder.ConnectionString);
            newBuilder[DatabaseKeyName] = newDatabase;
            return newBuilder;
        }

        public abstract string[] ListDatabases(DbConnectionStringBuilder builder);
        public abstract string[] ListDatabases(DbConnection con);

        public string[] ListDatabasesAsync(DbConnectionStringBuilder builder, CancellationToken token)
        {
            //list the database on the server
            DbConnection con = GetConnection(builder);
            
            //this will work or timeout
            var openTask = con.OpenAsync(token);
            openTask.Wait(token);

            return ListDatabases(con);
        }

        public abstract DbConnectionStringBuilder EnableAsync(DbConnectionStringBuilder builder);

        public abstract IDiscoveredDatabaseHelper GetDatabaseHelper();
        public abstract IQuerySyntaxHelper GetQuerySyntaxHelper();

        public abstract void CreateDatabase(DbConnectionStringBuilder builder, IHasRuntimeName newDatabaseName);

        public ManagedTransaction BeginTransaction(DbConnectionStringBuilder builder)
        {
            var con = GetConnection(builder);
            con.Open();
            var transaction = con.BeginTransaction();

            return new ManagedTransaction(con,transaction);
        }

        public DatabaseType DatabaseType { get; private set; }
        public abstract Dictionary<string, string> DescribeServer(DbConnectionStringBuilder builder);

        public bool RespondsWithinTime(DbConnectionStringBuilder builder, int timeoutInSeconds,out Exception exception)
        {
            try
            {
                var copyBuilder = GetConnectionStringBuilder(builder.ConnectionString);
                copyBuilder[ConnectionTimeoutKeyName] = timeoutInSeconds;

                using (var con = GetConnection(copyBuilder))
                {
                    con.Open();

                    con.Close();

                    exception = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }
        public abstract string GetExplicitUsernameIfAny(DbConnectionStringBuilder builder);
        public abstract string GetExplicitPasswordIfAny(DbConnectionStringBuilder builder);

        protected DiscoveredServerHelper(DatabaseType databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}