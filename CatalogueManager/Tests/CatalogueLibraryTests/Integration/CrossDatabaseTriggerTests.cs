using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.Triggers;
using CatalogueLibrary.Triggers.Exceptions;
using CatalogueLibrary.Triggers.Implementations;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CrossDatabaseTriggerTests : DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TriggerImplementationTest(DatabaseType type)
        {
            var db = GetCleanedServer(type, "CrossDatabaseTriggerTests");
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

            var archiveTable = tbl.Database.ExpectTable(tbl.GetRuntimeName() + "_Archive");
            Assert.IsTrue(archiveTable.Exists());

            Assert.AreEqual(7,archiveTable.DiscoverColumns().Count());

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

            tbl.AddColumn("amagad",new DatabaseTypeRequest(typeof(float),null,new Tuple<int, int>(2,2)),true);
            implementer = factory.Create(tbl);

            Assert.Throws<IrreconcilableColumnDifferencesInArchiveException>(() => implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());

            archiveTable.AddColumn("amagad", new DatabaseTypeRequest(typeof(float), null, new Tuple<int, int>(2, 2)), true);

            var checks = new TriggerChecks(tbl, true);
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

            string problems;
            string worked;
            implementer.DropTrigger(out problems,out worked);

            Assert.IsNullOrEmpty(problems);

            Assert.AreEqual(TriggerStatus.Missing, implementer.GetTriggerStatus());
        }
    }
}