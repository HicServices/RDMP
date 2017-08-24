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
        public ExternalDatabaseServer QueryCachingDatabaseServer;
        
        [TestFixtureSetUp]
        public void Setup()
        {
            MasterDatabaseScriptExecutor scripter = new MasterDatabaseScriptExecutor(ServerICanCreateRandomDatabasesAndTablesOn.DataSource, QueryCachingDatabaseName, ServerICanCreateRandomDatabasesAndTablesOn.UserID, ServerICanCreateRandomDatabasesAndTablesOn.Password);
            scripter.CreateAndPatchDatabaseWithDotDatabaseAssembly(typeof(QueryCaching.Database.Class1).Assembly, new ThrowImmediatelyCheckNotifier());

            QueryCachingDatabaseServer = new ExternalDatabaseServer(CatalogueRepository,QueryCachingDatabaseName);
            QueryCachingDatabaseServer.Server = ServerICanCreateRandomDatabasesAndTablesOn.DataSource;
            QueryCachingDatabaseServer.Database = QueryCachingDatabaseName;
            QueryCachingDatabaseServer.Username = ServerICanCreateRandomDatabasesAndTablesOn.UserID;
            QueryCachingDatabaseServer.Password = ServerICanCreateRandomDatabasesAndTablesOn.Password;
            QueryCachingDatabaseServer.SaveToDatabase();
        }
        
        [TestFixtureTearDown]
        public void Destroy()
        {
            QueryCachingDatabaseServer.DeleteInDatabase();

            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(QueryCachingDatabaseName).ForceDrop();
        }
    }
}
