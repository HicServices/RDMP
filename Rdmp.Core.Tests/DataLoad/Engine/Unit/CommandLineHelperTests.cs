// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.IO;
using FAnsi.Discovery;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[Category("Unit")]
internal class CommandLineHelperTests
{
    [Test]
    public void TestGetValueString()
    {
        var date = new DateTime(2004, 1, 1);
        Assert.AreEqual("\"2004-01-01\"", CommandLineHelper.GetValueString(date));
            

        var fi = new FileInfo(TestContext.CurrentContext.TestDirectory);
        Assert.AreEqual($@"""{TestContext.CurrentContext.TestDirectory}""", CommandLineHelper.GetValueString(fi));

        const string db = "db-name";
        Assert.AreEqual(db, CommandLineHelper.GetValueString(db));

        ImplementationManager.Load<MicrosoftSQLImplementation>();

        //notice how server and db don't actually exist, thats cool they implement IMightNotExist
        var dbInfo = new DiscoveredServer(new SqlConnectionStringBuilder {DataSource = "server"}).ExpectDatabase("db");
        Assert.AreEqual("--database-name=db --database-server=server", CommandLineHelper.CreateArgString("DbInfo", dbInfo));
    }

    [Test]
    public void TestGetValueStringError()
    {
        var obj = new CommandLineHelperTests();
        Assert.Throws<ArgumentException>(()=>CommandLineHelper.GetValueString(obj));
    }

    [Test]
    public void TestCreateArgString()
    {
        var date = new DateTime(2004, 1, 1);
        var argString = CommandLineHelper.CreateArgString("DateFrom", date);
        Assert.AreEqual("--date-from=\"2004-01-01\"", argString);
    }

    [Test]
    public void TestDateTimeCreateArgString()
    {
        var date = new DateTime(2004, 1, 1, 12, 34, 56);
        var argString = CommandLineHelper.CreateArgString("DateFrom", date);
        Assert.AreEqual("--date-from=\"2004-01-01 12:34:56\"", argString);
    }

    [Test]
    public void TestEmptyArgumentName()
    {
        Assert.Throws<ArgumentException>(()=>CommandLineHelper.CreateArgString("", "value"));
    }

    [Test]
    public void TestNameWithoutLeadingUppercaseCharacter()
    {
        Assert.Throws<ArgumentException>(()=>CommandLineHelper.CreateArgString("dateFrom", "2014-01-01"));
    }

    [Test]
    public void TestNullValue()
    {
        Assert.Throws<ArgumentException>(()=>CommandLineHelper.CreateArgString("DateFrom", null));
    }
}