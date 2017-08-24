using System;
using System.Data;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using QueryCaching.Database;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CohortManagerTests.QueryTests
{
    public class CohortQueryBuilderWithCacheTests : CohortIdentificationTests
    {
        private SqlConnectionStringBuilder queryCacheBuilder;
        private ExternalDatabaseServer externalDatabaseServer;
        
        [TestFixtureSetUp]
        public void SetUpCache()
        {
            queryCacheBuilder = new SqlConnectionStringBuilder(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString);
            queryCacheBuilder.InitialCatalog = TestDatabaseNames.Prefix + "QueryCache";
            
            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(queryCacheBuilder.ConnectionString);

            Console.WriteLine("QueryCachingDatabaseIs" + typeof (Class1).Assembly.FullName);

            executor.CreateAndPatchDatabase(typeof(CachedAggregateConfigurationResultsManager).Assembly,new ThrowImmediatelyCheckNotifier());
            
            externalDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, "QueryCacheForUnitTests");
            
            externalDatabaseServer.Server = queryCacheBuilder.DataSource;
            externalDatabaseServer.Database = queryCacheBuilder.InitialCatalog;
            externalDatabaseServer.Username = queryCacheBuilder.UserID;
            externalDatabaseServer.Password = queryCacheBuilder.Password;
            externalDatabaseServer.SaveToDatabase();


        }

        [TestFixtureTearDown]
        public void DropDatabases()
        {
            
            new DiscoveredServer(queryCacheBuilder).ExpectDatabase(queryCacheBuilder.InitialCatalog).ForceDrop();
            externalDatabaseServer.DeleteInDatabase();
        }

        [Test]
        public void TestGettingAggregateJustFromConfig_DistinctCHISelect()
        {

            CachedAggregateConfigurationResultsManager manager = new CachedAggregateConfigurationResultsManager( externalDatabaseServer);
            
            cohortIdentificationConfiguration.QueryCachingServer_ID = externalDatabaseServer.ID;
            cohortIdentificationConfiguration.SaveToDatabase();
            

            CohortQueryBuilder builder = new CohortQueryBuilder(cohortIdentificationConfiguration);
            cohortIdentificationConfiguration.CreateRootContainerIfNotExists();
            cohortIdentificationConfiguration.RootCohortAggregateContainer.AddChild(aggregate1,0);
            try
            {
                Assert.AreEqual(@"
(
	/*UnitTestAggregate1*/
	SELECT distinct
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData]
)
", builder.SQL);

                using(SqlConnection con= new SqlConnection(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString))
                {
                    con.Open();

                    SqlDataAdapter da = new SqlDataAdapter(builder.SQL,con);
                    var dt = new DataTable();
                    da.Fill(dt);

                    manager.CommitResults(new CacheCommitIdentifierList(aggregate1, @"/*UnitTestAggregate1*/
SELECT distinct
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData]", dt, null, 30));
                }


                CohortQueryBuilder builderCached = new CohortQueryBuilder(cohortIdentificationConfiguration);

                Assert.AreEqual(@"
(
	/*Cached:UnitTestAggregate1*/
	select * from ["+queryCacheBuilder.InitialCatalog+"]..[IndexedExtractionIdentifierList_AggregateConfiguration" + aggregate1.ID + @"]

)
", builderCached.SQL);

            }
            finally
            {
                cohortIdentificationConfiguration.RootCohortAggregateContainer.RemoveChild(aggregate1);
                
            }

        }
    }
}