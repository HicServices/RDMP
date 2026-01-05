// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.QueryBuilding;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation.QueryTests;

public class CohortQueryBuilderTestsInvolvingTableValuedParameters : DatabaseTests
{
    private readonly TestableTableValuedFunction _function = new();

    private void CreateFunction()
    {
        _function.Create(GetCleanedServer(DatabaseType.MicrosoftSQLServer), CatalogueRepository);
    }

    [Test]
    public void CohortGenerationDifferingTableValuedParametersTest()
    {
        CreateFunction();

        //In this example we have 2 configurations which both target the same table valued function but which must have different parameter values
        var config1 = new AggregateConfiguration(CatalogueRepository, _function.Cata,
            "CohortGenerationDifferingTableValuedParametersTest_1")
        {
            CountSQL = null
        };
        config1.SaveToDatabase();

        var config2 = new AggregateConfiguration(CatalogueRepository, _function.Cata,
            "CohortGenerationDifferingTableValuedParametersTest_2")
        {
            CountSQL = null
        };
        config2.SaveToDatabase();

        var cic = new CohortIdentificationConfiguration(CatalogueRepository,
            "CohortGenerationDifferingTableValuedParametersTest");

        cic.EnsureNamingConvention(config1);
        cic.EnsureNamingConvention(config2);

        try
        {
            //make the string column the extraction identifier
            _function.ExtractionInformations[1].IsExtractionIdentifier = true;
            _function.ExtractionInformations[1].SaveToDatabase();

            //add the extraction identifier as the only dimension one ach of the aggregate configurations that we will use for the cohort identification query
            new AggregateDimension(CatalogueRepository, _function.ExtractionInformations[1], config1);
            new AggregateDimension(CatalogueRepository, _function.ExtractionInformations[1], config2);

            Assert.That(cic.RootCohortAggregateContainer_ID, Is.Null);

            //create a root container for it
            var container = new CohortAggregateContainer(CatalogueRepository, SetOperation.INTERSECT);

            //set the container as the root container for the cohort identification task object
            cic.RootCohortAggregateContainer_ID = container.ID;
            cic.SaveToDatabase();

            //put both the aggregates into the container
            container.AddChild(config1, 0);
            container.AddChild(config2, 1);

            var builder = new CohortQueryBuilder(cic, null);
            Assert.That(
                CollapseWhitespace(builder.SQL), Is.EqualTo(CollapseWhitespace(
                    string.Format(
                        @"DECLARE @startNumber AS int;
SET @startNumber=5;
DECLARE @stopNumber AS int;
SET @stopNumber=10;
DECLARE @name AS varchar(50);
SET @name='fish';

(
	/*cic_{0}_CohortGenerationDifferingTableValuedParametersTest_1*/
	SELECT
	distinct
	MyAwesomeFunction.[Name]
	FROM 
	[" + TestDatabaseNames.Prefix +
                        @"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction

	INTERSECT

	/*cic_{0}_CohortGenerationDifferingTableValuedParametersTest_2*/
	SELECT
	distinct
	MyAwesomeFunction.[Name]
	FROM 
	[" + TestDatabaseNames.Prefix +
                        @"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction
)
", cic.ID))));

            //now override JUST @name
            var param1 = new AnyTableSqlParameter(CatalogueRepository, config1, "DECLARE @name AS varchar(50);")
            {
                Value = "'lobster'"
            };
            param1.SaveToDatabase();

            var param2 = new AnyTableSqlParameter(CatalogueRepository, config2, "DECLARE @name AS varchar(50);")
            {
                Value = "'monkey'"
            };
            param2.SaveToDatabase();

            var builder2 = new CohortQueryBuilder(cic, null);

            Assert.That(
                CollapseWhitespace(builder2.SQL), Is.EqualTo(CollapseWhitespace(
                    string.Format(
                        @"DECLARE @startNumber AS int;
SET @startNumber=5;
DECLARE @stopNumber AS int;
SET @stopNumber=10;
DECLARE @name AS varchar(50);
SET @name='lobster';
DECLARE @name_2 AS varchar(50);
SET @name_2='monkey';

(
	/*cic_{0}_CohortGenerationDifferingTableValuedParametersTest_1*/
	SELECT
	distinct
	MyAwesomeFunction.[Name]
	FROM 
	[" + TestDatabaseNames.Prefix +
                        @"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction

	INTERSECT

	/*cic_{0}_CohortGenerationDifferingTableValuedParametersTest_2*/
	SELECT
	distinct
	MyAwesomeFunction.[Name]
	FROM 
	[" + TestDatabaseNames.Prefix +
                        @"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name_2) AS MyAwesomeFunction
)
", cic.ID))));
        }
        finally
        {
            cic.DeleteInDatabase();
            config1.DeleteInDatabase();
            config2.DeleteInDatabase();
        }
    }
}