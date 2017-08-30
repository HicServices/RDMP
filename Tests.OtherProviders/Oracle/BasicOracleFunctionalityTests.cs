using System.Linq;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace Tests.OtherProviders.Oracle
{
    [TestFixture]
    [Category("Oracle")]
    public class BasicOracleFunctionalityTests : DatabaseTests
    {
        DiscoveredServer server;
        private DiscoveredDatabase _database;
        private bool _isServerAvailable = true;

        [TestFixtureSetUp]
        public void CreateTestDatabase()
        {
            if (DiscoveredOracleServer == null)
            {
                _isServerAvailable = false;
                return;
            }

            server = DiscoveredOracleServer;
            if (!server.Exists())
            {
                _isServerAvailable = false;
                return;
            }

            //cleanup 
            _database = server.ExpectDatabase("BOB");

            DeleteTestDatabase();

            Assert.IsFalse(_database.Exists());

            //create bob
            server.CreateDatabase("BOB");
            
            //create a table in it
            using (var con = server.GetConnection())
            {
                con.Open();

               
                var cmdCreate =
                    server.GetCommand(@"CREATE TABLE BOB.Fish 
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

            if(_database.Exists())
            {
                if ((_database.ExpectTable("FISH").Exists()))
                {
                    _database.ExpectTable("FISH").Drop();
                    Assert.IsFalse(_database.ExpectTable("FISH").Exists());
                }
                _database.Drop();
            }
        }

        [SetUp]
        public void BeforeEveryTest()
        {
            if (!_isServerAvailable)
                Assert.Inconclusive("No Oracle server is available, cannot run test.");
        }

        [Test]
        public void ListDatabases()
        {
            var databases = server.DiscoverDatabases();
            Assert.IsTrue(databases.Count(db => db.GetRuntimeName().Equals("BOB")) == 1);
        }

        [Test]
        public void ListTables()
        {
            var tables = server.ExpectDatabase("BOB").DiscoverTables(false);
            Assert.IsTrue(tables.Count(t=>t.GetRuntimeName().Equals("FISH"))==1);
        }

        [Test]
        public void RowCount_WithTransaction()
        {
            var table = server.ExpectDatabase("BOB").ExpectTable("FISH");

            //now rows to start with
            Assert.AreEqual(0, table.GetRowCount());

            using (var trans = server.BeginNewTransactedConnection())
            {
                server.GetCommand("INSERT INTO BOB.FISH VALUES (1,'flibble',1.5,'0101010101',2.5,1.1,1000,1)", trans).ExecuteNonQuery();

                Assert.AreEqual(1, table.GetRowCount(trans.ManagedTransaction));

                trans.ManagedTransaction.AbandonAndCloseConnection();
            }

            Assert.AreEqual(0, table.GetRowCount());
        }

        [Test]
        public void RowCount()
        {
            var table = server.ExpectDatabase("BOB").ExpectTable("FISH");

            //now rows to start with
            Assert.AreEqual(0, table.GetRowCount());

            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand("INSERT INTO BOB.FISH VALUES (1,'flibble',1.5,'0101010101',2.5,1.1,1000,1)", con)
                    .ExecuteNonQuery();

                Assert.AreEqual(1, table.GetRowCount());

                server.GetCommand("DELETE FROM BOB.FISH", con)
                    .ExecuteNonQuery();

                Assert.AreEqual(0, table.GetRowCount());
            }
        }

        [Test]
        public void ListColumns()
        {
            var cols = server.ExpectDatabase("BOB").ExpectTable("FISH").DiscoverColumns();

            Assert.AreEqual("int", cols.Single(c => c.GetRuntimeName().Equals("ID")).DataType.SQLType);
            Assert.AreEqual(false, cols.Single(c => c.GetRuntimeName().Equals("ID")).AllowNulls);

            Assert.AreEqual("varchar(10)", cols.Single(c => c.GetRuntimeName().Equals("NAME")).DataType.SQLType);
            Assert.AreEqual(true, cols.Single(c => c.GetRuntimeName().Equals("NAME")).AllowNulls);

            Assert.AreEqual("decimal(2,1)", cols.Single(c => c.GetRuntimeName().Equals("HEIGHT")).DataType.SQLType);

            Assert.AreEqual("char(10)", cols.Single(c => c.GetRuntimeName().Equals("CHI")).DataType.SQLType);
            Assert.AreEqual("double", cols.Single(c => c.GetRuntimeName().Equals("MYFLOAT")).DataType.SQLType);
            Assert.AreEqual("double", cols.Single(c => c.GetRuntimeName().Equals("MYDOUBLE")).DataType.SQLType);
            Assert.AreEqual("decimal(10,0)", cols.Single(c => c.GetRuntimeName().Equals("CHIASNUMERIC")).DataType.SQLType);
            Assert.AreEqual("int", cols.Single(c => c.GetRuntimeName().Equals("TEENYNUMBER")).DataType.SQLType);   
        }

        [Test]
        public void GetPrimaryKeys()
        {
            var cols = server.ExpectDatabase("BOB").ExpectTable("FISH").DiscoverColumns();

            Assert.IsTrue(cols.Single(c=>c.GetRuntimeName().Equals("ID")).IsPrimaryKey);
            Assert.IsTrue(cols.Single(c=>c.GetRuntimeName().Equals("HEIGHT")).IsPrimaryKey);

            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("CHI")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("MYFLOAT")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("CHIASNUMERIC")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("TEENYNUMBER")).IsPrimaryKey);
        }
    }
}
