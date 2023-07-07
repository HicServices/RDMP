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
            new DatabaseColumnRequest("name", new DatabaseTypeRequest(typeof (string), 30),false),
            new DatabaseColumnRequest("bubbles", new DatabaseTypeRequest(typeof (int)))
        });

        var factory = new TriggerImplementerFactory(type);
        var implementer = factory.Create(tbl);
            
        Assert.AreEqual(TriggerStatus.Missing,implementer.GetTriggerStatus());

        Assert.AreEqual(2,tbl.DiscoverColumns().Length);

        implementer = factory.Create(tbl);

        //no primary keys
        Assert.Throws<TriggerException>(()=>implementer.CreateTrigger(new ThrowImmediatelyCheckNotifier()));

        tbl.CreatePrimaryKey(tbl.DiscoverColumn("name"));

        implementer = factory.Create(tbl);

        implementer.CreateTrigger(new ThrowImmediatelyCheckNotifier());

        Assert.AreEqual(4, tbl.DiscoverColumns().Length);

        var archiveTable = tbl.Database.ExpectTable($"{tbl.GetRuntimeName()}_Archive");
        Assert.IsTrue(archiveTable.Exists());

        Assert.AreEqual(7, archiveTable.DiscoverColumns().Length);

        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("name")));
        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("bubbles")));
        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("hic_dataLoadrunID",StringComparison.CurrentCultureIgnoreCase)));
        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("hic_validFrom",StringComparison.CurrentCultureIgnoreCase)));
        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("hic_validTo",StringComparison.CurrentCultureIgnoreCase)));
        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("hic_userID",StringComparison.CurrentCultureIgnoreCase)));
        Assert.AreEqual(1, archiveTable.DiscoverColumns().Count(c => c.GetRuntimeName().Equals("hic_status")));
            
        //is the trigger now existing
        Assert.AreEqual(TriggerStatus.Enabled, implementer.GetTriggerStatus());

        //does it function as expected
        using(var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var cmd = tbl.Database.Server.GetCommand(string.Format("INSERT INTO {0}(name,bubbles) VALUES('bob',1)",tbl.GetRuntimeName()),con);
            cmd.ExecuteNonQuery();

            Assert.AreEqual(1,tbl.GetRowCount());
            Assert.AreEqual(0,archiveTable.GetRowCount());

            cmd = tbl.Database.Server.GetCommand(string.Format("UPDATE {0} set bubbles=2",tbl.GetRuntimeName()), con);
            cmd.ExecuteNonQuery();

            Assert.AreEqual(1, tbl.GetRowCount());
            Assert.AreEqual(1, archiveTable.GetRowCount());

            var archive = archiveTable.GetDataTable();
            var dr = archive.Rows.Cast<DataRow>().Single();

            Assert.AreEqual(((DateTime)dr["hic_validTo"]).Date,DateTime.Now.Date);
        }
            
        //do the strict check too
        Assert.IsTrue(implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody()); 

        tbl.AddColumn("amagad",new DatabaseTypeRequest(typeof(float),null,new DecimalSize(2,2)),true,30);
        implementer = factory.Create(tbl);

        Assert.Throws<IrreconcilableColumnDifferencesInArchiveException>(() => implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());

        archiveTable.AddColumn("amagad", new DatabaseTypeRequest(typeof(float), null, new DecimalSize(2, 2)), true, 30);

        var checks = new TriggerChecks(tbl);
        checks.Check(new AcceptAllCheckNotifier());

        Assert.IsTrue(implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());

            
        //does it function as expected
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();

            Assert.AreEqual(1, tbl.GetRowCount());
            Assert.AreEqual(1, archiveTable.GetRowCount());

            var cmd = tbl.Database.Server.GetCommand(string.Format("UPDATE {0} set amagad=1.0", tbl.GetRuntimeName()), con);
            cmd.ExecuteNonQuery();

            cmd = tbl.Database.Server.GetCommand(string.Format("UPDATE {0} set amagad=.09", tbl.GetRuntimeName()), con);
            cmd.ExecuteNonQuery();

            Assert.AreEqual(1, tbl.GetRowCount());
            Assert.AreEqual(3, archiveTable.GetRowCount());

            var archive = archiveTable.GetDataTable();
            Assert.AreEqual(1,archive.Rows.Cast<DataRow>().Count(r=>Equals(r["amagad"],(decimal)1.00)));
            Assert.AreEqual(2, archive.Rows.Cast<DataRow>().Count(r => r["amagad"] == DBNull.Value));
        }

        implementer.DropTrigger(out var problems, out string worked);

        Assert.IsTrue(string.IsNullOrEmpty(problems));

        Assert.AreEqual(TriggerStatus.Missing, implementer.GetTriggerStatus());
    }
}