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

public class UsefulStuffTests:DatabaseTests
{
        
    [Test]
    public void GetRowCountWhenNoIndexes()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
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
        Assert.AreEqual(1, cmdInsert.ExecuteNonQuery());


        var cmdView = db.Server.GetCommand(
            "CREATE VIEW v_GetRowCount_Views as select * from GetRowCount_Views", con);
        cmdView.ExecuteNonQuery();

        Assert.AreEqual(1, db.ExpectTable("v_GetRowCount_Views").GetRowCount());
    }

    [Test]
    public void PascalCaseStringToHumanReadable()
    {
        Assert.AreEqual("teststringhere", UsefulStuff.PascalCaseStringToHumanReadable("teststringhere"));
        Assert.AreEqual("test Stringhere", UsefulStuff.PascalCaseStringToHumanReadable("testStringhere"));
        Assert.AreEqual("test String Here", UsefulStuff.PascalCaseStringToHumanReadable("testStringHere"));
        Assert.AreEqual("Test String Here", UsefulStuff.PascalCaseStringToHumanReadable("TestStringHere"));
        Assert.AreEqual("TEST String Here", UsefulStuff.PascalCaseStringToHumanReadable("TESTStringHere"));
        Assert.AreEqual("Test STRING Here", UsefulStuff.PascalCaseStringToHumanReadable("TestSTRINGHere"));
        Assert.AreEqual("Test String HERE", UsefulStuff.PascalCaseStringToHumanReadable("TestStringHERE"));

        //Some practical tests for completeness sake
        Assert.AreEqual("A", UsefulStuff.PascalCaseStringToHumanReadable("A"));
        Assert.AreEqual("AS", UsefulStuff.PascalCaseStringToHumanReadable("AS"));
        Assert.AreEqual("A String", UsefulStuff.PascalCaseStringToHumanReadable("AString"));
        Assert.AreEqual("String A Test", UsefulStuff.PascalCaseStringToHumanReadable("StringATest"));
        Assert.AreEqual("String AS Test", UsefulStuff.PascalCaseStringToHumanReadable("StringASTest"));
        Assert.AreEqual("CT Head", UsefulStuff.PascalCaseStringToHumanReadable("CTHead"));
        Assert.AreEqual("WHERE Clause", UsefulStuff.PascalCaseStringToHumanReadable("WHEREClause"));
        Assert.AreEqual("Sql WHERE", UsefulStuff.PascalCaseStringToHumanReadable("SqlWHERE"));

        Assert.AreEqual("Test String", UsefulStuff.PascalCaseStringToHumanReadable("Test_String"));
        Assert.AreEqual("Test string", UsefulStuff.PascalCaseStringToHumanReadable("Test_string"));
        Assert.AreEqual("test String", UsefulStuff.PascalCaseStringToHumanReadable("_testString"));
        Assert.AreEqual("test String", UsefulStuff.PascalCaseStringToHumanReadable("_testString_"));
    }

}