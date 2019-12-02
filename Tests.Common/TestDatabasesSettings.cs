// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;

namespace Tests.Common
{
    class TestDatabasesSettings
    {
        public string Prefix { get { return TestDatabaseNames.Prefix; } set { TestDatabaseNames.Prefix = value; } }

        public string ServerName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string SqlServerLowPrivilegeUsername { get; set; }
        public string SqlServerLowPrivilegePassword { get; set; }
        
        public string MySql { get; set; }
        public string MySqlLowPrivilegeUsername { get; set; }
        public string MySqlLowPrivilegePassword { get; set; }

        public string Oracle { get; set; }
        public string OracleLowPrivilegeUsername { get; set; }
        public string OracleLowPrivilegePassword { get; set; }
        
        public string PostgreSql { get; set; }
        public string PostgreSqlLowPrivilegeUsername { get; set; }
        public string PostgreSqlLowPrivilegePassword { get; set; }

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
                case DatabaseType.PostgreSql:
                    return PostgreSqlLowPrivilegeUsername;
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
                case DatabaseType.PostgreSql:
                    return PostgreSqlLowPrivilegePassword;
                default:
                    throw new ArgumentOutOfRangeException("databaseType");
            }
        }
    }
}
