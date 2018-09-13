using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders;
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
        public static ComprehensiveQueryPerformanceCounter PerformanceCounter = null;

        /// <summary>
        /// Sets the default Global timeout in seconds for new DbCommand objects being created 
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

        public static DbParameter GetParameter(string parameterName, DatabaseType databaseType)
        {
            return new DatabaseHelperFactory(databaseType).CreateInstance().GetParameter(parameterName);
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

        static TypeDeciderFactory typeDeciderFactory = new TypeDeciderFactory();

        /// <summary>
        /// Gets a DbParameter hard typed with the correct DbType for the discoveredColumn and the Value set to the correct Value representation (e.g. DBNull for nulls or whitespace).
        /// <para>Also handles converting DateTime representations since many DBMS are a bit rubbish at that</para> 
        /// </summary>
        /// <param name="paramName">The name for the parameter e.g. @myParamter</param>
        /// <param name="syntaxHelper">The syntax that can figure out Types for the DBMS you are targetting, grab from your nearest <see cref="IHasQuerySyntaxHelper"/> e.g. <see cref="DiscoveredColumn.Table"/></param>
        /// <param name="discoveredColumn">The column the parameter is for loading - this is used to determine the DbType for the paramter</param>
        /// <param name="value">The value to populate into the command, this will be converted to DBNull.Value if the value is nullish</param>
        /// <returns></returns>
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
