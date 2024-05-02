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
///     Determines if a command line argument provided was a reference to a <see cref="DiscoveredDatabase" />
/// </summary>
public partial class PickDatabase : PickObjectBase
{
    public override string Format => "DatabaseType:{DatabaseType}:[Name:{DatabaseName}:]{ConnectionString}";

    public override string Help =>
        @"DatabaseType (Required):
    MicrosoftSQLServer
    MySql
    Oracle

Name: (Optional if in connection string)
ConnectionString (Required)";

    public override IEnumerable<string> Examples => new[]
    {
        "DatabaseType:MicrosoftSQLServer:Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;",

        //see https://stackoverflow.com/questions/4950897/odp-net-integrated-security-invalid-connection-string-argument
        "DatabaseType:Oracle:Name:Bob:Data Source=MyOracleDB;User Id=/;"
    };

    public PickDatabase() : base(null,
        PickDbRegex())
    {
    }

    public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
    {
        var m = MatchOrThrow(arg, idx);

        var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), m.Groups[1].Value, true);
        var dbName = Trim("Name:", m.Groups[2].Value);
        var connectionString = m.Groups[3].Value;

        var server = new DiscoveredServer(connectionString, dbType);

        var db = string.IsNullOrWhiteSpace(dbName) ? server.GetCurrentDatabase() : server.ExpectDatabase(dbName);

        return db == null
            ? throw new CommandLineObjectPickerParseException(
                "Missing database name parameter, it was not in connection string or specified explicitly", idx, arg)
            : new CommandLineObjectPickerArgumentValue(arg, idx, db);
    }

    public override IEnumerable<string> GetAutoCompleteIfAny()
    {
        yield return "DatabaseType:";
        yield return "Oracle";
    }

    [GeneratedRegex("^DatabaseType:([A-Za-z]+):(Name:[^:]+:)?(.*)$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex PickDbRegex();
}