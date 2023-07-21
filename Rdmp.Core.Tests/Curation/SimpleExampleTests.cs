// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation;

public class SimpleExampleTests : DatabaseTests
{
    [Test]
    public void Test1()
    {
        var cata = new Catalogue(CatalogueRepository, "My Test Cata");
        Assert.IsTrue(cata.Exists());
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void Test2(DatabaseType type)
    {
        var database = GetCleanedServer(type);

        Assert.IsTrue(database.Exists());
        Assert.IsEmpty(database.DiscoverTables(true));
        Assert.IsNotNull(database.GetRuntimeName());
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void TestReadDataLowPrivileges(DatabaseType type)
    {
        var database = GetCleanedServer(type);

        //create a table on the server
        var dt = new DataTable();
        dt.Columns.Add("MyCol");
        dt.Rows.Add("Hi");
        dt.PrimaryKey = new[] { dt.Columns[0] };

        var tbl = database.CreateTable("MyTable", dt);

        //at this point we are reading it with the credentials setup by GetCleanedServer
        Assert.AreEqual(1, tbl.GetRowCount());
        Assert.AreEqual(1, tbl.DiscoverColumns().Length);
        Assert.IsTrue(tbl.DiscoverColumn("MyCol").IsPrimaryKey);

        //create a reference to the table in RMDP
        Import(tbl, out var tableInfo, out var columnInfos);

        //setup credentials for the table in RDMP (this will be Inconclusive if you have not enabled it in TestDatabases.txt
        SetupLowPrivilegeUserRightsFor(tableInfo, TestLowPrivilegePermissions.Reader);

        //request access to the database using DataLoad context
        var newDatabase = DataAccessPortal.ExpectDatabase(tableInfo, DataAccessContext.DataLoad);

        //get new reference to the table
        var newTbl = newDatabase.ExpectTable(tableInfo.GetRuntimeName());

        //the credentials should be different
        Assert.AreNotEqual(tbl.Database.Server.ExplicitUsernameIfAny, newTbl.Database.Server.ExplicitUsernameIfAny);

        //try re-reading the data 
        Assert.AreEqual(1, newTbl.GetRowCount());
        Assert.AreEqual(1, newTbl.DiscoverColumns().Length);
        Assert.IsTrue(newTbl.DiscoverColumn("MyCol").IsPrimaryKey);

        //low priority user shouldn't be able to drop tables
        Assert.That(newTbl.Drop, Throws.Exception);

        //normal testing user should be able to
        tbl.Drop();
    }
}