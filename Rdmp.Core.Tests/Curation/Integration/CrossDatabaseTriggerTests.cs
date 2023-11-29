// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.Integration;

public class CrossDatabaseTriggerTests : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TriggerImplementationTest(DatabaseType type)
    {
        var db = GetCleanedServer(type);
        var tbl = db.CreateTable("MyTable", new[]
        {
            new DatabaseColumnRequest("name", new DatabaseTypeRequest(typeof(string), 30), false),
            new DatabaseColumnRequest("bubbles", new DatabaseTypeRequest(typeof(int)))
        });

        var factory = new TriggerImplementerFactory(type);
        var implementer = factory.Create(tbl);

        Assert.That(implementer.GetTriggerStatus(), Is.EqualTo(TriggerStatus.Missing));

        Assert.That(tbl.DiscoverColumns(), Has.Length.EqualTo(2));

        implementer = factory.Create(tbl);

        //no primary keys
        Assert.Throws<TriggerException>(() => implementer.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet));

        tbl.CreatePrimaryKey(tbl.DiscoverColumn("name"));

        implementer = factory.Create(tbl);

        implementer.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        Assert.That(tbl.DiscoverColumns(), Has.Length.EqualTo(4));

        var archiveTable = tbl.Database.ExpectTable($"{tbl.GetRuntimeName()}_Archive");
        Assert.That(archiveTable.Exists());

        Assert.That(archiveTable.DiscoverColumns(), Has.Length.EqualTo(7));

        Assert.That(archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("name")), Is.EqualTo(1));
        Assert.That(archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("bubbles")), Is.EqualTo(1));
        Assert.That(archiveTable.DiscoverColumns().Count(c =>
                c.GetRuntimeName().Equals("hic_dataLoadrunID", StringComparison.CurrentCultureIgnoreCase)), Is.EqualTo(1));
        Assert.That(archiveTable.DiscoverColumns().Count(c =>
                c.GetRuntimeName().Equals("hic_validFrom", StringComparison.CurrentCultureIgnoreCase)), Is.EqualTo(1));
        Assert.That(archiveTable.DiscoverColumns().Count(c =>
                c.GetRuntimeName().Equals("hic_validTo", StringComparison.CurrentCultureIgnoreCase)), Is.EqualTo(1));
        Assert.That(archiveTable.DiscoverColumns().Count(c =>
                c.GetRuntimeName().Equals("hic_userID", StringComparison.CurrentCultureIgnoreCase)), Is.EqualTo(1));
        Assert.That(archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("hic_status")), Is.EqualTo(1));

        //is the trigger now existing
        Assert.That(implementer.GetTriggerStatus(), Is.EqualTo(TriggerStatus.Enabled));

        //does it function as expected
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var cmd = tbl.Database.Server.GetCommand(
                $"INSERT INTO {tbl.GetRuntimeName()}(name,bubbles) VALUES('bob',1)", con);
            cmd.ExecuteNonQuery();

            Assert.That(tbl.GetRowCount(), Is.EqualTo(1));
            Assert.That(archiveTable.GetRowCount(), Is.EqualTo(0));

            cmd = tbl.Database.Server.GetCommand($"UPDATE {tbl.GetRuntimeName()} set bubbles=2", con);
            cmd.ExecuteNonQuery();

            Assert.That(tbl.GetRowCount(), Is.EqualTo(1));
            Assert.That(archiveTable.GetRowCount(), Is.EqualTo(1));

            var archive = archiveTable.GetDataTable();
            var dr = archive.Rows.Cast<DataRow>().Single();

            Assert.That(DateTime.Now.Date, Is.EqualTo(((DateTime)dr["hic_validTo"]).Date));
        }

        //do the strict check too
        Assert.That(implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());

        tbl.AddColumn("amagad", new DatabaseTypeRequest(typeof(float), null, new DecimalSize(2, 2)), true, 30);
        implementer = factory.Create(tbl);

        Assert.Throws<IrreconcilableColumnDifferencesInArchiveException>(() =>
            implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());

        archiveTable.AddColumn("amagad", new DatabaseTypeRequest(typeof(float), null, new DecimalSize(2, 2)), true, 30);

        var checks = new TriggerChecks(tbl);
        checks.Check(new AcceptAllCheckNotifier());

        Assert.That(implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());


        //does it function as expected
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();

            Assert.That(tbl.GetRowCount(), Is.EqualTo(1));
            Assert.That(archiveTable.GetRowCount(), Is.EqualTo(1));

            var cmd = tbl.Database.Server.GetCommand($"UPDATE {tbl.GetRuntimeName()} set amagad=1.0", con);
            cmd.ExecuteNonQuery();

            cmd = tbl.Database.Server.GetCommand($"UPDATE {tbl.GetRuntimeName()} set amagad=.09", con);
            cmd.ExecuteNonQuery();

            Assert.That(tbl.GetRowCount(), Is.EqualTo(1));
            Assert.That(archiveTable.GetRowCount(), Is.EqualTo(3));

            var archive = archiveTable.GetDataTable();
            Assert.That(archive.Rows.Cast<DataRow>().Count(r => Equals(r["amagad"], (decimal)1.00)), Is.EqualTo(1));
            Assert.That(archive.Rows.Cast<DataRow>().Count(r => r["amagad"] == DBNull.Value), Is.EqualTo(2));
        }

        implementer.DropTrigger(out var problems, out _);

        Assert.That(string.IsNullOrEmpty(problems));

        Assert.That(implementer.GetTriggerStatus(), Is.EqualTo(TriggerStatus.Missing));
    }
}