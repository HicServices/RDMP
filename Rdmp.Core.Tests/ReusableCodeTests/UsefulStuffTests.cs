// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode;
using Tests.Common;

namespace Rdmp.Core.Tests.ReusableCodeTests;

public class UsefulStuffTests : DatabaseTests
{
    [Test]
    public void GetRowCountWhenNoIndexes()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var table = db.ExpectTable("GetRowCountWhenNoIndexes");
        Assert.That(table.GetRuntimeName(), Is.EqualTo("GetRowCountWhenNoIndexes"));
        var server = table.Database.Server;

        using var con = server.GetConnection();
        con.Open();

        var cmd = server.GetCommand($"CREATE TABLE {table.GetRuntimeName()} (age int, name varchar(5))", con);
        cmd.ExecuteNonQuery();

        var cmdInsert = server.GetCommand($"INSERT INTO {table.GetRuntimeName()} VALUES (1,'Fish')", con);
        Assert.Multiple(() =>
        {
            Assert.That(cmdInsert.ExecuteNonQuery(), Is.EqualTo(1));

            Assert.That(table.GetRowCount(), Is.EqualTo(1));
        });
    }

    [Test]
    public void GetRowCount_Views()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        using var con = db.Server.GetConnection();
        con.Open();

        var cmd = db.Server.GetCommand("CREATE TABLE GetRowCount_Views (age int, name varchar(5))", con);
        cmd.ExecuteNonQuery();

        var cmdInsert = db.Server.GetCommand("INSERT INTO GetRowCount_Views VALUES (1,'Fish')", con);
        Assert.That(cmdInsert.ExecuteNonQuery(), Is.EqualTo(1));


        var cmdView = db.Server.GetCommand(
            "CREATE VIEW v_GetRowCount_Views as select * from GetRowCount_Views", con);
        cmdView.ExecuteNonQuery();

        Assert.That(db.ExpectTable("v_GetRowCount_Views").GetRowCount(), Is.EqualTo(1));
    }

    [Test]
    public void PascalCaseStringToHumanReadable()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("teststringhere"), Is.EqualTo("teststringhere"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("testStringhere"), Is.EqualTo("test Stringhere"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("testStringHere"), Is.EqualTo("test String Here"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("TestStringHere"), Is.EqualTo("Test String Here"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("TESTStringHere"), Is.EqualTo("TEST String Here"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("TestSTRINGHere"), Is.EqualTo("Test STRING Here"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("TestStringHERE"), Is.EqualTo("Test String HERE"));

            //Some practical tests for completeness sake
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("A"), Is.EqualTo("A"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("AS"), Is.EqualTo("AS"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("AString"), Is.EqualTo("A String"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("StringATest"), Is.EqualTo("String A Test"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("StringASTest"), Is.EqualTo("String AS Test"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("CTHead"), Is.EqualTo("CT Head"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("WHEREClause"), Is.EqualTo("WHERE Clause"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("SqlWHERE"), Is.EqualTo("Sql WHERE"));

            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("Test_String"), Is.EqualTo("Test String"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("Test_string"), Is.EqualTo("Test string"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("_testString"), Is.EqualTo("test String"));
            Assert.That(UsefulStuff.PascalCaseStringToHumanReadable("_testString_"), Is.EqualTo("test String"));
        });
    }
}