using System;
using System.Collections.Generic;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Triggers.Implementations;
using DataLoadEngine.Job;
using DataLoadEngine.Migration;
using HIC.Logging;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{
    public class CrossDatabaseMergeCommandTest:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
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

            dt.Rows.Add(new object[]{"Dave",18,"DD3 1AB"});
            dt.Rows.Add(new object[] {"Dave", 25, "DD1 1XS" });
            dt.Rows.Add(new object[] {"Mango", 32, DBNull.Value});

            dt.PrimaryKey = new[]{colName,colAge};

            var from = dbFrom.CreateTable("CrossDatabaseMergeCommandTo_ToTable_STAGING", dt);

            Assert.IsTrue(from.DiscoverColumn("Name").IsPrimaryKey);
            Assert.IsTrue(from.DiscoverColumn("Age").IsPrimaryKey);
            Assert.IsFalse(from.DiscoverColumn("Postcode").IsPrimaryKey);

            dt.Rows.Clear();
            
            dt.Rows.Add(new object[] { "Dave", 25, "DD1 1PS" });//update
            dt.Rows.Add(new object[] { "Chutney", 32, DBNull.Value }); //new
            dt.Rows.Add(new object[] { "Mango", 32, DBNull.Value }); //ignored because already present in dataset

            var to = dbTo.CreateTable("ToTable", dt);

            //import the to table as a TableInfo
            var importer = new TableInfoImporter(CatalogueRepository, to);
            TableInfo ti;
            ColumnInfo[] cis;
            importer.DoImport(out ti, out cis);

            //put the backup trigger on the live table (this will also create the needed hic_ columns etc)
            var triggerImplementer = new TriggerImplementerFactory(databaseType).Create(to);
            triggerImplementer.CreateTrigger(new ThrowImmediatelyCheckNotifier());

            var configuration = new MigrationConfiguration(dbFrom, LoadBubble.Staging, LoadBubble.Live,new FixedStagingDatabaseNamer(to.Database.GetRuntimeName(),from.Database.GetRuntimeName()));
            
            var migrationHost = new MigrationHost(dbFrom, dbTo, configuration);

            //set up a logging task
            var logServer = new ServerDefaults(CatalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
            var logManager = new LogManager(logServer);
            logManager.CreateNewLoggingTaskIfNotExists("CrossDatabaseMergeCommandTest");
            var dli = logManager.CreateDataLoadInfo("CrossDatabaseMergeCommandTest", "tests", "running test", "", true);

            var job = new ThrowImmediatelyDataLoadJob();
            job.DataLoadInfo = dli;
            job.RegularTablesToLoad = new List<TableInfo>(new[]{ti});
            
            migrationHost.Migrate(job, new GracefulCancellationToken());
            
            var resultantDt = to.GetDataTable();
            Assert.AreEqual(4,resultantDt.Rows.Count);
        }
    }
}
