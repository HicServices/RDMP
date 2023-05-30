// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Repositories;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.UI.Tests.CommandExecution.Alter;

class ExecuteCommandAlterColumnTypeTests:DatabaseTests
{
    [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
    [UITimeout(10000)]
    public void AlterColumnType_NoArchive(DatabaseType dbType)
    {

        var db = GetCleanedServer(dbType);
        var tbl = db.CreateTable("MyTbl", new[] { new DatabaseColumnRequest("mycol", new DatabaseTypeRequest(typeof(string), 10)) });

        Import(tbl, out var ti, out _);

        var ui = new UITests();
        var activator = new TestActivateItems(ui, new MemoryDataExportRepository());
                       
        var myCol = tbl.DiscoverColumn("myCol");
            
        //should have started out as 10
        Assert.AreEqual(10, myCol.DataType.GetLengthIfString());

        //we want the new type to be 50 long
        var newType = myCol.DataType.SQLType.Replace("10", "50");
        activator.TypeTextResponse = newType;

        var cmd = new ExecuteCommandAlterColumnType(activator, ti.ColumnInfos.Single());
        cmd.Execute();

        //rediscover the col to get the expected new datatype
        myCol = tbl.DiscoverColumn("myCol");

        Assert.AreEqual(newType, myCol.DataType.SQLType);
        Assert.AreEqual(newType, ti.ColumnInfos[0].Data_type);

        tbl.Drop();
    }


    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.Oracle)]
    [UITimeout(10000)]
    public void AlterColumnType_WithArchive(DatabaseType dbType)
    {

        var db = GetCleanedServer(dbType);
        var tbl = db.CreateTable("MyTbl", new[] { new DatabaseColumnRequest("mycol", new DatabaseTypeRequest(typeof(string), 10)) });
        var tblArchive = db.CreateTable("MyTbl_Archive", new[] { new DatabaseColumnRequest("mycol", new DatabaseTypeRequest(typeof(string), 10)) });

        Import(tbl, out var ti, out _);

        var ui = new UITests();
        var activator = new TestActivateItems(ui, new MemoryDataExportRepository());

        var myCol = tbl.DiscoverColumn("myCol");

        //should have started out as 10
        Assert.AreEqual(10, myCol.DataType.GetLengthIfString());

        var oldType = myCol.DataType.SQLType;
        //we want the new type to be 50 long
        var newType = oldType.Replace("10", "50");
        activator.TypeTextResponse = newType;

        var cmd = new ExecuteCommandAlterColumnType(activator, ti.ColumnInfos.Single());
        cmd.Execute();

        //rediscover the col to get the expected new datatype
        myCol = tbl.DiscoverColumn("myCol");
        var myColArchive = tblArchive.DiscoverColumn("myCol");

        Assert.AreEqual(newType, myCol.DataType.SQLType);
        Assert.AreEqual(newType, ti.ColumnInfos[0].Data_type);

        //if they changed the archive then the archive column should also match on Type otherwise it should have stayed the old Type
        Assert.AreEqual(newType, myColArchive.DataType.SQLType);

        tbl.Drop();
        tblArchive.Drop();
    }
}