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
        Assert.That(CatalogueRepository.GetAllObjects<TableInfo>().Any());
        tableInfo.DeleteInDatabase();
    }

    [Test]
    public void CreateNewTableInfoInDatabase_valid_pass()
    {
        var table = new TableInfo(CatalogueRepository, "TestDB..TestTableName");

        Assert.That(table, Is.Not.Null);

        table.DeleteInDatabase();

        var ex = Assert.Throws<KeyNotFoundException>(() => CatalogueRepository.GetObjectByID<TableInfo>(table.ID));
        Assert.That($"Could not find TableInfo with ID {table.ID}", Is.EqualTo(ex.Message));
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

        Assert.Multiple(() =>
        {
            Assert.That(tableAfter.Database, Is.EqualTo("CHI_AMALG"));
            Assert.That(tableAfter.Server, Is.EqualTo("Highly restricted"));
            Assert.That(tableAfter.Name, Is.EqualTo("Fishmongery!"));
            Assert.That(tableAfter.DatabaseType, Is.EqualTo(DatabaseType.Oracle));
        });

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
            Assert.Multiple(() =>
            {
                Assert.That(c.GetRuntimeName(), Is.EqualTo("ANOMyCol"));
                Assert.That(c.GetRuntimeName(LoadStage.AdjustRaw), Is.EqualTo("MyCol"));
                Assert.That(c.GetRuntimeName(LoadStage.PostLoad), Is.EqualTo("ANOMyCol"));

                Assert.That(table.GetRuntimeName(), Is.EqualTo("TestTableName"));
                Assert.That(table.GetRuntimeName(LoadBubble.Raw), Is.EqualTo("TestTableName"));
                Assert.That(table.GetRuntimeName(LoadBubble.Staging), Is.EqualTo("TestDB_TestTableName_STAGING"));

                Assert.That(table.GetRuntimeName(LoadBubble.Staging, new SuffixBasedNamer()), Is.EqualTo("TestTableName_STAGING"));
                Assert.That(table.GetRuntimeName(LoadBubble.Staging, new FixedStagingDatabaseNamer("TestDB")), Is.EqualTo("TestDB_TestTableName_STAGING"));

                Assert.That(table.GetRuntimeName(LoadBubble.Live), Is.EqualTo("TestTableName"));
            });
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

        var tbl = db.CreateTable("Fish", new[] { new DatabaseColumnRequest("MyCol", "int") { IsPrimaryKey = true } },
            "Omg");

        Assert.Multiple(() =>
        {
            Assert.That(tbl.GetRuntimeName(), Is.EqualTo("Fish"));
            Assert.That(tbl.Schema, Is.EqualTo("Omg"));
            Assert.That(tbl.GetFullyQualifiedName(), Does.EndWith("[Omg].[Fish]"));

            Assert.That(tbl.Exists());
        });

        Import(tbl, out var ti, out var cols);

        Assert.That(ti.Schema, Is.EqualTo("Omg"));
        var tbl2 = ti.Discover(DataAccessContext.InternalDataProcessing);
        Assert.Multiple(() =>
        {
            Assert.That(tbl2.Schema, Is.EqualTo("Omg"));
            Assert.That(tbl2.Exists());

            Assert.That(ti.Name, Does.EndWith("[Omg].[Fish]"));

            Assert.That(ti.GetFullyQualifiedName(), Does.EndWith("[Omg].[Fish]"));
        });

        var c = cols.Single();

        Assert.Multiple(() =>
        {
            Assert.That(c.GetRuntimeName(), Is.EqualTo("MyCol"));
            Assert.That(c.GetFullyQualifiedName(), Does.Contain("[Omg].[Fish]"));

            //should be primary key
            Assert.That(c.IsPrimaryKey);
        });

        var triggerFactory = new TriggerImplementerFactory(DatabaseType.MicrosoftSQLServer);
        var impl = triggerFactory.Create(tbl);

        Assert.That(impl.GetTriggerStatus(), Is.EqualTo(TriggerStatus.Missing));

        impl.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.Multiple(() =>
        {
            Assert.That(impl.GetTriggerStatus(), Is.EqualTo(TriggerStatus.Enabled));

            Assert.That(impl.CheckUpdateTriggerIsEnabledAndHasExpectedBody());
        });

        //should be synced
        var sync = new TableInfoSynchronizer(ti);
        sync.Synchronize(new AcceptAllCheckNotifier());

        //Test importing the _Legacy table valued function that should be created in the Omg schema and test syncing that too.
        var tvf = ti.Discover(DataAccessContext.InternalDataProcessing).Database
            .ExpectTableValuedFunction("Fish_Legacy", "Omg");
        Assert.That(tvf.Exists());

        var importerTvf = new TableValuedFunctionImporter(CatalogueRepository, tvf);
        importerTvf.DoImport(out var tvfTi, out var tvfCols);

        Assert.That(tvfTi.Schema, Is.EqualTo("Omg"));

        var syncTvf = new TableInfoSynchronizer(tvfTi);
        syncTvf.Synchronize(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.That(tvfTi.Name, Does.EndWith("[Omg].Fish_Legacy(@index) AS Fish_Legacy"));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void TestView(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);
        var syntax = db.Server.GetQuerySyntaxHelper();

        var dt = new DataTable();
        dt.Columns.Add("FF");

        var tbl = db.CreateTable("MyTable", dt);
        Import(tbl, out var tblInfo, out _);

        Assert.Multiple(() =>
        {
            Assert.That(tblInfo.Discover(DataAccessContext.InternalDataProcessing).Exists());
            Assert.That(tblInfo.Discover(DataAccessContext.InternalDataProcessing).TableType, Is.EqualTo(TableType.Table));
        });

        var viewName = "MyView";

        //oracle likes to create stuff under your user account not the database your actually using!
        if (dbType == DatabaseType.Oracle)
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

        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();

            var cmd = tbl.GetCommand(sql, con);
            cmd.ExecuteNonQuery();
        }

        var view = tbl.Database.ExpectTable("MyView", null, TableType.View);
        Import(view, out var viewInfo, out _);

        var sync = new TableInfoSynchronizer(viewInfo);
        sync.Synchronize(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.Multiple(() =>
        {
            Assert.That(viewInfo.Discover(DataAccessContext.InternalDataProcessing).Exists());
            Assert.That(viewInfo.Discover(DataAccessContext.InternalDataProcessing).TableType, Is.EqualTo(TableType.View));
        });

        view.Drop();
        Assert.That(view.Exists(), Is.False);
    }
}