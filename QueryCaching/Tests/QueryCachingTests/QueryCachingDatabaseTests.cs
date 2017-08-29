using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using DataLoadEngine.DatabaseManagement.Operations;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;
using AcceptAllCheckNotifier = ReusableLibraryCode.Checks.AcceptAllCheckNotifier;

namespace QueryCachingTests
{
    public class QueryCachingDatabaseTests:DatabaseTests
    {
        protected string QueryCachingDatabaseName = Tests.Common.TestDatabaseNames.GetConsistentName("QueryCaching");
        public DiscoveredDatabase DiscoveredQueryCachingDatabase { get; set; }
        public ExternalDatabaseServer QueryCachingDatabaseServer;

        [TestFixtureSetUp]
        public void Setup()
        {
            DiscoveredQueryCachingDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(QueryCachingDatabaseName);

            MasterDatabaseScriptExecutor scripter = new MasterDatabaseScriptExecutor(DiscoveredQueryCachingDatabase);
            scripter.CreateAndPatchDatabaseWithDotDatabaseAssembly(typeof(QueryCaching.Database.Class1).Assembly, new ThrowImmediatelyCheckNotifier());

            QueryCachingDatabaseServer = new ExternalDatabaseServer(CatalogueRepository,QueryCachingDatabaseName);
            QueryCachingDatabaseServer.SetProperties(DiscoveredQueryCachingDatabase);
        }

        [TestFixtureTearDown]
        public void Destroy()
        {
            QueryCachingDatabaseServer.DeleteInDatabase();

            if (DiscoveredQueryCachingDatabase.Exists())
                DiscoveredQueryCachingDatabase.ForceDrop();
        }
    }
}
