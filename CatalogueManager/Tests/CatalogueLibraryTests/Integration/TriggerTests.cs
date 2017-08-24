using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Checks.Checkers;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class TriggerTests :DatabaseTests
    {
        private TriggerImplementer _implementer;
        private DiscoveredTable _table;
        private DiscoveredTable _archiveTable;

        [SetUp]
        public void CreateTable()
        {
            RunSQL("CREATE TABLE TriggerTests(name varchar(30) not null,bubbles int)");
            
            _implementer = new TriggerImplementer(DiscoveredDatabaseICanCreateRandomTablesIn, "TriggerTests");
            _table = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("TriggerTests");
            _archiveTable = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("TriggerTests_Archive");
        }

        [Test]
        public void NoTriggerExists()
        {
            Assert.AreEqual(TriggerImplementer.TriggerStatus.Missing, _implementer.CheckUpdateTriggerIsEnabledOnServer());
        }

        [Test]
        public void CreateWithNoPks_Complain()
        {
            var ex = Assert.Throws<Exception>(()=>_implementer.CreateTrigger(_table.DiscoverColumns().Where(c=>c.IsPrimaryKey).Select(k=>k.GetRuntimeName()).ToArray(),new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("There must be at least 1 primary key", ex.Message);
        }

        [Test]
        public void CreateWithPks_Valid()
        {
            RunSQL("Alter TABLE TriggerTests ADD PRIMARY KEY (name)");
            _implementer.CreateTrigger(
                _table.DiscoverColumns().Where(c => c.IsPrimaryKey).Select(k => k.GetRuntimeName()).ToArray(),
                new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(TriggerImplementer.TriggerStatus.Enabled,_implementer.CheckUpdateTriggerIsEnabledOnServer());
            Assert.AreEqual(true, _implementer.CheckUpdateTriggerIsEnabled_Advanced(new[] {"name"}));
        }

        [Test]
        public void AlterTest_InvalidThenRecreateItAndItsValidAgain()
        {
            CreateWithPks_Valid();

            RunSQL("ALTER TABLE TriggerTests add fish int");
            RunSQL("ALTER TABLE TriggerTests_Archive add fish int");
            
            //still not valid because trigger SQL is missing it in the column list
            var ex = Assert.Throws<Exception>(()=> _implementer.CheckUpdateTriggerIsEnabled_Advanced(new[] { "name" }));
            Assert.IsTrue(ex.InnerException.Message.Equals(@"Trigger TriggerTests_OnUpdate is corrupt
Strings differ at index 915
EXPECTED:RunID,hic_validFrom,fish,hic_v...
ACTUAL  :RunID,hic_validFrom,hic_validT...
-----------------------------^"));

            
            string problemsDroppingTrigger, thingsThatWorkedDroppingTrigger;
            _implementer.DropTrigger(out problemsDroppingTrigger, out thingsThatWorkedDroppingTrigger);
            _implementer.CreateTrigger(new[] {"name"},new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(true, _implementer.CheckUpdateTriggerIsEnabled_Advanced(new[] { "name" }));
        }

        [Test]
        public void NowTestDataInsertion()
        {
            AlterTest_InvalidThenRecreateItAndItsValidAgain();

            RunSQL("INSERT INTO TriggerTests (name,bubbles,fish,hic_validFrom) VALUES ('Franky',3,5,'2001-01-02')");

            RunSQL("UPDATE TriggerTests set bubbles =99");

            //new value is 99
            Assert.AreEqual(99,ExecuteScalar("Select bubbles FROM TriggerTests where name = 'Franky'"));
            //archived value is 3
            Assert.AreEqual(3, ExecuteScalar("Select bubbles FROM TriggerTests_Archive where name = 'Franky'"));

            //legacy in 2001-01-01 it didn't exist
            Assert.IsNull( ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-01') where name = 'Franky'"));
            //legacy in 2001-01-03 it did exist and was 3
            Assert.AreEqual(3, ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-03') where name = 'Franky'"));
            //legacy boundary case?
            Assert.AreEqual(3, ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-02') where name = 'Franky'"));
            
            //legacy today it is 99
            Assert.AreEqual(99, ExecuteScalar("Select bubbles FROM TriggerTests_Legacy(GETDATE()) where name = 'Franky'"));

        }

        [Test]
        public void IdentityTest()
        {
            
            RunSQL("Alter TABLE TriggerTests ADD myident int identity(1,1) PRIMARY KEY");

            _implementer.CreateTrigger(new[] { "myident" },new ThrowImmediatelyCheckNotifier());
            _implementer.CheckUpdateTriggerIsEnabled_Advanced(new[] {"myident"});

            

        }

        private object ExecuteScalar(string sql)
        {
            var svr = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = svr.GetConnection())
            {
                con.Open();
                return svr.GetCommand(sql, con).ExecuteScalar();
            }
        }

        private void RunSQL(string sql)
        {
            var svr = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = svr.GetConnection())
            {
                con.Open();
                svr.GetCommand(sql, con).ExecuteNonQuery();
            }
        }

        [TearDown]
        public void DropTable()
        {
            string problemsDroppingTrigger, thingsThatWorkedDroppingTrigger;
            _implementer.DropTrigger(out problemsDroppingTrigger, out thingsThatWorkedDroppingTrigger);

            if(_archiveTable.Exists())
                _archiveTable.Drop();

            _table.Drop();
        }
    }
}
