using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using QueryCachingTests;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CohortManagerTests.QueryTests
{
    public class JoinableCohortConfigurationTests : CohortIdentificationTests
    {

        [Test]
        public void CreateJoinable()
        {
            JoinableCohortAggregateConfiguration joinable = null;
            try
            {
                joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);

                Assert.AreEqual(joinable.CohortIdentificationConfiguration_ID, cohortIdentificationConfiguration.ID);
                Assert.AreEqual(joinable.AggregateConfiguration_ID, aggregate1.ID);
            }
            finally
            {
                if(joinable != null)
                    joinable.DeleteInDatabase();
            }
        }

        [Test]
        public void CreateJoinable_IsAlreadyInAContainer()
        {
            cohortIdentificationConfiguration.RootCohortAggregateContainer.AddChild(aggregate1,1);
            
            var ex = Assert.Throws<NotSupportedException>(() => new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1));
            Assert.AreEqual("Cannot make aggregate UnitTestAggregate1 into a Joinable aggregate because it is already in a CohortAggregateContainer", ex.Message);
            cohortIdentificationConfiguration.RootCohortAggregateContainer.RemoveChild(aggregate1);
        }

        [Test]
        public void CreateJoinable_NoIsExtractionIdentifier()
        {
            //delete the first dimension (chi)
            aggregate1.AggregateDimensions.First().DeleteInDatabase();

            var ex = Assert.Throws<NotSupportedException>(()=>new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1));
            Assert.AreEqual("Cannot make aggregate UnitTestAggregate1 into a Joinable aggregate because it has 0 columns marked IsExtractionIdentifier", ex.Message);
        }

        [Test]
        public void CreateJoinable_AddTwice()
        {
            //delete the first dimension (chi)
            var join1 = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);
            try
            {
                var ex = Assert.Throws<SqlException>(() => new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1));
                Assert.IsTrue(ex.Message.Contains("ix_eachAggregateCanOnlyBeJoinableOnOneProject"));
            }
            finally
            {
                join1.DeleteInDatabase();
            }
        }

        [Test]
        public void CreateUsers()
        {
            JoinableCohortAggregateConfiguration joinable = null; 
            try
            {
                joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);
                joinable.AddUser(aggregate2);

                Assert.IsTrue(joinable.Users.Length == 1);
                Assert.AreEqual(aggregate2,joinable.Users[0].AggregateConfiguration);
            }
            finally 
            {
                if (joinable != null) 
                    joinable.DeleteInDatabase();
            }
        }

        [Test]
        public void CreateUsers_DuplicateUser()
        {
            var joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);
            try
            {
                joinable.AddUser(aggregate2);
                Assert.Throws<SqlException>(()=>joinable.AddUser(aggregate2));
            }
            finally
            {
                joinable.DeleteInDatabase();
            }
        }

        [Test]
        public void CreateUsers_SelfReferrential()
        {
            var joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);
            try
            {
                var ex = Assert.Throws<NotSupportedException>(()=>joinable.AddUser(aggregate1));
                Assert.AreEqual("Cannot configure AggregateConfiguration UnitTestAggregate1 as a Join user to itself!", ex.Message);
            }
            finally
            {
                joinable.DeleteInDatabase();
            }
        }

        [Test]
        public void CreateUsers_ToAnyOtherJoinable()
        {
            var joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);
            var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate2);
            try
            {
                var ex = Assert.Throws<NotSupportedException>(() => joinable.AddUser(aggregate2));
                Assert.AreEqual("Cannot add user UnitTestAggregate2 because that AggregateConfiguration is itself a JoinableCohortAggregateConfiguration", ex.Message);
            }
            finally
            {
                joinable.DeleteInDatabase();
                joinable2.DeleteInDatabase();
            }
        }
        [Test]
        public void CreateUsers_ToNoExtractionIdentifierTable()
        {
            var joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);

            aggregate2.AggregateDimensions.First().DeleteInDatabase();

            try
            {
                var ex = Assert.Throws<NotSupportedException>(() => joinable.AddUser(aggregate2));
                Assert.AreEqual("Cannot configure AggregateConfiguration UnitTestAggregate2 as join user because it does not contain exactly 1 IsExtractionIdentifier dimension", ex.Message);
            }
            finally
            {
                joinable.DeleteInDatabase();
            }
        }

        [Test]
        public void QueryBuilderTest()
        {
            var builder = new CohortQueryBuilder(aggregate1, null);
            
            //make aggregate 2 a joinable
            var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository,cohortIdentificationConfiguration, aggregate2);
            joinable2.AddUser(aggregate1);


            Console.WriteLine(builder.SQL);
            try
            {
                using (var con = (SqlConnection) DiscoveredDatabaseICanCreateRandomTablesIn.Server.GetConnection())
                {
                    con.Open();

                    var dbReader = new SqlCommand(builder.SQL, con).ExecuteReader();
                    
                    //can read at least one row
                    Assert.IsTrue(dbReader.Read());
                }

                string expectedTableAlias = "ix" + joinable2.ID;

                //after joinables
                Assert.AreEqual(
                    string.Format(
    @"/*UnitTestAggregate1*/
SELECT distinct
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData] LEFT Join (
	/*UnitTestAggregate2*/
	SELECT distinct
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData]
){0}
on ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi] = {0}.chi",expectedTableAlias), builder.SQL);

            }
            finally
            {
                joinable2.Users[0].DeleteInDatabase();
                joinable2.DeleteInDatabase();
            }
        }

        [Test]
        public void QueryBuilderTest_AdditionalColumn()
        {
            var anotherCol = aggregate2.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("dtCreated"));
            aggregate2.AddDimension(anotherCol);

            //make aggregate 2 a joinable
            var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate2);
            joinable2.AddUser(aggregate1);

            string expectedTableAlias = "ix" + joinable2.ID;

            var filterContainer1 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            var filterContainer2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);

            var filter1 = new AggregateFilter(CatalogueRepository, "Within 1 year of event", filterContainer1);
            var filter2 = new AggregateFilter(CatalogueRepository, "DateAfter2001", filterContainer2);

            filter1.WhereSQL = string.Format("ABS(DATEDIFF(year, {0}.dtCreated, ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].dtCreated)) <= 1",expectedTableAlias);
            filter1.SaveToDatabase();

            filter2.WhereSQL = "dtCreated > '2001-01-01'";
            filter2.SaveToDatabase();

            aggregate1.RootFilterContainer_ID = filterContainer1.ID;
            aggregate1.SaveToDatabase();

            aggregate2.RootFilterContainer_ID = filterContainer2.ID;
            aggregate2.SaveToDatabase();

            var builder = new CohortQueryBuilder(aggregate1, null);


            Console.WriteLine(builder.SQL);

            
            try
            {
                using (var con = (SqlConnection)DiscoveredDatabaseICanCreateRandomTablesIn.Server.GetConnection())
                {
                    con.Open();

                    var dbReader = new SqlCommand(builder.SQL, con).ExecuteReader();

                    //can read at least one row
                    Assert.IsTrue(dbReader.Read());
                }


                //after joinables
                Assert.AreEqual(
                    string.Format(
    @"/*UnitTestAggregate1*/
SELECT distinct
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData] LEFT Join (
	/*UnitTestAggregate2*/
	SELECT distinct
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi], ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[dtCreated]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData]
	WHERE
	(
	/*DateAfter2001*/
	dtCreated > '2001-01-01'
	)
){0}
on ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi] = {0}.chi

