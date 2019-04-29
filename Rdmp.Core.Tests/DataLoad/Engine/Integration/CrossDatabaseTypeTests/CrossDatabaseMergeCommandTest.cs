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
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CatalogueLibrary.Data.Defaults;
using Rdmp.Core.CatalogueLibrary.Data.EntityNaming;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Migration;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.Logging;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.CrossDatabaseTypeTests
{
    public class CrossDatabaseMergeCommandTest:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
        public void TestMerge(DatabaseType databaseType)
        {
            var dbFrom = GetCleanedServer(databaseType, "CrossDatabaseMergeCommandFrom");
            var dbTo = GetCleanedServer(databaseType, "CrossDatabaseMergeCommandTo");

            var dt = new DataTable();
            var colName = new DataColumn("Name");
            var colAge = new DataColumn("Age");
            dt.Columns.Add(colName);
            dt.Columns.Add(colAge);
            dt.Columns.Add("Postcode");

            //Data in live awaiting to be updated
            dt.Rows.Add(new object[]{"Dave",18,"DD3 1AB"});
            dt.Rows.Add(new object[] {"Dave", 25, "DD1 1XS" });
            dt.Rows.Add(new object[] {"Mango", 32, DBNull.Value});
            dt.Rows.Add(new object[] { "Filli", 32,"DD3 78L" });
            dt.Rows.Add(new object[] { "Mandrake", 32, DBNull.Value });

            dt.PrimaryKey = new[]{colName,colAge};

            var to = dbTo.CreateTable("ToTable", dt);

            Assert.IsTrue(to.DiscoverColumn("Name").IsPrimaryKey);
            Assert.IsTrue(to.DiscoverColumn("Age").IsPrimaryKey);
            Assert.IsFalse(to.DiscoverColumn("Postcode").IsPrimaryKey);

            dt.Rows.Clear();
            
            //new data being loaded
            dt.Rows.Add(new object[] { "Dave", 25, "DD1 1PS" }); //update to change postcode to "DD1 1PS"
            dt.Rows.Add(new object[] { "Chutney", 32, DBNull.Value }); //new insert Chutney
            dt.Rows.Add(new object[] { "Mango", 32, DBNull.Value }); //ignored because already present in dataset
            dt.Rows.Add(new object[] { "Filli", 32, DBNull.Value }); //update from "DD3 78L" null
            dt.Rows.Add(new object[] { "Mandrake", 32, "DD1 1PS" }); //update from null to "DD1 1PS"
            dt.Rows.Add(new object[] { "Mandrake", 31, "DD1 1PS" }); // insert because Age is unique (and part of pk)
            
            var from = dbFrom.CreateTable("CrossDatabaseMergeCommandTo_ToTable_STAGING", dt);
            

            //import the to table as a TableInfo
            TableInfo ti;
            ColumnInfo[] cis;
            var cata = Import(to,out ti,out cis);

            //put the backup trigger on the live table (this will also create the needed hic_ columns etc)
            var triggerImplementer = new TriggerImplementerFactory(databaseType).Create(to);
            triggerImplementer.CreateTrigger(new ThrowImmediatelyCheckNotifier());

            var configuration = new MigrationConfiguration(dbFrom, LoadBubble.Staging, LoadBubble.Live,new FixedStagingDatabaseNamer(to.Database.GetRuntimeName(),from.Database.GetRuntimeName()));

            var lmd = new LoadMetadata(CatalogueRepository);
            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();

            var migrationHost = new MigrationHost(dbFrom, dbTo, configuration, new HICDatabaseConfiguration(lmd));

            //set up a logging task
            var logServer = new ServerDefaults(CatalogueRepository).GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
            var logManager = new LogManager(logServer);
            logManager.CreateNewLoggingTaskIfNotExists("CrossDatabaseMergeCommandTest");
            var dli = logManager.CreateDataLoadInfo("CrossDatabaseMergeCommandTest", "tests", "running test", "", true);

            var job = new ThrowImmediatelyDataLoadJob();
            job.DataLoadInfo = dli;
            job.RegularTablesToLoad = new List<ITableInfo>(new[]{ti});
            
            migrationHost.Migrate(job, new GracefulCancellationToken());
            
            var resultantDt = to.GetDataTable();
            Assert.AreEqual(7,resultantDt.Rows.Count);

            AssertRowEquals(resultantDt, "Dave", 25, "DD1 1PS");
            AssertRowEquals(resultantDt, "Chutney", 32, DBNull.Value);
            AssertRowEquals(resultantDt, "Mango", 32, DBNull.Value);
            
            AssertRowEquals(resultantDt,"Filli",32,DBNull.Value);
            AssertRowEquals(resultantDt, "Mandrake", 32, "DD1 1PS");
            AssertRowEquals(resultantDt, "Mandrake", 31, "DD1 1PS");
            
            AssertRowEquals(resultantDt, "Dave", 18, "DD3 1AB");


            var archival = logManager.GetArchivalDataLoadInfos("CrossDatabaseMergeCommandTest", new CancellationToken());
            var log = archival.First();


            Assert.AreEqual(dli.ID,log.ID);
            Assert.AreEqual(2,log.TableLoadInfos.Single().Inserts);
            Assert.AreEqual(3, log.TableLoadInfos.Single().Updates);
        }

        private void AssertRowEquals(DataTable resultantDt,string name,int age, object postcode)
        {
            Assert.AreEqual(
                1, resultantDt.Rows.Cast<DataRow>().Count(r => Equals(r["Name"], name) && Equals(r["Age"], age) && Equals(r["Postcode"], postcode)),
                "Did not find expected record:" + string.Join(",",name,age,postcode));
        }
    }
}
