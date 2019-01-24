using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Discovery.TypeTranslation.TypeDeciders;
using FAnsi.Implementation;
using Fansi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.Performance;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Provides Cross Database Platform Type translation e.g. GetCommand returns SqlCommand when passed an SqlConnection and a MySqlCommand when passed a
    /// MySqlConnection (etc).  Also provides central debugging/performance evaluation of the queries RDMP is using to access Catalogue databases etc via
    /// installing a ComprehensiveQueryPerformanceCounter.  
    /// </summary>
    public class DatabaseCommandHelper
    {
        private static readonly Dictionary<DatabaseType, IImplementation> _dbConHelpersByType = new Dictionary<DatabaseType, IImplementation>()
        {
            {DatabaseType.MySql,new MySqlImplementation()},
            {DatabaseType.Oracle,new OracleImplementation()},
            {DatabaseType.MicrosoftSQLServer,new MicrosoftSQLImplementation()},
        };

        public static ComprehensiveQueryPerformanceCounter PerformanceCounter = null;

        /// <summary>
        /// Sets the default Global timeout in seconds for new DbCommand objects being created 
        /// </summary>
        public static int GlobalTimeout = 30;

        

        public static IDiscoveredServerHelper For(DbConnection con)
        {
            return _dbConHelpersByType.Values.Single(i => i.IsFor(con)).GetServerHelper();
        }

        public static IDiscoveredServerHelper For(DbConnectionStringBuilder connectionStringBuilder)
        {
            return _dbConHelpersByType.Values.Single(i => i.IsFor(connectionStringBuilder)).GetServerHelper();
        }

        public static IDiscoveredServerHelper For(DatabaseType dbType)
        {
            return _dbConHelpersByType[dbType].GetServerHelper();
        }

        public static IDiscoveredServerHelper For(DbCommand cmd)
        {
            if (cmd is SqlCommand)
                return _dbConHelpersByType[DatabaseType.MicrosoftSQLServer].GetServerHelper();
            if (cmd is OracleCommand)
                return _dbConHelpersByType[DatabaseType.Oracle].GetServerHelper();
            if (cmd is MySqlCommand)
                return _dbConHelpersByType[DatabaseType.MySql].GetServerHelper();

            throw new NotSupportedException("Didn't know what helper to use for DbCommand Type " + cmd.GetType());
            //todo: add this method to implementation in FAnsi
            //return _dbConHelpersByType.Values.Single(i => i.IsFor(cmd)).GetServerHelper();
        }

        public static DbCommand GetCommand(string s, DbConnection con, DbTransaction transaction = null)
        {
            var cmd = For(con).GetCommand(s, con, transaction);
            
            if(PerformanceCounter != null)
                PerformanceCounter.AddAudit(cmd,Environment.StackTrace.ToString());

            cmd.CommandTimeout = GlobalTimeout;
            return cmd;
        }

        public static DbCommand GetInsertCommand(DbCommand cmd)
        {
            var toReturn = For(cmd).GetCommandBuilder(cmd).GetInsertCommand(true);
            toReturn.CommandTimeout = cmd.CommandTimeout = GlobalTimeout;
            
            return toReturn;
        }

        public static DbParameter GetParameter(string parameterName,DbCommand forCommand)
        {
            return For(forCommand).GetParameter(parameterName);
        }

        public static DbParameter GetParameter(string parameterName, DatabaseType databaseType)
        {
            return For(databaseType).GetParameter(parameterName);
        }
        // only used in missing fields checker, should be in UsefulStuff?
        public static DbConnection GetConnection(DbConnectionStringBuilder connectionStringBuilder)
        {
            return For(connectionStringBuilder).GetConnection(connectionStringBuilder);
        }

        public static DbDataAdapter GetDataAdapter(DbCommand cmd)
        {
            return For(cmd).GetDataAdapter(cmd);
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
                case DatabaseType.MySql:
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

        static TypeDeciderFactory typeDeciderFactory = new TypeDeciderFactory();

        
        public static DbParameter GetParameter(string paramName, IQuerySyntaxHelper syntaxHelper, DiscoveredColumn discoveredColumn, object value)
        {
            var p = GetParameter(paramName, syntaxHelper.DatabaseType);

            var tt = syntaxHelper.TypeTranslater;
            p.DbType = tt.GetDbTypeForSQLDBType(discoveredColumn.DataType.SQLType);
            var cSharpType = tt.GetCSharpTypeForSQLDBType(discoveredColumn.DataType.SQLType);

            if (syntaxHelper.IsBasicallyNull(value))
                p.Value = DBNull.Value;
            else  
                if (value is string && typeDeciderFactory.IsSupported(cSharpType)) //if the input is a string and it's for a hard type e.g. TimeSpan 
                {
                    var o = typeDeciderFactory.Create(cSharpType).Parse((string)value);

                    //Apparently everyone in Microsoft hates TimeSpans - see test MicrosoftHatesDbTypeTime
                    if (o is TimeSpan && syntaxHelper.DatabaseType == DatabaseType.MicrosoftSQLServer)
                        o = Convert.ToDateTime(o.ToString());

                    p.Value = o;

                }
                else
                    p.Value = value;

            return p;
        }
    }
}
