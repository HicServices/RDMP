using System.Linq;

using DataLoadEngine.DatabaseManagement.Operations;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    
    public class VersioningTests :DatabaseTests
    {
        [Test]
        public void MasterDatabaseScriptExecutor_CreateDatabase()
        {
            string dbName = "CreateANewCatalogueDatabaseWithMasterDatabaseScriptExecutor";

            var database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);
            database.Drop();

            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(database);
            executor.CreateDatabase(@"
CREATE TABLE Bob
(
age int
)
GO", "1.0.0.0", new ThrowImmediatelyCheckNotifier());

            var versionTable = database.ExpectTable("Version");
            var bobTable = database.ExpectTable("Bob");

            Assert.IsTrue(versionTable.Exists());
            Assert.IsTrue(bobTable.Exists());

            database.Drop();
        }

    }
}
