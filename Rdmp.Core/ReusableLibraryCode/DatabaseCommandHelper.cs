// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Rdmp.Core.ReusableLibraryCode.Performance;

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
///     Provides Cross Database Platform Type translation e.g. GetCommand returns SqlCommand when passed an SqlConnection
///     and a MySqlCommand when passed a
///     MySqlConnection (etc).  Also provides central debugging/performance evaluation of the queries RDMP is using to
///     access Catalogue databases etc via
///     installing a ComprehensiveQueryPerformanceCounter.
/// </summary>
public class DatabaseCommandHelper
{
    private static readonly Dictionary<DatabaseType, IImplementation> _dbConHelpersByType = new()
    {
        { DatabaseType.MySql, new MySqlImplementation() },
        { DatabaseType.Oracle, new OracleImplementation() },
        { DatabaseType.MicrosoftSQLServer, new MicrosoftSQLImplementation() },
        { DatabaseType.PostgreSql, new PostgreSqlImplementation() }
    };

    public static ComprehensiveQueryPerformanceCounter PerformanceCounter = null;

    /// <summary>
    ///     Sets the default Global timeout in seconds for new DbCommand objects being created
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
        return cmd switch
        {
            SqlCommand => _dbConHelpersByType[DatabaseType.MicrosoftSQLServer].GetServerHelper(),
            OracleCommand => _dbConHelpersByType[DatabaseType.Oracle].GetServerHelper(),
            MySqlCommand => _dbConHelpersByType[DatabaseType.MySql].GetServerHelper(),
            NpgsqlCommand => _dbConHelpersByType[DatabaseType.PostgreSql].GetServerHelper(),
            _ => throw new NotSupportedException($"Didn't know what helper to use for DbCommand Type {cmd.GetType()}")
        };
        //todo: add this method to implementation in FAnsi
        //return _dbConHelpersByType.Values.Single(i => i.IsFor(cmd)).GetServerHelper();
    }

    public static DbCommand GetCommand(string s, DbConnection con, DbTransaction transaction = null)
    {
        var cmd = For(con).GetCommand(s, con, transaction);

        PerformanceCounter?.AddAudit(cmd, Environment.StackTrace);

        cmd.CommandTimeout = GlobalTimeout;
        return cmd;
    }

    public static DbCommand GetInsertCommand(DbCommand cmd)
    {
        var toReturn = For(cmd).GetCommandBuilder(cmd).GetInsertCommand(true);
        toReturn.CommandTimeout = cmd.CommandTimeout = GlobalTimeout;

        return toReturn;
    }

    public static DbParameter GetParameter(string parameterName, DbCommand forCommand)
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
        var dbParameter = GetParameter(parameterName, command);
        dbParameter.Value = valueForParameter;
        command.Parameters.Add(dbParameter);
    }
}