using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle;
using ReusableLibraryCode.Performance;

namespace ReusableLibraryCode
{
    public class DatabaseCommandHelper
    {
        public const string DatabaseFieldNamesRegex = "[A-Za-z0-9_]*";
        
        public static ComprehensiveQueryPerformanceCounter PerformanceCounter = null;

        /// <summary>
        /// Sets the default Global timeout for new DbCommand objects being created 
        /// </summary>
        public static int GlobalTimeout = 30;

        public static DbCommand GetCommand(string s, DbConnection con, DbTransaction transaction = null)
        {
            var cmd =  new DatabaseHelperFactory(con).CreateInstance().GetCommand(s,con,transaction);

            if(PerformanceCounter != null)
                PerformanceCounter.AddAudit(cmd,Environment.StackTrace.ToString());

            cmd.CommandTimeout = GlobalTimeout;
            return cmd;
        }

        public static DbCommand GetInsertCommand(DbCommand cmd)
        {
            var toReturn = new DatabaseHelperFactory(cmd).CreateInstance().GetCommandBuilder(cmd).GetInsertCommand(true);
            toReturn.CommandTimeout = cmd.CommandTimeout = GlobalTimeout;
            
            return toReturn;
        }

        public static DbParameter GetParameter(string parameterName,DbCommand forCommand)
        {
            return new DatabaseHelperFactory(forCommand).CreateInstance().GetParameter(parameterName);
        }
        

        // only used in missing fields checker, should be in UsefulStuff?
        public static DbConnection GetConnection(DbConnectionStringBuilder connectionStringBuilder)
        {
            return new DatabaseHelperFactory(connectionStringBuilder).CreateInstance().GetConnection(connectionStringBuilder);
        }

        public static DbDataAdapter GetDataAdapter(DbCommand cmd)
        {
            return new DatabaseHelperFactory(cmd).CreateInstance().GetDataAdapter(cmd);
        }

        public static void AddParameterWithValueToCommand(string parameterName, DbCommand command, object valueForParameter)
        {
            DbParameter dbParameter = GetParameter(parameterName, command);
            dbParameter.Value = valueForParameter;
            command.Parameters.Add(dbParameter);
        }

        public static DbConnectionStringBuilder GetConnectionStringBuilder(string targetCatalogueConnectionString, DatabaseType targetDatabaseType)
        {
            IDiscoveredServerHelper helper;
            
            switch (targetDatabaseType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    helper = new MicrosoftSQLServerHelper();
                    break;
                case DatabaseType.MYSQLServer:
                    helper = new MySqlServerHelper();
                    break;
                case DatabaseType.Oracle:
                    helper = new OracleServerHelper();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("targetDatabaseType");
            }

            return helper.GetConnectionStringBuilder(targetCatalogueConnectionString);
        }
    }
}
