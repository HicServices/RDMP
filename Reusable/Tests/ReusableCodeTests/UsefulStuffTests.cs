using System;
using System.Data;
using System.Data.SqlClient;
using Diagnostics.TestData;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace ReusableCodeTests
{
    public class UsefulStuffTests:DatabaseTests
    {
        private BulkTestsData _bulkTests;


        [OneTimeSetUp]
        public void BulkTestsSetUp()
        {
            _bulkTests = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn);
            _bulkTests.SetupTestData();
        }

        [OneTimeTearDown]
        public void BulkTestsTearDown()
        {
            _bulkTests.Destroy();
        }

        [Test]
        public void BulkDataFloatHandling_DoesNotReturnFunnyRounding()
        {
            DataTable dataTable = _bulkTests.GetDataTable(1000);

            bool atLeastOneDodgyValue = false;
            foreach (DataRow dr in dataTable.Rows)
                if (dr["patient_triage_score"].ToString().Length >= 5)
                {
                    Console.WriteLine("Dodgy value spotted:" + dr["patient_triage_score"]);
                    atLeastOneDodgyValue = true;
                }

            Assert.IsFalse(atLeastOneDodgyValue);
        }

        [Test]
        public void GetRowCountWhenNoIndexes()
        {
            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            var table = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("GetRowCountWhenNoIndexes");
            Assert.AreEqual("GetRowCountWhenNoIndexes",table.GetRuntimeName());

            using(var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand("CREATE TABLE " + table .GetRuntimeName()+ " (age int, name varchar(5))", con);
                cmd.ExecuteNonQuery();

                var cmdInsert = server.GetCommand("INSERT INTO " + table.GetRuntimeName() + " VALUES (1,'Fish')", con);
                Assert.AreEqual(1,cmdInsert.ExecuteNonQuery());

                Assert.AreEqual(1,table.GetRowCount());
                
            }
        }

        [Test]
        public void GetRowCount_Views()
        {
            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;

            using (var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand("CREATE TABLE GetRowCount_Views (age int, name varchar(5))", con);
                cmd.ExecuteNonQuery();

                var cmdInsert = server.GetCommand("INSERT INTO GetRowCount_Views VALUES (1,'Fish')", con);
                Assert.AreEqual(1, cmdInsert.ExecuteNonQuery());


                var cmdView = server.GetCommand(
                    "CREATE VIEW v_GetRowCount_Views as select * from GetRowCount_Views", con);
                cmdView.ExecuteNonQuery();

                Assert.AreEqual(1,
                    DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("v_GetRowCount_Views").GetRowCount());
            }
        }
        
        [Test]
        public void BulkInsertWithBetterErrorMessagesCorrectlyWarnsUsAboutFloats()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("myfloat", typeof(object));
            dt.Rows.Add(1.5f);

            SqlBulkCopy copy = new SqlBulkCopy(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString);
            var ex = Assert.Throws<NotSupportedException>(()=>UsefulStuff.BulkInsertWithBetterErrorMessages(copy, dt, DiscoveredServerICanCreateRandomDatabasesAndTablesOn));
            StringAssert.Contains("Found float value 1.5 in data table, SQLServer does not support floats in bulk insert",ex.Message);
        }
    }
}
