// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation.QueryTests;

public class JoinableCohortConfigurationTests : CohortIdentificationTests
{
    private DiscoveredDatabase _queryCachingDatabase;

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
            joinable?.DeleteInDatabase();
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
        aggregate1.ClearAllInjections();

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
            if(CatalogueRepository is TableRepository)
            {
                var ex = Assert.Throws<SqlException>(() => new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1));
                Assert.IsTrue(ex.Message.Contains("ix_eachAggregateCanOnlyBeJoinableOnOneProject"));
            }
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
            joinable?.DeleteInDatabase();
        }
    }

    [Test]
    public void CreateUsers_DuplicateUser()
    {
        var joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate1);
        try
        {
            joinable.AddUser(aggregate2);
            var ex = Assert.Throws<Exception>(()=>joinable.AddUser(aggregate2));
            Assert.AreEqual($"AggregateConfiguration 'UnitTestAggregate2' already uses 'Patient Index Table:cic_{cohortIdentificationConfiguration.ID}_UnitTestAggregate1'. Only one patient index table join is permitted.", ex.Message);
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
        aggregate2.ClearAllInjections();

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
        //make aggregate 2 a joinable
        var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository,cohortIdentificationConfiguration, aggregate2);
        joinable2.AddUser(aggregate1);

        var builder = new CohortQueryBuilder(aggregate1, null,null);
        Console.WriteLine(builder.SQL);
        try
        {

            using (var con = (SqlConnection)Database.Server.GetConnection())
            {
                con.Open();

                using var dbReader = new SqlCommand(builder.SQL, con).ExecuteReader();

                //can read at least one row
                Assert.IsTrue(dbReader.Read());
            }

            var expectedTableAlias = $"ix{joinable2.ID}";

            //after joinables
            Assert.AreEqual(
                string.Format(
                    @"/*cic_{1}_UnitTestAggregate1*/
SELECT
distinct
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+ @"ScratchArea].[dbo].[BulkData]
LEFT Join (
	/*cic_{1}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
){0}
on ["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi] = {0}.chi",expectedTableAlias,cohortIdentificationConfiguration.ID), builder.SQL);

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

        var expectedTableAlias = $"ix{joinable2.ID}";

        var filterContainer1 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        var filterContainer2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);

        var filter1 = new AggregateFilter(CatalogueRepository, "Within 1 year of event", filterContainer1);
        var filter2 = new AggregateFilter(CatalogueRepository, "DateAfter2001", filterContainer2);

        filter1.WhereSQL = string.Format("ABS(DATEDIFF(year, {0}.dtCreated, ["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].dtCreated)) <= 1",expectedTableAlias);
        filter1.SaveToDatabase();

        filter2.WhereSQL = "dtCreated > '2001-01-01'";
        filter2.SaveToDatabase();

        aggregate1.RootFilterContainer_ID = filterContainer1.ID;
        aggregate1.SaveToDatabase();

        aggregate2.RootFilterContainer_ID = filterContainer2.ID;
        aggregate2.SaveToDatabase();

        var builder = new CohortQueryBuilder(aggregate1, null,null);


        Console.WriteLine(builder.SQL);

            
        try
        {
            using (var con = (SqlConnection)Database.Server.GetConnection())
            {
                con.Open();

                using var dbReader = new SqlCommand(builder.SQL, con).ExecuteReader();

                //can read at least one row
                Assert.IsTrue(dbReader.Read());
            }


            //after joinables
            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(
                        @"/*cic_{1}_UnitTestAggregate1*/
SELECT
distinct
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+ @"ScratchArea].[dbo].[BulkData]
LEFT Join (
	/*cic_{1}_UnitTestAggregate2*/
	SELECT distinct
	[" + TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi], ["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[dtCreated]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
	WHERE
	(
	/*DateAfter2001*/
	dtCreated > '2001-01-01'
	)
){0}
on ["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi] = {0}.chi

WHERE
(
/*Within 1 year of event*/
ABS(DATEDIFF(year, {0}.dtCreated, ["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].dtCreated)) <= 1
)", expectedTableAlias,cohortIdentificationConfiguration.ID)), CollapseWhitespace(builder.SQL));

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

        var expectedTableAlias = $"ix{joinable2.ID}";

        var filterContainer1 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        var filterContainer2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);

        var filter1 = new AggregateFilter(CatalogueRepository, "Within 1 year of event", filterContainer1);
        var filter2 = new AggregateFilter(CatalogueRepository, "DateAfter2001", filterContainer2);

        filter1.WhereSQL = string.Format("ABS(DATEDIFF(year, {0}.dtCreated, ["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].dtCreated)) <= 1", expectedTableAlias);
        filter1.SaveToDatabase();

        filter2.WhereSQL = "dtCreated > '2001-01-01'";
        filter2.SaveToDatabase();

        aggregate1.RootFilterContainer_ID = filterContainer1.ID;
        aggregate1.SaveToDatabase();

        aggregate2.RootFilterContainer_ID = filterContainer2.ID;
        aggregate2.SaveToDatabase();

        //add the first aggregate to the configuration
        rootcontainer.AddChild(aggregate1,1);

        var globalParameter = new AnyTableSqlParameter(CatalogueRepository, cohortIdentificationConfiguration,"DECLARE @fish varchar(50)")
            {
                Comment = "Comments for the crazies",
                Value = "'fishes'"
            };
        globalParameter.SaveToDatabase();

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);

        try
        {
            var clone = cohortIdentificationConfiguration.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);

            var cloneBuilder = new CohortQueryBuilder(clone,null);

            var origSql = builder.SQL;
            var cloneOrigSql = cloneBuilder.SQL;

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
                .Select(ag => ag.RootFilterContainer);//grab their containers so we can clean them SetUp

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
        var queryCachingDatabaseName = To.GetRuntimeName();
        _queryCachingDatabase = To;

        //make aggregate 2 a joinable
        var joinable2 = new JoinableCohortAggregateConfiguration(CatalogueRepository, cohortIdentificationConfiguration, aggregate2);
        joinable2.AddUser(aggregate1);

        //make aggregate 2 have an additional column (dtCreated)
        var anotherCol = aggregate2.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("dtCreated"));
        aggregate2.AddDimension(anotherCol);

        //create a caching server
        var scripter = new MasterDatabaseScriptExecutor(_queryCachingDatabase);
        scripter.CreateAndPatchDatabase(new QueryCachingPatcher(), new AcceptAllCheckNotifier());

        var queryCachingDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, queryCachingDatabaseName,null);
        queryCachingDatabaseServer.SetProperties(_queryCachingDatabase);
            
        try
        {

            var builderForCaching = new CohortQueryBuilder(aggregate2, null,null);

            var cacheDt = new DataTable();
            using (var con = (SqlConnection)Database.Server.GetConnection())
            {
                con.Open();
                var da = new SqlDataAdapter(new SqlCommand(builderForCaching.SQL, con));
                da.Fill(cacheDt);
            }

            var cacheManager = new CachedAggregateConfigurationResultsManager(queryCachingDatabaseServer);
            cacheManager.CommitResults(new CacheCommitJoinableInceptionQuery(aggregate2, builderForCaching.SQL, cacheDt, null,30));

            try
            {
                var builder = new CohortQueryBuilder(aggregate1, null,null)
                {
                    //make the builder use the query cache we just set SetUp
                    CacheServer = queryCachingDatabaseServer
                };

                Console.WriteLine(builder.SQL);

                using (var con = (SqlConnection)Database.Server.GetConnection())
                {
                    con.Open();

                    using var dbReader = new SqlCommand(builder.SQL, con).ExecuteReader();

                    //can read at least one row
                    Assert.IsTrue(dbReader.Read());
                }

                var expectedTableAlias = $"ix{joinable2.ID}";

                //after joinables
                Assert.AreEqual(
                    CollapseWhitespace(
                        string.Format(
                            @"/*cic_{2}_UnitTestAggregate1*/
SELECT
distinct
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
LEFT Join (
	/*Cached:cic_{2}_UnitTestAggregate2*/
	select * from [{3}]..[JoinableInceptionQuery_AggregateConfiguration{1}]

){0}
on [" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData].[chi] = {0}.chi",
                            expectedTableAlias,  //{0}
                            aggregate2.ID, //{1}
                            cohortIdentificationConfiguration.ID,//{2}
                            queryCachingDatabaseName) //{3}
                    ), CollapseWhitespace(builder.SQL));

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
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(queryCachingDatabaseName).Drop();

        }
            
            

           
    }
}