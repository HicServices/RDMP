using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    internal class OracleServerHelper : DiscoveredServerHelper
    {
        public OracleServerHelper() : base(DatabaseType.Oracle)
        {
        }

        protected override string ServerKeyName { get { return "DATA SOURCE"; } }
        protected override string DatabaseKeyName { get { return "USER ID"; } }//this is pretty insane is this really what oracle does?

        #region Up Typing
        public override DbCommand GetCommand(string s, DbConnection con, DbTransaction transaction = null)
        {
            return new OracleCommand(s, con as OracleConnection) {Transaction = transaction as OracleTransaction};
        }

        public override DbDataAdapter GetDataAdapter(DbCommand cmd)
        {
            return new OracleDataAdapter((OracleCommand) cmd);
        }

        public override DbCommandBuilder GetCommandBuilder(DbCommand cmd)
        {
            return new OracleCommandBuilder((OracleDataAdapter) GetDataAdapter(cmd));
        }

        public override DbParameter GetParameter(string parameterName)
        {
            return new OracleParameter(parameterName,null);
        }

        public override DbConnection GetConnection(DbConnectionStringBuilder builder)
        {
            return new OracleConnection(builder.ConnectionString);
        }

        public override DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            return new OracleConnectionStringBuilder(connectionString);
        }
        #endregion

        public override DbConnectionStringBuilder GetConnectionStringBuilder(string server, string database, string username, string password)
        {
            var toReturn = new OracleConnectionStringBuilder() {DataSource = server};

            if (string.IsNullOrWhiteSpace(username))
                toReturn.UserID = "/";
            else
            {
                toReturn.UserID = username;
                toReturn.Password = password;
            }
            
            return toReturn;
        }

        public override DbConnectionStringBuilder ChangeDatabase(DbConnectionStringBuilder builder, string newDatabase)
        {
            //does not apply to oracle since user = database but we create users with random passwords
            return builder;
        }

        public override DbConnectionStringBuilder EnableAsync(DbConnectionStringBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override IDiscoveredDatabaseHelper GetDatabaseHelper()
        {
            return new OracleDatabaseHelper();
        }

        public override IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new OracleQuerySyntaxHelper();
        }

        public override void CreateDatabase(DbConnectionStringBuilder builder, IHasRuntimeName newDatabaseName)
        {
            using(var con = new OracleConnection(builder.ConnectionString))
            {
                con.Open();
                //create a new user with a random password!!! - go oracle this makes perfect sense database=user!
                var cmd = new OracleCommand(
                    "CREATE USER " + newDatabaseName.GetRuntimeName() + " IDENTIFIED BY pwd" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 27) //oracle only allows 30 character passwords
                    ,(OracleConnection) con); 
                cmd.ExecuteNonQuery();

                cmd = new OracleCommand("ALTER USER " + newDatabaseName.GetRuntimeName() + " quota unlimited on system", (OracleConnection)con);
                cmd.ExecuteNonQuery();
            }
        }

        public override Dictionary<string, string> DescribeServer(DbConnectionStringBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override bool RespondsWithinTime(DbConnectionStringBuilder builder, int timeoutInSeconds, out Exception exception)
        {
            throw new NotImplementedException();
        }

        public override string[] ListDatabases(DbConnectionStringBuilder builder)
        {
            //todo do we have to edit the builder in here incase it is pointed at nothing?
            using (var con = new OracleConnection(builder.ConnectionString))
            {
                con.Open();
                return ListDatabases(con);
            }
        }

        public override string[] ListDatabases(DbConnection con)
        {
            var cmd = GetCommand("select distinct username from dba_users", con); //already comes as single column called Database
            
            List<string> databases = new List<string>();

            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    databases.Add((string) r["username"]);
            
            return databases.ToArray();
        }
    }
}