// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

internal class TableInfoTests : DatabaseTests
{
    [Test]
    public void GetAllTableInfos_moreThan1_pass()
    {
        var tableInfo = new TableInfo(CatalogueRepository, "AMAGAD!!!");
        Assert.IsTrue(CatalogueRepository.GetAllObjects<TableInfo>().Any());
        tableInfo.DeleteInDatabase();
    }

    [Test]
    public void CreateNewTableInfoInDatabase_valid_pass()
    {
        var table = new TableInfo(CatalogueRepository, "TestDB..TestTableName");

        Assert.NotNull(table);

        table.DeleteInDatabase();

        var ex = Assert.Throws<KeyNotFoundException>(() => CatalogueRepository.GetObjectByID<TableInfo>(table.ID));
        Assert.AreEqual(ex.Message, $"Could not find TableInfo with ID {table.ID}");
    }

    [Test]
    public void update_changeAllProperties_pass()
    {
        var table = new TableInfo(CatalogueRepository, "CHI_AMALG..SearchStuff")
        {
            Database = "CHI_AMALG",
            Server = "Highly restricted",
            Name = "Fishmongery!",
            DatabaseType = DatabaseType.Oracle
        };

        table.SaveToDatabase();

        var tableAfter = CatalogueRepository.GetObjectByID<TableInfo>(table.ID);

        Assert.IsTrue(tableAfter.Database == "CHI_AMALG");
        Assert.IsTrue(tableAfter.Server == "Highly restricted");
        Assert.IsTrue(tableAfter.Name == "Fishmongery!");
        Assert.IsTrue(tableAfter.DatabaseType == DatabaseType.Oracle);

        tableAfter.DeleteInDatabase();
    }



    [Test]
    [TestCase("[TestDB]..[TestTableName]", "[TestDB]..[TestTableName].[ANOMyCol]")]
    [TestCase("TestDB..TestTableName", "TestDB..TestTableName.ANOMyCol")]
    public void CreateNewTableInfoInDatabase_Naming(string tableName, string columnName)
    {
        var table = new TableInfo(CatalogueRepository, tableName)
        {
            Database = "TestDB"
        };
        table.SaveToDatabase();

        var c = new ColumnInfo(CatalogueRepository, columnName, "varchar(100)", table)
        {
            ANOTable_ID = -100
        };

        try
        {
            Assert.AreEqual("ANOMyCol",c.GetRuntimeName());
            Assert.AreEqual("MyCol", c.GetRuntimeName(LoadStage.AdjustRaw));
            Assert.AreEqual("ANOMyCol", c.GetRuntimeName(LoadStage.PostLoad));

            Assert.AreEqual("TestTableName", table.GetRuntimeName());
            Assert.AreEqual("TestTableName", table.GetRuntimeName(LoadBubble.Raw));
            Assert.AreEqual("TestDB_TestTableName_STAGING", table.GetRuntimeName(LoadBubble.Staging));

            Assert.AreEqual("TestTableName_STAGING", table.GetRuntimeName(LoadBubble.Staging, new SuffixBasedNamer()));
            Assert.AreEqual("TestDB_TestTableName_STAGING", table.GetRuntimeName(LoadBubble.Staging, new FixedStagingDatabaseNamer("TestDB")));

            Assert.AreEqual("TestTableName", table.GetRuntimeName(LoadBubble.Live));

        }
        finally
        {
            c.DeleteInDatabase();
            table.DeleteInDatabase();
        }
    }

    [Test]
    public void TestCreateTableInSchemaAndImportAsTableInfo()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        using var con = db.Server.GetConnection();
        con.Open();

        db.Server.GetCommand("CREATE SCHEMA Omg", con).ExecuteNonQuery();

        var tbl = db.CreateTable("Fish", new [] {new DatabaseColumnRequest("MyCol", "int"){IsPrimaryKey = true}},schema: "Omg");

        Assert.AreEqual("Fish", tbl.GetRuntimeName());
        Assert.AreEqual( "Omg", tbl.Schema);
        Assert.IsTrue(tbl.GetFullyQualifiedName().EndsWith("[Omg].[Fish]"));

        Assert.IsTrue(tbl.Exists());

