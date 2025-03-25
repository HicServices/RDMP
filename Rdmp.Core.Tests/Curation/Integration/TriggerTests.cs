// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Exceptions;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.Integration;

public class TriggerTests : DatabaseTests
{
    private DiscoveredTable _table;
    private DiscoveredTable _archiveTable;
    private DiscoveredDatabase _database;


    private void CreateTable(DatabaseType dbType)
    {
        _database = GetCleanedServer(dbType);

        _table = _database.CreateTable("TriggerTests", new DatabaseColumnRequest[]
        {
            new("name", new DatabaseTypeRequest(typeof(string), 30)) { AllowNulls = false },
            new("bubbles", new DatabaseTypeRequest(typeof(int)))
        });

        _archiveTable = _database.ExpectTable("TriggerTests_Archive");
    }

    private ITriggerImplementer GetImplementer()
    {
        return new TriggerImplementerFactory(_database.Server.DatabaseType).Create(_table);
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void NoTriggerExists(DatabaseType dbType)
    {
        CreateTable(dbType);

        var implementer = GetImplementer();

        //most likely doesn't exist but may do
        implementer.DropTrigger(out _, out _);

        Assert.That(implementer.GetTriggerStatus(), Is.EqualTo(TriggerStatus.Missing));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void CreateWithNoPks_Complain(DatabaseType dbType)
    {
        CreateTable(dbType);

        var ex = Assert.Throws<TriggerException>(() =>
            GetImplementer().CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("There must be at least 1 primary key"));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void CreateWithPks_Valid(DatabaseType dbType)
    {
        CreateTable(dbType);

        _table.CreatePrimaryKey(_table.DiscoverColumn("name"));
        GetImplementer().CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.Multiple(() =>
        {
            Assert.That(GetImplementer().GetTriggerStatus(), Is.EqualTo(TriggerStatus.Enabled));
            Assert.That(GetImplementer().CheckUpdateTriggerIsEnabledAndHasExpectedBody(), Is.EqualTo(true));
        });
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void Create_WithDodgyColumnNames(DatabaseType dbType)
    {
        _database = GetCleanedServer(dbType);

        _table = _database.CreateTable("Trol lol My Table Select * from Group by fish", new DatabaseColumnRequest[]
        {
            new("My Lovely Column Select * From Lolz", new DatabaseTypeRequest(typeof(string), 30))
                { AllowNulls = false, IsPrimaryKey = true },
            new("ANormalColumnName", new DatabaseTypeRequest(typeof(int))),
            new("Group By Meeee Colll trollolol", new DatabaseTypeRequest(typeof(int)))
        });

        GetImplementer().CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.Multiple(() =>
        {
            Assert.That(GetImplementer().GetTriggerStatus(), Is.EqualTo(TriggerStatus.Enabled));
            Assert.That(GetImplementer().CheckUpdateTriggerIsEnabledAndHasExpectedBody(), Is.EqualTo(true));
        });
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void AlterTest_InvalidThenRecreateItAndItsValidAgain(DatabaseType dbType)
    {
        CreateWithPks_Valid(dbType);

        _table.AddColumn("fish", new DatabaseTypeRequest(typeof(int)), true, 500);
        _archiveTable.AddColumn("fish", new DatabaseTypeRequest(typeof(int)), true, 500);

        //still not valid because trigger SQL is missing it in the column list
        var ex = Assert.Throws<ExpectedIdenticalStringsException>(() =>
            GetImplementer().CheckUpdateTriggerIsEnabledAndHasExpectedBody());
        Assert.That(ex.Message, Is.Not.Null);

        var implementer = GetImplementer();
        implementer.DropTrigger(out var problemsDroppingTrigger, out _);
        Assert.That(problemsDroppingTrigger, Is.Empty);

        implementer.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.That(implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody(), Is.EqualTo(true));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void NowTestDataInsertion(DatabaseType dbType)
    {
        AlterTest_InvalidThenRecreateItAndItsValidAgain(dbType);

        _table.Insert(new Dictionary<string, object>
        {
            { "name", "Franky" },
            { "bubbles", 3 },
            { "hic_validFrom", new DateTime(2001, 1, 2) },
            { "hic_dataLoadRunID", 7 }
        });

        var liveOldRow = _table.GetDataTable().Rows.Cast<DataRow>().Single(r => r["bubbles"] as int? == 3);
        Assert.That((DateTime)liveOldRow[SpecialFieldNames.ValidFrom], Is.EqualTo(new DateTime(2001, 1, 2)));

        RunSQL("UPDATE {0} set bubbles =99", _table.GetFullyQualifiedName());

        Assert.Multiple(() =>
        {
            //new value is 99
            Assert.That(ExecuteScalar("Select bubbles FROM {0} where name = 'Franky'", _table.GetFullyQualifiedName()),
                Is.EqualTo(99));
            //archived value is 3
            Assert.That(
                ExecuteScalar("Select bubbles FROM {0} where name = 'Franky'", _archiveTable.GetFullyQualifiedName()),
                Is.EqualTo(3));
        });

        //Legacy table valued function only works for MicrosoftSQLServer
        if (dbType == DatabaseType.MicrosoftSQLServer)
            Assert.Multiple(() =>
            {
                //legacy in 2001-01-01 it didn't exist
                Assert.That(
                    ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-01') where name = 'Franky'"),
                    Is.Null);
                //legacy in 2001-01-03 it did exist and was 3
                Assert.That(
                    ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-03') where name = 'Franky'"),
                    Is.EqualTo(3));
                //legacy boundary case?
                Assert.That(
                    ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-02') where name = 'Franky'"),
                    Is.EqualTo(3));

                //legacy today it is 99
                Assert.That(ExecuteScalar("Select bubbles FROM TriggerTests_Legacy(GETDATE()) where name = 'Franky'"),
                    Is.EqualTo(99));
            });

        // Live row should now reflect that it is validFrom today
        var liveNewRow = _table.GetDataTable().Rows.Cast<DataRow>().Single(r => r["bubbles"] as int? == 99);
        Assert.That(((DateTime)liveNewRow[SpecialFieldNames.ValidFrom]).Date, Is.EqualTo(DateTime.Now.Date));

        // Archived row should not have had its validFrom field broken
        var archivedRow = _archiveTable.GetDataTable().Rows.Cast<DataRow>().Single(r => r["bubbles"] as int? == 3);
        Assert.That((DateTime)archivedRow[SpecialFieldNames.ValidFrom], Is.EqualTo(new DateTime(2001, 1, 2)));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void DiffDatabaseDataFetcherTest(DatabaseType dbType)
    {
        CreateTable(dbType);

        _table.CreatePrimaryKey(_table.DiscoverColumn("name"));

        GetImplementer().CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        _table.Insert(new Dictionary<string, object>
        {
            { "name", "Franky" },
            { "bubbles", 3 },
            { "hic_validFrom", new DateTime(2001, 1, 2) },
            { "hic_dataLoadRunID", 7 }
        });

        Thread.Sleep(1000);
        RunSQL("UPDATE {0} SET bubbles=1", _table.GetFullyQualifiedName());

        Thread.Sleep(1000);
        RunSQL("UPDATE {0} SET bubbles=2", _table.GetFullyQualifiedName());

        Thread.Sleep(1000);
        RunSQL("UPDATE {0} SET bubbles=3", _table.GetFullyQualifiedName());

        Thread.Sleep(1000);
        RunSQL("UPDATE {0} SET bubbles=4", _table.GetFullyQualifiedName());

        Thread.Sleep(1000);

        Assert.Multiple(() =>
        {
            Assert.That(_table.GetRowCount(), Is.EqualTo(1));
            Assert.That(_archiveTable.GetRowCount(), Is.EqualTo(4));
        });

        Import(_table, out var ti, out _);
        var fetcher = new DiffDatabaseDataFetcher(1, ti, 7, 100);

        fetcher.FetchData(new AcceptAllCheckNotifier());
        Assert.Multiple(() =>
        {
            Assert.That(fetcher.Updates_New.Rows[0]["bubbles"], Is.EqualTo(4));
            Assert.That(fetcher.Updates_Replaced.Rows[0]["bubbles"], Is.EqualTo(3));

            Assert.That(fetcher.Updates_New.Rows, Has.Count.EqualTo(1));
            Assert.That(fetcher.Updates_Replaced.Rows, Has.Count.EqualTo(1));
        });
    }


    [Test]
    public void IdentityTest()
    {
        CreateTable(DatabaseType.MicrosoftSQLServer);

        RunSQL("Alter TABLE TriggerTests ADD myident int identity(1,1) PRIMARY KEY");

        var implementer = new MicrosoftSQLTriggerImplementer(_table);

        implementer.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);
        implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody();
    }

    private object ExecuteScalar(string sql, params string[] args)
    {
        if (args.Length != 0)
            sql = string.Format(sql, args);

        var svr = _database.Server;
        using var con = svr.GetConnection();
        con.Open();
        return svr.GetCommand(sql, con).ExecuteScalar();
    }

    private void RunSQL(string sql, params string[] args)
    {
        if (args.Length != 0)
            sql = string.Format(sql, args);
        if (_database == null)
            throw new Exception("You must call CreateTable first");

        using var con = _database.Server.GetConnection();
        con.Open();
        _database.Server.GetCommand(sql, con).ExecuteNonQuery();
    }
}