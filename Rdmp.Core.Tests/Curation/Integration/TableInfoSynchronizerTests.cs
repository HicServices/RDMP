// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.Integration;

public class TableInfoSynchronizerTests : DatabaseTests
{
    private DiscoveredServer _server;
    private ITableInfo tableInfoCreated;
    private ColumnInfo[] columnInfosCreated;
    private DiscoveredDatabase _database;

    private const string TABLE_NAME = "TableInfoSynchronizerTests";

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _database = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        _server = _database.Server;

        using (var con = _server.GetConnection())
        {
            con.Open();
            _server.GetCommand($"CREATE TABLE {TABLE_NAME}(Name varchar(10), Address varchar(500))", con)
                .ExecuteNonQuery();
        }

        var tbl = _database.ExpectTable("TableInfoSynchronizerTests");

        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out tableInfoCreated, out columnInfosCreated);
    }

    [Test]
    public void SynchronizationTests_NoChanges()
    {
        Assert.That(tableInfoCreated.GetRuntimeName(), Is.EqualTo(TABLE_NAME));

        var synchronizer = new TableInfoSynchronizer(tableInfoCreated);
        Assert.That(synchronizer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet), Is.EqualTo(true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SynchronizationTests_ColumnDropped(bool acceptChanges)
    {
        Assert.That(tableInfoCreated.GetRuntimeName(), Is.EqualTo(TABLE_NAME));

        var table = _database.ExpectTable(TABLE_NAME);
        var colToDrop = table.DiscoverColumn("Address");
        table.DropColumn(colToDrop);

        var synchronizer = new TableInfoSynchronizer(tableInfoCreated);

        if (acceptChanges)
        {
            Assert.Multiple(() =>
            {
                //accept changes should result in a synchronized table
                Assert.That(synchronizer.Synchronize(new AcceptAllCheckNotifier()), Is.EqualTo(true));
                Assert.That(tableInfoCreated.ColumnInfos, Has.Length.EqualTo(1)); //should only be 1 remaining
            });
        }
        else
        {
            var ex = Assert.Throws<Exception>(() => synchronizer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet));
            Assert.That(ex.Message, Is.EqualTo("The ColumnInfo Address no longer appears in the live table."));
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SynchronizationTests_ColumnAdded(bool acceptChanges)
    {
        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            _server.GetCommand($"ALTER TABLE {TABLE_NAME} ADD Birthday datetime not null", con).ExecuteNonQuery();
        }


        var synchronizer = new TableInfoSynchronizer(tableInfoCreated);

        if (acceptChanges)
        {
            Assert.Multiple(() =>
            {
                //accept changes should result in a synchronized table
                Assert.That(synchronizer.Synchronize(new AcceptAllCheckNotifier()), Is.EqualTo(true));
                Assert.That(tableInfoCreated.ColumnInfos, Has.Length.EqualTo(3)); //should 3 now
            });
        }
        else
        {
            var ex = Assert.Throws<Exception>(() => synchronizer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet));
            Assert.That(ex.Message, Is.EqualTo("The following columns are missing from the TableInfo:Birthday"));
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SynchronizationTests_ColumnAddedWithCatalogue(bool acceptChanges)
    {
        var cataEngineer = new ForwardEngineerCatalogue(tableInfoCreated, columnInfosCreated);
        cataEngineer.ExecuteForwardEngineering(out var cata, out var cataItems, out var extractionInformations);

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(cata.Name, Is.EqualTo(TABLE_NAME));
                Assert.That(cataItems, Has.Length.EqualTo(2));
                Assert.That(extractionInformations, Has.Length.EqualTo(2));
            });

            using (var con = _server.GetConnection())
            {
                con.Open();
                _server.GetCommand($"ALTER TABLE {TABLE_NAME} ADD Birthday datetime not null", con).ExecuteNonQuery();
            }

            var synchronizer = new TableInfoSynchronizer(tableInfoCreated);

            if (acceptChanges)
            {
                Assert.Multiple(() =>
                {
                    //accept changes should result in a synchronized table
                    Assert.That(synchronizer.Synchronize(new AcceptAllCheckNotifier()), Is.EqualTo(true));
                    Assert.That(tableInfoCreated.ColumnInfos, Has.Length.EqualTo(3)); //should 3 now
                    Assert.That(cata.CatalogueItems, Has.Length.EqualTo(3)); //should 3 now
                    Assert.That(cata.GetAllExtractionInformation(ExtractionCategory.Any), Has.Length.EqualTo(3)); //should 3 now
                });

                Assert.Multiple(() =>
                {
                    Assert.That(cata.GetAllExtractionInformation(ExtractionCategory.Any)
                                        .Count(e => e.SelectSQL.Contains("Birthday")), Is.EqualTo(1));
                    Assert.That(cata.CatalogueItems.Count(ci => ci.Name.Contains("Birthday")), Is.EqualTo(1));
                });
            }
            else
            {
                var ex = Assert.Throws<Exception>(() => synchronizer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet));
                Assert.That(ex.Message, Is.EqualTo("The following columns are missing from the TableInfo:Birthday"));
            }
        }
        finally
        {
            cata.DeleteInDatabase();
        }
    }


    /// <summary>
    /// RDMPDEV-1548 This test explores an issue in v3.1 RDMP where synchronization of a TableInfo would fail if there were other tables
    /// in the database which contained brackets in the table name
    /// </summary>
    [Test]
    public void Test_SynchronizeTable_BracketsInTableName()
    {
        var db = _database;

        //FAnsi doesn't let you create tables with brackets in the names so we have to do it manually
        using (var con = db.Server.GetConnection())
        {
            con.Open();
            var cmd = db.Server.GetCommand("CREATE TABLE [BB (ff)] (A int not null)", con);
            cmd.ExecuteNonQuery();
        }

        var tbl = db.CreateTable("FF",
            new DatabaseColumnRequest[]
            {
                new("F", new DatabaseTypeRequest(typeof(int)))
            });

        Import(tbl, out var ti, out _);

        var s = new TableInfoSynchronizer(ti);
        s.Synchronize(ThrowImmediatelyCheckNotifier.Quiet);
    }
}