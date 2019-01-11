using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FAnsi;
using ReusableLibraryCode;

namespace Tests.Common
{
    class TestDatabasesSettings
    {
        public string Prefix { get { return TestDatabaseNames.Prefix; } set { TestDatabaseNames.Prefix = value; } }

        public string ServerName { get; set; }
        public string SqlServerLowPrivilegeUsername { get; set; }
        public string SqlServerLowPrivilegePassword { get; set; }
        
        public string MySql { get; set; }
        public string MySqlLowPrivilegeUsername { get; set; }
        public string MySqlLowPrivilegePassword { get; set; }

        public string Oracle { get; set; }
        public string OracleLowPrivilegeUsername { get; set; }
        public string OracleLowPrivilegePassword { get; set; }

        public string GetLowPrivilegeUsername(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return SqlServerLowPrivilegeUsername;
                case DatabaseType.MySql:
                    return MySqlLowPrivilegeUsername;
                case DatabaseType.Oracle:
                    return OracleLowPrivilegeUsername;
                default:
                    throw new ArgumentOutOfRangeException("databaseType");
            }
        }

        public string GetLowPrivilegePassword(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return SqlServerLowPrivilegePassword;
                case DatabaseType.MySql:
                    return MySqlLowPrivilegePassword;
                case DatabaseType.Oracle:
                    return OracleLowPrivilegePassword;
                default:
                    throw new ArgumentOutOfRangeException("databaseType");
            }
        }
    }
}
