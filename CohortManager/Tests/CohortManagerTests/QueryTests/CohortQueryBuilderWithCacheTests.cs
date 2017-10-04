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
        private DiscoveredDatabase queryCacheDatabase;
        private ExternalDatabaseServer externalDatabaseServer;
        private DatabaseColumnRequest _chiColumnSpecification = new DatabaseColumnRequest("chi","varchar(10)");
        
        [TestFixtureSetUp]
        public void SetUpCache()
        {
            queryCacheDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(TestDatabaseNames.Prefix + "QueryCache");

            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(queryCacheDatabase);

            Console.WriteLine("QueryCachingDatabaseIs" + typeof (Class1).Assembly.FullName);

            executor.CreateAndPatchDatabase(typeof(CachedAggregateConfigurationResultsManager).Assembly,new ThrowImmediatelyCheckNotifier());
            
            externalDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, "QueryCacheForUnitTests");
            externalDatabaseServer.SetProperties(queryCacheDatabase);
        }

        [TestFixtureTearDown]
        public void DropDatabases()
        {
            queryCacheDatabase.ForceDrop();
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
                Assert.AreEqual(
CollapseWhitespace(
string.Format(
@"
(
	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData]
)
",cohortIdentificationConfiguration.ID)), 
 CollapseWhitespace(builder.SQL));

                var server = queryCacheDatabase.Server;
                using(var con = server.GetConnection())
                {
                    con.Open();

                    var da = server.GetDataAdapter(builder.SQL, con);
                    var dt = new DataTable();
                    da.Fill(dt);

                    manager.CommitResults(new CacheCommitIdentifierList(aggregate1,
                        string.Format(@"/*cic_{0}_UnitTestAggregate1*/
SELECT
distinct
[" +TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
FROM 
[" + TestDatabaseNames.Prefix + @"ScratchArea]..[BulkData]", cohortIdentificationConfiguration.ID), dt, _chiColumnSpecification, 30));
                }


                CohortQueryBuilder builderCached = new CohortQueryBuilder(cohortIdentificationConfiguration);

                Assert.AreEqual(
                    CollapseWhitespace(
                    string.Format(
@"
(
	/*Cached:cic_{0}_UnitTestAggregate1*/
	select * from [" + queryCacheDatabase.GetRuntimeName() + "]..[IndexedExtractionIdentifierList_AggregateConfiguration" + aggregate1.ID + @"]

)
",cohortIdentificationConfiguration.ID)),
 CollapseWhitespace(builderCached.SQL));

            }
            finally
            {
                cohortIdentificationConfiguration.RootCohortAggregateContainer.RemoveChild(aggregate1);
                
            }

        }
    }
}