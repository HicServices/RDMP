using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{
    class CrossDatabaseConstraintsTests : DatabaseTests
    {

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void BulkInsert_MixedCase(DatabaseType type)
        {
            var db = GetCleanedServer(type);

            var tbl = db.CreateTable("Test", new []
            {
                new DatabaseColumnRequest("bob",new DatabaseTypeRequest(typeof(string),100)),
                new DatabaseColumnRequest("Frank", new DatabaseTypeRequest(typeof(string),100))
            });

            DataTable dt = new DataTable(); //note that the column order here is reversed i.e. the DataTable column order doesn't match the database (intended)
            dt.Columns.Add("BoB");
            dt.Columns.Add("fRAnk");
            dt.Rows.Add("no", "yes");
            dt.Rows.Add("no", "no");

            using (var blk = tbl.BeginBulkInsert())
                blk.Upload(dt);

            var result = tbl.GetDataTable();
            Assert.AreEqual(2, result.Rows.Count);  //2 rows inserted
        }
                
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void UnmatchedColumnsBulkInsertTest_UsesDefaultValues_Passes(DatabaseType type)
        {
            var db = GetCleanedServer(type);

            var tbl = db.CreateTable("Test",new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("bob",new DatabaseTypeRequest(typeof(string),100)){IsPrimaryKey =  true,AllowNulls = false},
                new DatabaseColumnRequest("frank", new DatabaseTypeRequest(typeof(DateTime),100)){Default = MandatoryScalarFunctions.GetTodaysDate},
                new DatabaseColumnRequest("peter", new DatabaseTypeRequest(typeof(string),100)){AllowNulls = false},
            });

            DataTable dt = new DataTable(); //note that the column order here is reversed i.e. the DataTable column order doesn't match the database (intended)
            dt.Columns.Add("peter");
            dt.Columns.Add("bob");
            dt.Rows.Add("no", "yes");

            using (var blk = tbl.BeginBulkInsert())
            {
                blk.Upload(dt);
            }

            var result = tbl.GetDataTable();
            Assert.AreEqual(3,result.Columns.Count);
            Assert.AreEqual("yes", result.Rows[0]["bob"]);
            Assert.NotNull(result.Rows[0]["frank"]);
            Assert.GreaterOrEqual(result.Rows[0]["frank"].ToString().Length,5); //should be a date
            Assert.AreEqual("no", result.Rows[0]["peter"]);
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void UnmatchedColumnsBulkInsertTest_UsesDefaultValues_TwoLargeBatches_Passes(DatabaseType type)
        {
            const int numberOfRowsPerBatch = 100010;

            var db = GetCleanedServer(type);

            var tbl = db.CreateTable("Test", new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("bob",new DatabaseTypeRequest(typeof(string),100)){IsPrimaryKey =  true,AllowNulls = false},
                new DatabaseColumnRequest("frank", new DatabaseTypeRequest(typeof(DateTime),100)){Default = MandatoryScalarFunctions.GetTodaysDate},
                new DatabaseColumnRequest("peter", new DatabaseTypeRequest(typeof(string),100)){AllowNulls = false},
            });

            DataTable dt = new DataTable();//note that the column order here is reversed i.e. the DataTable column order doesn't match the database (intended)
            dt.Columns.Add("peter");
            dt.Columns.Add("bob");
            
            for (int i = 0; i < numberOfRowsPerBatch ; i++)
                dt.Rows.Add( "no",Guid.NewGuid().ToString());
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            using (var blk = tbl.BeginBulkInsert())
            {
                Assert.AreEqual(numberOfRowsPerBatch,blk.Upload(dt)); //affected rows should match batch size
            }
            sw.Stop();
            Console.WriteLine("Time taken:" + sw.ElapsedMilliseconds + "ms");
            
            dt.Rows.Clear();

            for (int i = 0; i < numberOfRowsPerBatch; i++)
                dt.Rows.Add( "no",Guid.NewGuid().ToString());

            sw.Restart();

            using (var blk = tbl.BeginBulkInsert())
            {
                Assert.AreEqual(numberOfRowsPerBatch,blk.Upload(dt));
            }
            sw.Stop();
            Console.WriteLine("Time taken:" + sw.ElapsedMilliseconds + "ms");



            var result = tbl.GetDataTable();
            Assert.AreEqual(3, result.Columns.Count);
            Assert.AreEqual(numberOfRowsPerBatch * 2, result.Rows.Count);
            Assert.NotNull(result.Rows[0]["bob"]);
            Assert.NotNull(result.Rows[0]["frank"]);
            Assert.GreaterOrEqual(result.Rows[0]["frank"].ToString().Length, 5); //should be a date
            Assert.AreEqual("no", result.Rows[0]["peter"]);
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void NullPrimaryKey_ThrowsException(DatabaseType type)
        {
            var db = GetCleanedServer(type);

            var tbl = db.CreateTable("Test", new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("bob",new DatabaseTypeRequest(typeof(string),100)){IsPrimaryKey =  true,AllowNulls = false}
            });

            DataTable dt = new DataTable();
            dt.Columns.Add("bob");
            dt.Rows.Add(DBNull.Value);

            using (var blk = tbl.BeginBulkInsert())
            {
                Assert.Throws(Is.InstanceOf<Exception>(),()=>blk.Upload(dt));
            }
        }
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void AutoIncrementPrimaryKey_Passes(DatabaseType type)
        {
            var db = GetCleanedServer(type);

            var tbl = db.CreateTable("Test", new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("bob",new DatabaseTypeRequest(typeof(int),100)){IsPrimaryKey =  true,AllowNulls = false,IsAutoIncrement = true},
                new DatabaseColumnRequest("frank",new DatabaseTypeRequest(typeof(string),100)){IsPrimaryKey =  true,AllowNulls = false}
            });

            DataTable dt = new DataTable();
            dt.Columns.Add("frank");
            dt.Rows.Add("fish");
            dt.Rows.Add("fish");
            dt.Rows.Add("tank");

            using (var blk = tbl.BeginBulkInsert())
            {
                Assert.AreEqual(3,blk.Upload(dt));
            }


            var result = tbl.GetDataTable();

            Assert.AreEqual(2, result.Columns.Count);
            Assert.AreEqual(3, result.Rows.Count);
            Assert.AreEqual(1, result.Rows.Cast<DataRow>().Count(r => Convert.ToInt32(r["bob"]) == 1));
            Assert.AreEqual(1, result.Rows.Cast<DataRow>().Count(r => Convert.ToInt32(r["bob"]) == 2));
            Assert.AreEqual(1, result.Rows.Cast<DataRow>().Count(r => Convert.ToInt32(r["bob"]) == 3));
            
        }
    }
}