WHERE
(
/*Within 1 year of event*/
ABS(DATEDIFF(year, {0}.dtCreated, ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].dtCreated)) <= 1
)", expectedTableAlias), builder.SQL);

            }
            finally
            {
                filter1.DeleteInDatabase();
                filter2.DeleteInDatabase();

                filterContainer1.DeleteInDatabase();

                filterContainer2.DeleteInDatabase();
                
                joinable2.Users[0].DeleteInDatabase();
                joinable2.DeleteInDatabase();
            }
        }

        [Test]
        public void QueryBuilderTest_JoinableCloning()
        {
            var anotherCol = aggregate2.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("dtCreated"));
            aggregate2.AddDimension(anotherCol);

            //make aggregate 2 a joinable
            var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate2);
            joinable2.AddUser(aggregate1);

            string expectedTableAlias = "ix" + joinable2.ID;

            var filterContainer1 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            var filterContainer2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);

            var filter1 = new AggregateFilter(CatalogueRepository, "Within 1 year of event", filterContainer1);
            var filter2 = new AggregateFilter(CatalogueRepository, "DateAfter2001", filterContainer2);

            filter1.WhereSQL = string.Format("ABS(DATEDIFF(year, {0}.dtCreated, ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].dtCreated)) <= 1", expectedTableAlias);
            filter1.SaveToDatabase();

            filter2.WhereSQL = "dtCreated > '2001-01-01'";
            filter2.SaveToDatabase();

            aggregate1.RootFilterContainer_ID = filterContainer1.ID;
            aggregate1.SaveToDatabase();

            aggregate2.RootFilterContainer_ID = filterContainer2.ID;
            aggregate2.SaveToDatabase();

            //add the first aggregate to the configuration
            rootcontainer.AddChild(aggregate1,1);
            
            var globalParameter = new AnyTableSqlParameter(CatalogueRepository, cohortIdentificationConfiguration,"DECLARE @fish varchar(50)");
            globalParameter.Comment = "Comments for the crazies";
            globalParameter.Value = "'fishes'";
            globalParameter.SaveToDatabase();

            var builder = new CohortQueryBuilder(cohortIdentificationConfiguration);

            try
            {
                var clone = cohortIdentificationConfiguration.CreateClone(new ThrowImmediatelyCheckNotifier());

                var cloneBuilder = new CohortQueryBuilder(clone);

                string origSql = builder.SQL;
                string cloneOrigSql = cloneBuilder.SQL;

                Console.WriteLine("//////////////////////////////////////////////VERBATIM//////////////////////////////////////////////");
                Console.WriteLine(origSql);
                Console.WriteLine(cloneOrigSql);
                Console.WriteLine("//////////////////////////////////////////////END VERBATIM//////////////////////////////////////////////");

                var builderSql = Regex.Replace(Regex.Replace(origSql, "cic_[0-9]+_", ""), "ix[0-9]+", "ix");
                var cloneBuilderSql = Regex.Replace(Regex.Replace(cloneOrigSql, "cic_[0-9]+_", ""), "ix[0-9]+", "ix").Replace("(Clone)", "");//get rid of explicit ix53 etc for the comparison

                Console.WriteLine("//////////////////////////////////////////////TEST COMPARISON IS//////////////////////////////////////////////");
                Console.WriteLine(builderSql);
                Console.WriteLine(cloneBuilderSql);
                Console.WriteLine("//////////////////////////////////////////////END COMPARISON//////////////////////////////////////////////");

                Assert.AreEqual(builderSql, cloneBuilderSql);


                ////////////////Cleanup Database//////////////////////////////
                //find the WHERE logic too
                var containerClone = clone.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()//get all the aggregates
                    .Union(clone.GetAllJoinables().Select(j=>j.AggregateConfiguration))//including the joinables
                    .Where(a => a.RootFilterContainer_ID != null)//that have WHERE sql
                    .Select(ag => ag.RootFilterContainer);//grab their containers so we can clean them up

                ((IDeleteable)clone.GetAllParameters()[0]).DeleteInDatabase();
                clone.DeleteInDatabase();

                //delete the WHERE logic too
                foreach (AggregateFilterContainer c in containerClone)
                    c.DeleteInDatabase();
            }
            finally
            {
                rootcontainer.RemoveChild(aggregate1);

                filter1.DeleteInDatabase();
                filter2.DeleteInDatabase();

                filterContainer1.DeleteInDatabase();

                filterContainer2.DeleteInDatabase();

                joinable2.Users[0].DeleteInDatabase();
                joinable2.DeleteInDatabase();

                globalParameter.DeleteInDatabase();
            }
        }


        [Test]
        public void JoinablesWithCache()
        {
            const string queryCachingDatabaseName = "MyQueryCachingDatabase";
            var builder = new CohortQueryBuilder(aggregate1, null);

            //make aggregate 2 a joinable
            var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate2);
            joinable2.AddUser(aggregate1);

            //make aggregate 2 have an additional column (dtCreated)
            var anotherCol = aggregate2.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("dtCreated"));
            aggregate2.AddDimension(anotherCol);

            var remnantDb = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(queryCachingDatabaseName);
            if(remnantDb.Exists())
                remnantDb.ForceDrop();

            MasterDatabaseScriptExecutor scripter = new MasterDatabaseScriptExecutor(ServerICanCreateRandomDatabasesAndTablesOn.DataSource, queryCachingDatabaseName, ServerICanCreateRandomDatabasesAndTablesOn.UserID, ServerICanCreateRandomDatabasesAndTablesOn.Password);
            scripter.CreateAndPatchDatabaseWithDotDatabaseAssembly(typeof(QueryCaching.Database.Class1).Assembly, new AcceptAllCheckNotifier());

            var queryCachingDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, queryCachingDatabaseName);
            queryCachingDatabaseServer.Server = ServerICanCreateRandomDatabasesAndTablesOn.DataSource;
            queryCachingDatabaseServer.Database = queryCachingDatabaseName;
            queryCachingDatabaseServer.Username = ServerICanCreateRandomDatabasesAndTablesOn.UserID;
            queryCachingDatabaseServer.Password = ServerICanCreateRandomDatabasesAndTablesOn.Password;
            queryCachingDatabaseServer.SaveToDatabase();

            //make the builder use the query cache we just set up
            builder.CacheServer = queryCachingDatabaseServer;
            try
            {
               var builderForCaching = new CohortQueryBuilder(aggregate2, null, true);

                var cacheDt = new DataTable();
                using (SqlConnection con = (SqlConnection)DiscoveredDatabaseICanCreateRandomTablesIn.Server.GetConnection())
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(builderForCaching.SQL, con));
                    da.Fill(cacheDt);
                }

                var cacheManager = new CachedAggregateConfigurationResultsManager(queryCachingDatabaseServer);
                cacheManager.CommitResults(new CacheCommitJoinableInceptionQuery(aggregate2, builderForCaching.SQL, cacheDt, null,30));

                try
                {
                    Console.WriteLine(builder.SQL);

                    using (var con = (SqlConnection)DiscoveredDatabaseICanCreateRandomTablesIn.Server.GetConnection())
                    {
                        con.Open();

                        var dbReader = new SqlCommand(builder.SQL, con).ExecuteReader();

                        //can read at least one row
                        Assert.IsTrue(dbReader.Read());
                    }

                    string expectedTableAlias = "ix" + joinable2.ID;

                    //after joinables
                    Assert.AreEqual(
                        string.Format(
        @"/*UnitTestAggregate1*/
SELECT distinct
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData] LEFT Join (
	/*Cached:UnitTestAggregate2*/
	select * from [MyQueryCachingDatabase]..[JoinableInceptionQuery_AggregateConfiguration{1}]

){0}
on ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[chi] = {0}.chi", expectedTableAlias,aggregate2.ID), builder.SQL);

                }
                finally
                {
                    joinable2.Users[0].DeleteInDatabase();
                    joinable2.DeleteInDatabase();
                }
            }
            finally 
            {

                queryCachingDatabaseServer.DeleteInDatabase();
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(queryCachingDatabaseName).ForceDrop();
                
            }
            
            

           
        }
    }
}
