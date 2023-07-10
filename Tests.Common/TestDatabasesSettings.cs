// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;

namespace Tests.Common;

public class TestDatabasesSettings
{
#pragma warning disable CA1822 // Mark members as static - that upsets the YAML hack used for loading settings
    public string Prefix
    {
        get => TestDatabaseNames.Prefix;
        set => TestDatabaseNames.Prefix = value;
    }
#pragma warning restore CA1822 // Mark members as static

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


    public bool UseFileSystemRepo { get; set; }

    public string GetLowPrivilegeUsername(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.MicrosoftSQLServer => SqlServerLowPrivilegeUsername,
            DatabaseType.MySql => MySqlLowPrivilegeUsername,
            DatabaseType.Oracle => OracleLowPrivilegeUsername,
            DatabaseType.PostgreSql => PostgreSqlLowPrivilegeUsername,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType))
        };
    }



    public string GetLowPrivilegePassword(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.MicrosoftSQLServer => SqlServerLowPrivilegePassword,
            DatabaseType.MySql => MySqlLowPrivilegePassword,
            DatabaseType.Oracle => OracleLowPrivilegePassword,
            DatabaseType.PostgreSql => PostgreSqlLowPrivilegePassword,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType))
        };
    }
}