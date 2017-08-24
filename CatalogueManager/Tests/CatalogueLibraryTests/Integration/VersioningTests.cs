using System.Linq;

using DataLoadEngine.DatabaseManagement.Operations;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    [TestFixture]
    public class VersioningTests :DatabaseTests
    {
        [Test]
        public void MasterDatabaseScriptExecutor_CreateDatabase()
        {
            var target = ServerICanCreateRandomDatabasesAndTablesOn;
            string dbName = "CreateANewCatalogueDatabaseWithMasterDatabaseScriptExecutor";

            var database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);
            if (database != null)
                database.ForceDrop();

            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(target.DataSource, dbName, target.UserID, target.Password);
            executor.CreateDatabase(@"
CREATE TABLE Bob
(
age int
)
GO", "1.0.0.0", new ThrowImmediatelyCheckNotifier());

            var db = new DiscoveredServer(ServerICanCreateRandomDatabasesAndTablesOn).ExpectDatabase(dbName);
            var versionTable = db.ExpectTable("Version");
            var bobTable = db.ExpectTable("Bob");

            Assert.IsTrue(versionTable.Exists());
            Assert.IsTrue(bobTable.Exists());

            if (db.Exists())
            {
                var tables = db.DiscoverTables(true);
                foreach (var table in tables.Where(table => table.Exists()))
                {
                    table.Drop();
                }

                db.Drop();
            }
        }

    }
}
