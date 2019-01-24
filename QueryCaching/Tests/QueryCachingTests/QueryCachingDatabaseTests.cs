using CatalogueLibrary.Data;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace QueryCachingTests
{
    public class QueryCachingDatabaseTests:DatabaseTests
    {
        protected string QueryCachingDatabaseName = Tests.Common.TestDatabaseNames.GetConsistentName("QueryCaching");
        public DiscoveredDatabase DiscoveredQueryCachingDatabase { get; set; }
        public ExternalDatabaseServer QueryCachingDatabaseServer;

        [OneTimeSetUp]
        public void Setup()
        {
            DiscoveredQueryCachingDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(QueryCachingDatabaseName);

            if(DiscoveredQueryCachingDatabase.Exists())
                DiscoveredQueryCachingDatabase.Drop();

            MasterDatabaseScriptExecutor scripter = new MasterDatabaseScriptExecutor(DiscoveredQueryCachingDatabase);
            scripter.CreateAndPatchDatabaseWithDotDatabaseAssembly(typeof(QueryCaching.Database.Class1).Assembly, new ThrowImmediatelyCheckNotifier());

            QueryCachingDatabaseServer = new ExternalDatabaseServer(CatalogueRepository,QueryCachingDatabaseName);
            QueryCachingDatabaseServer.SetProperties(DiscoveredQueryCachingDatabase);
        }

        [OneTimeTearDown]
        public void Destroy()
        {
            QueryCachingDatabaseServer.DeleteInDatabase();

            if (DiscoveredQueryCachingDatabase.Exists())
                DiscoveredQueryCachingDatabase.Drop();
        }
    }
}
