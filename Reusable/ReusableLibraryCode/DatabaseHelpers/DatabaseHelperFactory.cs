using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle;

namespace ReusableLibraryCode.DatabaseHelpers
{
    public class DatabaseHelperFactory
    {
        private readonly DatabaseType _type;

        public DatabaseHelperFactory(DatabaseType type)
        {
            _type = type;
        }

        public DatabaseHelperFactory(DbConnection con)
        {
            if (con is SqlConnection)
               _type = DatabaseType.MicrosoftSQLServer;
            else
            if (con is MySqlConnection)
                _type = DatabaseType.MYSQLServer;
            else if (con is OracleConnection)
                _type = DatabaseType.Oracle;
            else
                throw new Exception("Unexpected connection type " + con.GetType().Name);
        }

        public DatabaseHelperFactory(DbCommand cmd)
        {
            if(cmd is SqlCommand)
                _type = DatabaseType.MicrosoftSQLServer;
            else
            if (cmd is MySqlCommand)
                _type = DatabaseType.MYSQLServer;
            else
            if (cmd is OracleCommand)
                _type = DatabaseType.Oracle;
            else 
                throw new Exception("Unexpected command type " + cmd.GetType().Name);
        }

        public DatabaseHelperFactory(DbConnectionStringBuilder builder)
        {
            if (builder is SqlConnectionStringBuilder)
                _type = DatabaseType.MicrosoftSQLServer;
            else 
            if (builder is MySqlConnectionStringBuilder)
                _type = DatabaseType.MYSQLServer;
            else if (builder is OracleConnectionStringBuilder)
                _type = DatabaseType.Oracle;
            else
                throw new Exception("Unexpected DbConnectionStringBuilder type " + builder.GetType().Name);
        }

        public IDiscoveredServerHelper CreateInstance()
        {
            switch (_type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return new MicrosoftSQLServerHelper();
                case DatabaseType.MYSQLServer:
                    return new MySqlServerHelper();
                case DatabaseType.Oracle:
                    return new OracleServerHelper();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
