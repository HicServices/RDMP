// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation.QueryTests;

public class CohortQueryBuilderTests : CohortIdentificationTests
{
    private readonly string _scratchDatabaseName = TestDatabaseNames.GetConsistentName("ScratchArea");
        
    [Test]
    public void TestGettingAggregateJustFromConfig_DistinctCHISelect()
    {
        var builder = new CohortQueryBuilder(aggregate1,null,null);

        Assert.AreEqual(CollapseWhitespace(string.Format(@"/*cic_{0}_UnitTestAggregate1*/
SELECT 
distinct
[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
FROM 
[" + _scratchDatabaseName + @"].[dbo].[BulkData]", cohortIdentificationConfiguration.ID)), CollapseWhitespace(builder.SQL));
    }
        
    [Test]
    public void TestGettingAggregateJustFromConfig_SelectStar()
    {
        var builder = new CohortQueryBuilder(aggregate1, null,null);

        Assert.AreEqual(CollapseWhitespace(
            string.Format(@"/*cic_{0}_UnitTestAggregate1*/
	SELECT
	TOP 1000
	*
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]",cohortIdentificationConfiguration.ID)), CollapseWhitespace(builder.GetDatasetSampleSQL()));
    }


    /// <summary>
    /// When we <see cref="CohortQueryBuilder.GetDatasetSampleSQL"/> we normally get "select TOP 1000 *" of the query body BUT
    /// if there's HAVING sql then SQL will balk at select *.  In this case we expect it to just run the normal distinct chi
    /// bit but put a TOP X on it.
    /// </summary>
    [Test]
    public void Test_GetDatasetSampleSQL_WithHAVING()
    {
        aggregate1.HavingSQL = "count(*)>1";

        var builder = new CohortQueryBuilder(aggregate1, null,null);

        Assert.AreEqual(CollapseWhitespace(
            string.Format(@"/*cic_{0}_UnitTestAggregate1*/
	SELECT
    distinct
	TOP 1000
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
    group by 
    [" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
    HAVING
    count(*)>1", cohortIdentificationConfiguration.ID)), CollapseWhitespace(builder.GetDatasetSampleSQL()));
    }


    [Test]
    public void TestGettingAggregateSQLFromEntirity()
    {
        Assert.AreEqual(null, aggregate1.GetCohortAggregateContainerIfAny());

        //set the order so that 2 comes before 1
        rootcontainer.AddChild(aggregate2, 1);
        rootcontainer.AddChild(aggregate1, 5);

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);

        Assert.AreEqual(rootcontainer,aggregate1.GetCohortAggregateContainerIfAny());
        try
        {
            Assert.AreEqual(

                CollapseWhitespace(string.Format(
                    @"(
	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
)"
       
                    ,cohortIdentificationConfiguration.ID))
                , CollapseWhitespace(builder.SQL));
        }
        finally 
        {
            rootcontainer.RemoveChild(aggregate1);
            rootcontainer.RemoveChild(aggregate2);
        }
    }

    [Test]
    public void TestOrdering_AggregateThenContainer()
    {
        //set the order so that a configuration is in position 1 
        rootcontainer.AddChild(aggregate1, 1);

        //then a container in position 2
        container1.Order = 2;
        container1.SaveToDatabase();
        rootcontainer.AddChild(container1);

        //container 1 contains both other aggregates
        container1.AddChild(aggregate2, 1);
        container1.AddChild(aggregate3, 2);

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);

        try
        {
            var allConfigurations = rootcontainer.GetAllAggregateConfigurationsRecursively();
            Assert.IsTrue(allConfigurations.Contains(aggregate1));
            Assert.IsTrue(allConfigurations.Contains(aggregate2));
            Assert.IsTrue(allConfigurations.Contains(aggregate3));

            Assert.AreEqual(
                CollapseWhitespace(string.Format(
                    @"(
	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT


	(
		/*cic_{0}_UnitTestAggregate2*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]

		UNION

		/*cic_{0}_UnitTestAggregate3*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	)

)",cohortIdentificationConfiguration.ID))
                ,
                CollapseWhitespace(builder.SQL));
        }
        finally
        {
            container1.RemoveChild(aggregate2);
            container1.RemoveChild(aggregate3);
            rootcontainer.RemoveChild(aggregate1);
        }
    }

    [Test]
    public void TestOrdering_ContainerThenAggregate()
    {
        //set the order so that a configuration is in position 1 
        rootcontainer.AddChild(aggregate1, 2);

        //then a container in position 2
        container1.Order = 1;
        container1.SaveToDatabase();
        rootcontainer.AddChild(container1);

        //container 1 contains both other aggregates
        container1.AddChild(aggregate2, 1);
        container1.AddChild(aggregate3, 2);

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);

        try
        {
            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(
                        @"(

	(
		/*cic_{0}_UnitTestAggregate2*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]

		UNION

		/*cic_{0}_UnitTestAggregate3*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	)


	EXCEPT

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
)",cohortIdentificationConfiguration.ID))
                , CollapseWhitespace(builder.SQL));
        }
        finally
        {
            container1.RemoveChild(aggregate2);
            container1.RemoveChild(aggregate3);
            rootcontainer.RemoveChild(aggregate1);
        }
    }

    [Test]
    public void TestGettingAggregateSQLFromEntirity_IncludingParametersAtTop()
    {
        //setup a filter (all filters must be in a container so the container is a default AND container)
        var AND = new AggregateFilterContainer(CatalogueRepository,FilterContainerOperation.AND);
        var filter = new AggregateFilter(CatalogueRepository,"hithere",AND);

        //give the filter an implicit parameter requiring bit of SQL
        filter.WhereSQL = "1=@abracadabra";
        filter.SaveToDatabase();

        //Make aggregate1 use the filter we just setup (required to happen before parameter creator gets hit because otherwise it won't know the IFilter DatabaseType because IFilter is an orphan at the moment)
        aggregate1.RootFilterContainer_ID = AND.ID;
        aggregate1.SaveToDatabase();

        //get it to create the parameters for us
        new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(filter, null);

        //get the parameter it just created, set its value and save it
        var param = (AggregateFilterParameter) filter.GetAllParameters().Single();
        param.Value = "1";
        param.ParameterSQL = "DECLARE @abracadabra AS int;";
        param.SaveToDatabase();

            
            
            
        //set the order so that 2 comes before 1
        rootcontainer.AddChild(aggregate2, 1);
        rootcontainer.AddChild(aggregate1, 5);

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);

        try
        {
            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(
                        @"DECLARE @abracadabra AS int;
SET @abracadabra=1;

(
	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	WHERE
	(
	/*hithere*/
	1=@abracadabra
	)
)
",cohortIdentificationConfiguration.ID))
                , CollapseWhitespace(builder.SQL));


            var builder2 = new CohortQueryBuilder(aggregate1, null,null);
            Assert.AreEqual(

                CollapseWhitespace(
                    string.Format(
                        @"DECLARE @abracadabra AS int;
SET @abracadabra=1;
/*cic_{0}_UnitTestAggregate1*/
SELECT
distinct
[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
FROM 
[" + _scratchDatabaseName + @"].[dbo].[BulkData]
WHERE
(
/*hithere*/
1=@abracadabra
)",cohortIdentificationConfiguration.ID)),
                CollapseWhitespace(builder2.SQL));


            var selectStar = new CohortQueryBuilder(aggregate1,null,null).GetDatasetSampleSQL();

            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(

                        @"DECLARE @abracadabra AS int;
SET @abracadabra=1;

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	TOP 1000
	*
	FROM 
	[" + TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
	WHERE
	(
	/*hithere*/
	1=@abracadabra
	)",cohortIdentificationConfiguration.ID)),
                CollapseWhitespace(selectStar));

        }
        finally
        {
            filter.DeleteInDatabase();
            AND.DeleteInDatabase();

            rootcontainer.RemoveChild(aggregate1);
            rootcontainer.RemoveChild(aggregate2);

        }
    }


    [Test]
    public void TestGettingAggregateSQLFromEntirity_StopEarly()
    {
        rootcontainer.AddChild(aggregate1,1);
        rootcontainer.AddChild(aggregate2,2);
        rootcontainer.AddChild(aggregate3,3);

        var builder = new CohortQueryBuilder(rootcontainer, null,null);
            
        builder.StopContainerWhenYouReach = aggregate2;
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
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT

	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
)
",cohortIdentificationConfiguration.ID)),
                CollapseWhitespace(builder.SQL));


            var builder2 = new CohortQueryBuilder(rootcontainer, null,null);
            builder2.StopContainerWhenYouReach = null;
            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(
                        @"
(
	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT

	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT

	/*cic_{0}_UnitTestAggregate3*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
)
",cohortIdentificationConfiguration.ID)),
                CollapseWhitespace(builder2.SQL));
        }
        finally
        {
            rootcontainer.RemoveChild(aggregate1);
            rootcontainer.RemoveChild(aggregate2);
            rootcontainer.RemoveChild(aggregate3);

        }
    }

    [Test]
    public void TestGettingAggregateSQLFromEntirity_StopEarlyContainer()
    {
        rootcontainer.AddChild(aggregate1, -5);

        container1.AddChild(aggregate2, 2);
        container1.AddChild(aggregate3, 3);
            
        rootcontainer.AddChild(container1);

        var aggregate4 = new AggregateConfiguration(CatalogueRepository, testData.catalogue, "UnitTestAggregate4");
        new AggregateDimension(CatalogueRepository, testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate4);

        rootcontainer.AddChild(aggregate4,5);
        var builder = new CohortQueryBuilder(rootcontainer, null,null);

        //Looks like:
        /*
         * 
        EXCEPT
        Aggregate 1
          UNION            <-----We tell it to stop after this container
           Aggregate2
           Aggregate3
        Aggregate 4
        */
        builder.StopContainerWhenYouReach = container1;
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
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]

	EXCEPT


	(
		/*cic_{0}_UnitTestAggregate2*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]

		UNION

		/*cic_{0}_UnitTestAggregate3*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	)

)
",cohortIdentificationConfiguration.ID)),
                CollapseWhitespace(builder.SQL));
        }
        finally
        {
            rootcontainer.RemoveChild(aggregate1);
            rootcontainer.RemoveChild(aggregate4);
            container1.RemoveChild(aggregate2);
            container1.RemoveChild(aggregate3);

            aggregate4.DeleteInDatabase();
        }
    }

    [Test]
    public void TestHavingSQL()
    {
        rootcontainer.AddChild(aggregate1, -5);

        container1.AddChild(aggregate2, 2);
        container1.AddChild(aggregate3, 3);

        aggregate2.HavingSQL = "count(*)>1";
        aggregate2.SaveToDatabase();
        aggregate1.HavingSQL = "SUM(Result)>10";
        aggregate1.SaveToDatabase();
        try
        {

            rootcontainer.AddChild(container1);

            var builder = new CohortQueryBuilder(rootcontainer, null,null);
            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(
                        @"
(
	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	group by 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	HAVING
	SUM(Result)>10

	EXCEPT


	(
		/*cic_{0}_UnitTestAggregate2*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]
		group by
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		HAVING
		count(*)>1

		UNION

		/*cic_{0}_UnitTestAggregate3*/
		SELECT
		distinct
		[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
		FROM 
		[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	)

)
",cohortIdentificationConfiguration.ID))
                , CollapseWhitespace(builder.SQL));

        }
        finally
        {
            rootcontainer.RemoveChild(aggregate1);

            container1.RemoveChild(aggregate2);
            container1.RemoveChild(aggregate3);

            aggregate2.HavingSQL = null;
            aggregate2.SaveToDatabase();
            aggregate1.HavingSQL = null;
            aggregate1.SaveToDatabase();
        }
    }

    [Test]
    [TestCase(true,true)]
    [TestCase(true, false)]
    [TestCase(false,true)]
    [TestCase(false, false)]
    public void TestGettingAggregateSQLFromEntirity_TwoFilterParametersPerDataset(bool valuesAreSame,bool memoryRepository)
    {
        var repo = memoryRepository ? (ICatalogueRepository)new MemoryCatalogueRepository() : CatalogueRepository;

        //create all the setup again but in the memory repository
        SetupTestData(repo);

        //setup a filter (all filters must be in a container so the container is a default AND container)
        var AND1 = new AggregateFilterContainer(repo,FilterContainerOperation.AND);
        var filter1_1 = new AggregateFilter(repo,"filter1_1",AND1);
        var filter1_2 = new AggregateFilter(repo,"filter1_2",AND1);

        var AND2 = new AggregateFilterContainer(repo,FilterContainerOperation.AND);
        var filter2_1 = new AggregateFilter(repo,"filter2_1",AND2);
        var filter2_2 = new AggregateFilter(repo,"filter2_2",AND2);
             
        //Filters must belong to containers BEFORE parameter creation
        //Make aggregate1 use the filter set we just setup
        aggregate1.RootFilterContainer_ID = AND1.ID;
        aggregate1.SaveToDatabase();

        //Make aggregate3 use the other filter set we just setup
        aggregate2.RootFilterContainer_ID = AND2.ID;
        aggregate2.SaveToDatabase();

        //set the order so that 2 comes before 1
        rootcontainer.AddChild(aggregate2, 1);
        rootcontainer.AddChild(aggregate1, 5);

        //give the filter an implicit parameter requiring bit of SQL
        foreach (var filter in new IFilter[]{filter1_1,filter1_2,filter2_1,filter2_2})
        {
            filter.WhereSQL = "@bob = 'bob'";
            filter.SaveToDatabase();     
            //get it to create the parameters for us
            new ParameterCreator(new AggregateFilterFactory(repo), null, null).CreateAll(filter, null);

            //get the parameter it just created, set its value and save it
            var param = (AggregateFilterParameter) filter.GetAllParameters().Single();
            param.Value = "'Boom!'";
            param.ParameterSQL = "DECLARE @bob AS varchar(10);";
                
            //if test case is different values then we change the values of the parameters
            if (!valuesAreSame && (filter.Equals(filter2_1) || Equals(filter, filter2_2)))
                param.Value = "'Grenades Are Go'";

            param.SaveToDatabase();
        }

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);
        Console.WriteLine( builder.SQL);

        try
        {
            if (valuesAreSame)
            {
                Assert.AreEqual(
                    CollapseWhitespace(
                        string.Format(
                            @"DECLARE @bob AS varchar(10);
SET @bob='Boom!';

(
	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	WHERE
	(
	/*filter2_1*/
	@bob = 'bob'
	AND
	/*filter2_2*/
	@bob = 'bob'
	)

	EXCEPT

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
	WHERE
	(
	/*filter1_1*/
	@bob = 'bob'
	AND
	/*filter1_2*/
	@bob = 'bob'
	)
)
",cohortIdentificationConfiguration.ID)),
                    CollapseWhitespace(builder.SQL));
            }
            else
            {
                Assert.AreEqual(

                    CollapseWhitespace( 
                        string.Format(
                            @"DECLARE @bob AS varchar(10);
SET @bob='Grenades Are Go';
DECLARE @bob_2 AS varchar(10);
SET @bob_2='Boom!';

(
	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
	FROM 
	["+TestDatabaseNames.Prefix+ @"ScratchArea].[dbo].[BulkData]
	WHERE
	(
	/*filter2_1*/
	@bob = 'bob'
	AND
	/*filter2_2*/
	@bob = 'bob'
	)

	EXCEPT

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
	WHERE
	(
	/*filter1_1*/
	@bob_2 = 'bob'
	AND
	/*filter1_2*/
	@bob_2 = 'bob'
	)
)
",cohortIdentificationConfiguration.ID)),
                    CollapseWhitespace(builder.SQL));
            }
        }
        finally
        {
            rootcontainer.RemoveChild(aggregate2);
            rootcontainer.RemoveChild(aggregate1);

            filter1_1.DeleteInDatabase();
            filter1_2.DeleteInDatabase();
            filter2_1.DeleteInDatabase();
            filter2_2.DeleteInDatabase();
                 
            AND1.DeleteInDatabase();
            AND2.DeleteInDatabase();

        }
    }

    [Test]
    public void TestGettingAggregateSQL_Aggregate_IsDisabled()
    {
        Assert.AreEqual(null, aggregate1.GetCohortAggregateContainerIfAny());

        //set the order so that 2 comes before 1
        rootcontainer.AddChild(aggregate2, 1);
        rootcontainer.AddChild(aggregate1, 5);

        //disable aggregate 1
        aggregate1.IsDisabled = true;
        aggregate1.SaveToDatabase();

        Assert.AreEqual(rootcontainer, aggregate1.GetCohortAggregateContainerIfAny());

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);
        try
        {
            Assert.AreEqual(

                CollapseWhitespace(string.Format(
                    @"(
	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + _scratchDatabaseName + @"].[dbo].[BulkData].[chi]
	FROM 
	[" + _scratchDatabaseName + @"].[dbo].[BulkData]
)"

                    , cohortIdentificationConfiguration.ID))
                , CollapseWhitespace(builder.SQL));
        }
        finally
        {

            aggregate1.IsDisabled = false;
            aggregate1.SaveToDatabase();

            rootcontainer.RemoveChild(aggregate1);
            rootcontainer.RemoveChild(aggregate2);
        }
    }
        
    [Test]
    public void TestGettingAggregateSQLFromEntirity_Filter_IsDisabled()
    {
        //setup a filter (all filters must be in a container so the container is a default AND container)
        var AND1 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        var filter1_1 = new AggregateFilter(CatalogueRepository, "filter1_1", AND1);
        var filter1_2 = new AggregateFilter(CatalogueRepository, "filter1_2", AND1);

        var AND2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        var filter2_1 = new AggregateFilter(CatalogueRepository, "filter2_1", AND2);
        var filter2_2 = new AggregateFilter(CatalogueRepository, "filter2_2", AND2);

        //Filters must belong to containers BEFORE parameter creation
        //Make aggregate1 use the filter set we just setup
        aggregate1.RootFilterContainer_ID = AND1.ID;
        aggregate1.SaveToDatabase();

        //Make aggregate3 use the other filter set we just setup
        aggregate2.RootFilterContainer_ID = AND2.ID;
        aggregate2.SaveToDatabase();

        //set the order so that 2 comes before 1
        rootcontainer.AddChild(aggregate2, 1);
        rootcontainer.AddChild(aggregate1, 5);

        filter2_2.IsDisabled = true;
        filter2_2.SaveToDatabase();

        //give the filter an implicit parameter requiring bit of SQL
        foreach (var filter in new IFilter[] { filter1_1, filter1_2, filter2_1, filter2_2 })
        {
            filter.WhereSQL = "@bob = 'bob'";
            filter.SaveToDatabase();
            //get it to create the parameters for us
            new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(filter, null);

            //get the parameter it just created, set its value and save it
            var param = (AggregateFilterParameter)filter.GetAllParameters().Single();
            param.Value = "'Boom!'";
            param.ParameterSQL = "DECLARE @bob AS varchar(10);";

            //change the values of the parameters
            if (filter.Equals(filter2_1) || Equals(filter, filter2_2))
                param.Value = "'Grenades Are Go'";

            param.SaveToDatabase();
        }

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration,null);

        Console.WriteLine(builder.SQL);

        try
        {
            Assert.AreEqual(
                CollapseWhitespace(
                    string.Format(

                        @"DECLARE @bob AS varchar(10);
SET @bob='Grenades Are Go';
DECLARE @bob_2 AS varchar(10);
SET @bob_2='Boom!';

(
	/*cic_{0}_UnitTestAggregate2*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData].[chi]
	FROM 
	[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData]
	WHERE
	(
	/*filter2_1*/
	@bob = 'bob'
	)

	EXCEPT

	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData].[chi]
	FROM 
	[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData]
	WHERE
	(
	/*filter1_1*/
	@bob_2 = 'bob'
	AND
	/*filter1_2*/
	@bob_2 = 'bob'
	)
)
", cohortIdentificationConfiguration.ID)),
                CollapseWhitespace(builder.SQL));
                
        }
        finally
        {

            filter2_2.IsDisabled = false;
            filter2_2.SaveToDatabase();

            rootcontainer.RemoveChild(aggregate2);
            rootcontainer.RemoveChild(aggregate1);

            filter1_1.DeleteInDatabase();
            filter1_2.DeleteInDatabase();
            filter2_1.DeleteInDatabase();
            filter2_2.DeleteInDatabase();

            AND1.DeleteInDatabase();
            AND2.DeleteInDatabase();

        }
    }
}