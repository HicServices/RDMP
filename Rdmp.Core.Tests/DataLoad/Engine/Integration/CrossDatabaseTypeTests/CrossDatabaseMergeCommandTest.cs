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
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Migration;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.CrossDatabaseTypeTests;

public class CrossDatabaseMergeCommandTest : FromToDatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestMerge(DatabaseType databaseType)
    {
        //microsoft one gets called for free in test setup (see base class)
        if (databaseType != DatabaseType.MicrosoftSQLServer)
            SetupFromTo(databaseType);

        var dt = new DataTable();
        var colName = new DataColumn("Name", typeof(string));
        var colAge = new DataColumn("Age", typeof(int));
        dt.Columns.Add(colName);
        dt.Columns.Add(colAge);
        dt.Columns.Add("Postcode", typeof(string));

        //Data in live awaiting toTbl be updated
        dt.Rows.Add(new object[] { "Dave", 18, "DD3 1AB" });
        dt.Rows.Add(new object[] { "Dave", 25, "DD1 1XS" });
        dt.Rows.Add(new object[] { "Mango", 32, DBNull.Value });
        dt.Rows.Add(new object[] { "Filli", 32, "DD3 78L" });
        dt.Rows.Add(new object[] { "Mandrake", 32, DBNull.Value });

        dt.PrimaryKey = new[] { colName, colAge };

        var toTbl = To.CreateTable("ToTable", dt);

        Assert.Multiple(() =>
        {
            Assert.That(toTbl.DiscoverColumn("Name").IsPrimaryKey);
            Assert.That(toTbl.DiscoverColumn("Age").IsPrimaryKey);
            Assert.That(toTbl.DiscoverColumn("Postcode").IsPrimaryKey, Is.False);
        });

        dt.Rows.Clear();

        //new data being loaded
        dt.Rows.Add(new object[] { "Dave", 25, "DD1 1PS" }); //update toTbl change postcode toTbl "DD1 1PS"
        dt.Rows.Add(new object[] { "Chutney", 32, DBNull.Value }); //new insert Chutney
        dt.Rows.Add(new object[] { "Mango", 32, DBNull.Value }); //ignored because already present in dataset
        dt.Rows.Add(new object[] { "Filli", 32, DBNull.Value }); //update from "DD3 78L" null
        dt.Rows.Add(new object[] { "Mandrake", 32, "DD1 1PS" }); //update from null toTbl "DD1 1PS"
        dt.Rows.Add(new object[] { "Mandrake", 31, "DD1 1PS" }); // insert because Age is unique (and part of pk)

        var fromTbl = From.CreateTable($"{DatabaseName}_ToTable_STAGING", dt);

        //import the toTbl table as a TableInfo
        var cata = Import(toTbl, out var ti, out var cis);

        //put the backup trigger on the live table (this will also create the needed hic_ columns etc)
        var triggerImplementer = new TriggerImplementerFactory(databaseType).Create(toTbl);
        triggerImplementer.CreateTrigger(ThrowImmediatelyCheckNotifier.Quiet);

        var configuration = new MigrationConfiguration(From, LoadBubble.Staging, LoadBubble.Live,
            new FixedStagingDatabaseNamer(toTbl.Database.GetRuntimeName(), fromTbl.Database.GetRuntimeName()));

        var lmd = new LoadMetadata(CatalogueRepository);
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        var migrationHost = new MigrationHost(From, To, configuration, new HICDatabaseConfiguration(lmd));

        //set SetUp a logging task
        var logServer = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        logManager.CreateNewLoggingTaskIfNotExists("CrossDatabaseMergeCommandTest");
        var dli = logManager.CreateDataLoadInfo("CrossDatabaseMergeCommandTest", "tests", "running test", "", true);

        var job = new ThrowImmediatelyDataLoadJob
        {
            LoadMetadata = lmd,
            DataLoadInfo = dli,
            RegularTablesToLoad = new List<ITableInfo>(new[] { ti })
        };

        migrationHost.Migrate(job, new GracefulCancellationToken());

        var resultantDt = toTbl.GetDataTable();
        Assert.That(resultantDt.Rows, Has.Count.EqualTo(7));

        AssertRowEquals(resultantDt, "Dave", 25, "DD1 1PS");
        AssertRowEquals(resultantDt, "Chutney", 32, DBNull.Value);
        AssertRowEquals(resultantDt, "Mango", 32, DBNull.Value);

        AssertRowEquals(resultantDt, "Filli", 32, DBNull.Value);
        AssertRowEquals(resultantDt, "Mandrake", 32, "DD1 1PS");
        AssertRowEquals(resultantDt, "Mandrake", 31, "DD1 1PS");

        AssertRowEquals(resultantDt, "Dave", 18, "DD3 1AB");


        var archival = logManager.GetArchivalDataLoadInfos("CrossDatabaseMergeCommandTest", new CancellationToken());
        var log = archival.First();

        Assert.Multiple(() =>
        {
            Assert.That(log.ID, Is.EqualTo(dli.ID));
            Assert.That(log.TableLoadInfos.Single().Inserts, Is.EqualTo(2));
            Assert.That(log.TableLoadInfos.Single().Updates, Is.EqualTo(3));
        });
    }

    private static void AssertRowEquals(DataTable resultantDt, string name, int age, object postcode)
    {
        Assert.That(
            resultantDt.Rows.Cast<DataRow>().Count(r =>
                Equals(r["Name"], name) && Equals(r["Age"], age) && Equals(r["Postcode"], postcode)), Is.EqualTo(1),
            "Did not find expected record:" + string.Join(",", name, age, postcode));
    }
}