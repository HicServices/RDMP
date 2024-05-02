// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

/// <summary>
///     Determines if a command line argument provided was a reference to a <see cref="DiscoveredTable" />
/// </summary>
public class PickTable : PickObjectBase
{
    public override string Format =>
        "Table:{TableName}:[Schema:{SchemaIfAny}:][IsView:{True/False}]:DatabaseType:{DatabaseType}:Name:{DatabaseName}:{ConnectionString}";

    public override string Help =>
        @"Table (Required): Name of the table you want
Schema (Optional): leave out this section unless your table is in a sub schema within the Database (MySql doesn't support schemas)
IsView (Optional): Defaults to false, pass 'true' to instead look for a view

DatabaseType (Required):
    MicrosoftSQLServer
    MySql
    Oracle

Name: Name of the database it resides in (Optional if in connection string)
ConnectionString (Required)";

    public override IEnumerable<string> Examples => new[]
    {
        "Table:MyTable:DatabaseType:MicrosoftSQLServer:Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;",
        "Table:MyTable2:Schema:dbo:IsView:True:DatabaseType:MicrosoftSQLServer:Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;"
    };


    public PickTable() : base(null,
        new Regex("^Table:([^:]+):(Schema:[^:]+:)?(IsView:[^:]+:)?DatabaseType:([A-Za-z]+):(Name:[^:]+:)?(.*)$",
            RegexOptions.IgnoreCase))
    {
    }

    public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
    {
        var m = MatchOrThrow(arg, idx);

        var tableName = m.Groups[1].Value;
        var schema = Trim("Schema:", m.Groups[2].Value);

        var isViewStr = Trim("IsView:", m.Groups[3].Value);
        var isViewBool = isViewStr != null && bool.Parse(isViewStr);

        var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), m.Groups[4].Value);
        var dbName = Trim("Name:", m.Groups[5].Value);
        var connectionString = m.Groups[6].Value;

        var server = new DiscoveredServer(connectionString, dbType);

        var db = (string.IsNullOrWhiteSpace(dbName) ? server.GetCurrentDatabase() : server.ExpectDatabase(dbName)) ??
                 throw new CommandLineObjectPickerParseException(
                     "Missing database name parameter, it was not in connection string or specified explicitly", idx,
                     arg);
        return new CommandLineObjectPickerArgumentValue(arg, idx,
            db.ExpectTable(tableName, schema, isViewBool ? TableType.View : TableType.Table));
    }
}