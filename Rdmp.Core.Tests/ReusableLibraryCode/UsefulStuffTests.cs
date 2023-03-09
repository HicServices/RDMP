// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Tests.Common;

namespace Rdmp.Core.Tests.ReusableLibraryCode;

public class UsefulStuffTests:DatabaseTests
{

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.Oracle)]
    [TestCase(DatabaseType.PostgreSql)]
    public void GetRowCounts(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);
        var table = db.ExpectTable("GetRowCountWhenNoIndexes");
        Assert.AreEqual("GetRowCountWhenNoIndexes",table.GetRuntimeName());
        var server = table.Database.Server;

        using var con = server.GetConnection();
        con.Open();

        var cmd = server.GetCommand($"CREATE TABLE {table.GetRuntimeName()} (age int, name varchar(5))", con);
        cmd.ExecuteNonQuery();

        var cmdInsert = server.GetCommand($"INSERT INTO {table.GetRuntimeName()} VALUES (1,'Fish')", con);
        Assert.AreEqual(1,cmdInsert.ExecuteNonQuery());

        Assert.AreEqual(1,table.GetRowCount());

        // Now test using views:
        cmd = db.Server.GetCommand("CREATE TABLE GetRowCount_Views (age int, name varchar(5))", con);
        cmd.ExecuteNonQuery();

        cmdInsert = db.Server.GetCommand("INSERT INTO GetRowCount_Views VALUES (1,'Fish')", con);
        Assert.AreEqual(1, cmdInsert.ExecuteNonQuery());


        var cmdView = db.Server.GetCommand(
            "CREATE VIEW v_GetRowCount_Views as select * from GetRowCount_Views", con);
        cmdView.ExecuteNonQuery();

        Assert.AreEqual(1, db.ExpectTable("v_GetRowCount_Views").GetRowCount());
    }
}