        Import(tbl,out var ti,out var cols);

        Assert.AreEqual("Omg",ti.Schema);
        var tbl2 = ti.Discover(DataAccessContext.InternalDataProcessing);
        Assert.AreEqual("Omg",tbl2.Schema);
        Assert.IsTrue(tbl2.Exists());

        Assert.IsTrue(ti.Name.EndsWith("[Omg].[Fish]"));

        Assert.IsTrue(ti.GetFullyQualifiedName().EndsWith("[Omg].[Fish]"));

        var c = cols.Single();

        Assert.AreEqual("MyCol",c.GetRuntimeName());
        StringAssert.Contains("[Omg].[Fish]",c.GetFullyQualifiedName());

        //should be primary key
        Assert.IsTrue(c.IsPrimaryKey);

        var triggerFactory = new TriggerImplementerFactory(DatabaseType.MicrosoftSQLServer);
        var impl = triggerFactory.Create(tbl);
                
        Assert.AreEqual(TriggerStatus.Missing,impl.GetTriggerStatus());

        impl.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.AreEqual(TriggerStatus.Enabled, impl.GetTriggerStatus());

        Assert.IsTrue( impl.CheckUpdateTriggerIsEnabledAndHasExpectedBody());

        //should be synced
        var sync = new TableInfoSynchronizer(ti);
        sync.Synchronize(new AcceptAllCheckNotifier());

        //Test importing the _Legacy table valued function that should be created in the Omg schema and test synching that too.
        var tvf = ti.Discover(DataAccessContext.InternalDataProcessing).Database.ExpectTableValuedFunction("Fish_Legacy", "Omg");
        Assert.IsTrue(tvf.Exists());

        var importerTvf = new TableValuedFunctionImporter(CatalogueRepository, tvf);
        importerTvf.DoImport(out var tvfTi,out var tvfCols);

        Assert.AreEqual("Omg",tvfTi.Schema);

        var syncTvf = new TableInfoSynchronizer(tvfTi);
        syncTvf.Synchronize(ThrowImmediatelyCheckNotifier.Quiet);

        StringAssert.EndsWith("[Omg].Fish_Legacy(@index) AS Fish_Legacy",tvfTi.Name);
    }

    [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
    public void TestView(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);
        var syntax = db.Server.GetQuerySyntaxHelper();

        var dt = new DataTable();
        dt.Columns.Add("FF");

        var tbl = db.CreateTable("MyTable",dt);
        Import(tbl,out var tblInfo,out _);
            
        Assert.IsTrue(tblInfo.Discover(DataAccessContext.InternalDataProcessing).Exists());
        Assert.AreEqual(TableType.Table,tblInfo.Discover(DataAccessContext.InternalDataProcessing).TableType);

        var viewName = "MyView";

        //oracle likes to create stuff under your user account not the database your actually using!
        if(dbType == DatabaseType.Oracle)
            viewName = syntax.EnsureFullyQualified(tbl.Database.GetRuntimeName(), null, "MyView");
            
        //postgres hates upper case tables (unless they are wrapped)
        if (dbType == DatabaseType.PostgreSql)
            viewName = syntax.EnsureWrapped(viewName);

        var sql = string.Format(@"CREATE VIEW {0} AS
SELECT {2}
FROM {1}",
            viewName,
            tbl.GetFullyQualifiedName(),
            syntax.EnsureWrapped("FF"));

        using(var con = tbl.Database.Server.GetConnection())
        {
            con.Open();

            var cmd = tbl.GetCommand(sql,con);
            cmd.ExecuteNonQuery();
        }

        var view = tbl.Database.ExpectTable("MyView",null,TableType.View);
        Import(view,out var viewInfo,out _);

        var sync = new TableInfoSynchronizer(viewInfo);
        sync.Synchronize(ThrowImmediatelyCheckNotifier.Quiet);
            
        Assert.IsTrue(viewInfo.Discover(DataAccessContext.InternalDataProcessing).Exists());
        Assert.AreEqual(TableType.View,viewInfo.Discover(DataAccessContext.InternalDataProcessing).TableType);

        view.Drop();
        Assert.IsFalse(view.Exists());
            
    }
}