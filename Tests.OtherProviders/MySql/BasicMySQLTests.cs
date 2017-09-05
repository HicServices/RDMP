using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace Tests.OtherProviders.MySql
{
    [TestFixture]
    [Category("Oracle")]
    public class BasicMySQLTests : DatabaseTests
    {
        DiscoveredServer server;
        private readonly string _databaseName = TestDatabaseNames.GetConsistentName("BOB");
        private bool _isServerAvailable;

        [TestFixtureSetUp]
        public void CreateTestDatabase()
        {
            if (DiscoveredMySqlServer == null)
            {
                _isServerAvailable = false;
                return;
            }

            server = DiscoveredMySqlServer;
            if (!server.Exists())
            {
                _isServerAvailable = false;
                return;
            }

            _isServerAvailable = true;

            //cleanup
            try
            {
                DeleteTestDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed with message " + e.Message );
            }
            
            DbCommand cmd;
            using (var con = server.GetConnection())
            {
                con.Open();

                cmd = server.GetCommand("CREATE database " + _databaseName, con);
                cmd.ExecuteNonQuery();

                var cmdCreate =
                    server.GetCommand(@"CREATE TABLE " + _databaseName + @".Fish 
(
id int not null,
name varchar(10) null,
height decimal(2,1) not null,
chi char(10),
myfloat float,
mydouble double precision,
chiAsNumeric numeric(10,0),
teenynumber smallint,
CONSTRAINT pk_Fish PRIMARY KEY (id, height)
)",
                        con);
                cmdCreate.ExecuteNonQuery();
            }
        }

        [TestFixtureTearDown]
        public void DeleteTestDatabase()
        {
            if (server == null)
                return;

            using (var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand("DROP database " + _databaseName, con);
                cmd.ExecuteNonQuery();
            }
        }

        [SetUp]
        public void BeforeEveryTest()
        {
            if (!_isServerAvailable)
                Assert.Inconclusive("No MySQL Server available to test machine");
        }

        [Test]
        public void ListDatabases()
        {
            var databases = server.DiscoverDatabases();
            Assert.AreEqual(1,databases.Count(db => db.GetRuntimeName().Equals(_databaseName.ToLower())));
        }

        [Test]
        public void ListTables()
        {
            var tables = server.ExpectDatabase(_databaseName).DiscoverTables(false);
            Assert.IsTrue(tables.Count(t=>t.GetRuntimeName().Equals("fish"))==1);
        }

        [Test]
        public void RowCount_WithTransaction()
        {
            var table = server.ExpectDatabase(_databaseName).ExpectTable("Fish");

            //now rows to start with
            Assert.AreEqual(0, table.GetRowCount());

            using (var trans = server.BeginNewTransactedConnection())
            {
                server.GetCommand("INSERT INTO " + _databaseName + ".Fish VALUES (1,'flibble',1.5,'0101010101',2.5,1.1,1000,1)", trans).ExecuteNonQuery();

                Assert.AreEqual(1, table.GetRowCount(trans.ManagedTransaction));

                trans.ManagedTransaction.AbandonAndCloseConnection();
            }

            Assert.AreEqual(0, table.GetRowCount());
        }


        [Test]
        public void BulkInsert()
        {
            var table = server.ExpectDatabase(_databaseName).ExpectTable("Fish");

            //now rows to start with
            Assert.AreEqual(0, table.GetRowCount());

            using (var i = table.BeginBulkInsert())
            {
                var dt = new DataTable();
                dt.Columns.Add("id");
                dt.Columns.Add("name");
                dt.Columns.Add("height");
                dt.Columns.Add("chi");
                dt.Columns.Add("myfloat");
                dt.Columns.Add("mydouble");
                dt.Columns.Add("chiAsNumeric");
                dt.Columns.Add("teenynumber");

                dt.Rows.Add("10", "flibble", 1.5, "0101010101", 2.5, 1.1, 1000, 5);
                dt.Rows.Add("11", "bandycoot", 1.1, "0202020202", 1.5, 1.2, 2000, 8);
                i.Upload(dt);
            }

            Assert.AreEqual(2, table.GetRowCount());

            using (var con = table.Database.Server.GetConnection())
            {
                var r = new MySqlCommand("select * from Fish", (MySqlConnection)con).ExecuteReader();
                Assert.IsTrue(r.Read());

                Assert.AreEqual(10, r["id"]);
                Assert.AreEqual("flibble", r["name"]);
                Assert.AreEqual(1.5, r["height"]);
                Assert.AreEqual("0101010101", r["chi"]);
                Assert.AreEqual(2.5, r["myfloat"]);
                Assert.AreEqual(1.1, r["mydouble"]);
                Assert.AreEqual(1000, r["chiAsNumeric"]);
                Assert.AreEqual(5, r["teenynumber"]);

                Assert.IsTrue(r.Read());

                Assert.AreEqual(11, r["id"]);
                Assert.AreEqual("bandycoot", r["name"]);
                Assert.AreEqual(1.1, r["height"]);
                Assert.AreEqual("0202020202", r["chi"]);
                Assert.AreEqual(1.5, r["myfloat"]);
                Assert.AreEqual(1.2, r["mydouble"]);
                Assert.AreEqual(2000, r["chiAsNumeric"]);
                Assert.AreEqual(8, r["teenynumber"]);
                r.Close();
            }
            
            table.Truncate();

            Assert.AreEqual(0, table.GetRowCount());

        }

        [Test]
        public void RowCount()
        {
            var table = server.ExpectDatabase(_databaseName).ExpectTable("Fish");

            //now rows to start with
            Assert.AreEqual(0, table.GetRowCount());

            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand("INSERT INTO " + _databaseName + ".Fish VALUES (1,'flibble',1.5,'0101010101',2.5,1.1,1000,1)", con)
                    .ExecuteNonQuery();

                Assert.AreEqual(1, table.GetRowCount());

                server.GetCommand("DELETE FROM " + _databaseName + ".Fish", con)
                    .ExecuteNonQuery();

                Assert.AreEqual(0, table.GetRowCount());
            }
        }

        [Test]
        public void ListColumns()
        {
            var cols = server.ExpectDatabase(_databaseName).ExpectTable("fish").DiscoverColumns();

            Assert.AreEqual("int",cols.Single(c=>c.GetRuntimeName().Equals("id")).DataType.SQLType);
            Assert.AreEqual("varchar(10)", cols.Single(c => c.GetRuntimeName().Equals("name")).DataType.SQLType);
            Assert.AreEqual("decimal(2,1)", cols.Single(c => c.GetRuntimeName().Equals("height")).DataType.SQLType);

            Assert.AreEqual("char(10)", cols.Single(c => c.GetRuntimeName().Equals("chi")).DataType.SQLType);
            Assert.AreEqual("float", cols.Single(c => c.GetRuntimeName().Equals("myfloat")).DataType.SQLType);
            Assert.AreEqual("double", cols.Single(c => c.GetRuntimeName().Equals("mydouble")).DataType.SQLType);
            Assert.AreEqual("decimal(10,0)", cols.Single(c => c.GetRuntimeName().Equals("chiasnumeric")).DataType.SQLType);
            Assert.AreEqual("smallint", cols.Single(c => c.GetRuntimeName().Equals("teenynumber")).DataType.SQLType);   
        }

        [Test]
        public void GetPrimaryKeys()
        {
            var cols = server.ExpectDatabase(_databaseName).ExpectTable("fish").DiscoverColumns();

            Assert.IsTrue(cols.Single(c=>c.GetRuntimeName().Equals("id")).IsPrimaryKey);
            Assert.IsTrue(cols.Single(c=>c.GetRuntimeName().Equals("height")).IsPrimaryKey);

            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("chi")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("myfloat")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c => c.GetRuntimeName().Equals("chiasnumeric")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("teenynumber")).IsPrimaryKey);
        }

    }
}
