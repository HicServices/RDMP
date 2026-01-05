// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.TableValuedFunctionTests;

public class AggregationTests : DatabaseTests
{
    private TestableTableValuedFunction _function;

    private void CreateFunction(ICatalogueRepository repo)
    {
        _function = new TestableTableValuedFunction();
        _function.Create(GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer), repo);
    }

    [Test]
    public void GenerateAggregateManuallyTest()
    {
        CreateFunction(CatalogueRepository);

        //do a count * on the query builder
        var queryBuilder = new AggregateBuilder("", "count(*)", null, new[] { _function.TableInfoCreated });

        Assert.That(queryBuilder.SQL, Does.Contain(@"SELECT"));
        Assert.That(queryBuilder.SQL, Does.Contain(@"count(*)"));

        Assert.That(queryBuilder.SQL, Does.Contain(@"DECLARE @name AS varchar(50);"));
        Assert.That(queryBuilder.SQL, Does.Contain(@"SET @name='fish';"));

        Assert.That(
            queryBuilder.SQL, Does.Contain("..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction"));

        Console.WriteLine(queryBuilder.SQL);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GenerateAggregateViaAggregateConfigurationTest(bool memoryRepo)
    {
        var repo = memoryRepo ? (ICatalogueRepository)new MemoryCatalogueRepository() : CatalogueRepository;
        CreateFunction(repo);

        var agg = new AggregateConfiguration(repo, _function.Cata, "MyExcitingAggregate");

        try
        {
            agg.HavingSQL = "count(*)>1";
            agg.SaveToDatabase();

            var aggregateForcedJoin = repo.AggregateForcedJoinManager;
            aggregateForcedJoin.CreateLinkBetween(agg, _function.TableInfoCreated);

            var queryBuilder = agg.GetQueryBuilder();

            Assert.That(
queryBuilder.SQL, Is.EqualTo($@"DECLARE @startNumber AS int;
SET @startNumber=5;
DECLARE @stopNumber AS int;
SET @stopNumber=10;
DECLARE @name AS varchar(50);
SET @name='fish';
/*MyExcitingAggregate*/
SELECT
count(*) AS MyCount
FROM 
[{TestDatabaseNames.Prefix}ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction
HAVING
count(*)>1"));
        }
        finally
        {
            agg.DeleteInDatabase();
        }
    }

    [Test]
    public void GenerateAggregateUsingOverridenParametersTest()
    {
        CreateFunction(CatalogueRepository);

        var agg = new AggregateConfiguration(CatalogueRepository, _function.Cata, "MyExcitingAggregate");

        try
        {
            var param = new AnyTableSqlParameter(CatalogueRepository, agg, "DECLARE @name AS varchar(50);")
            {
                Value = "'lobster'"
            };
            param.SaveToDatabase();

            var aggregateForcedJoin = new AggregateForcedJoin(CatalogueTableRepository);
            aggregateForcedJoin.CreateLinkBetween(agg, _function.TableInfoCreated);

            //do a count * on the query builder
            var queryBuilder = agg.GetQueryBuilder();

            Assert.That(queryBuilder.SQL, Does.Contain(@"SELECT"));
            Assert.That(queryBuilder.SQL, Does.Contain(@"count(*)"));

            //should have this version of things
            Assert.That(queryBuilder.SQL, Does.Contain(@"DECLARE @name AS varchar(50);"));
            Assert.That(queryBuilder.SQL, Does.Contain(@"SET @name='lobster';"));

            //instead of this version of things
            Assert.That(queryBuilder.SQL, Does.Not.Contain(@"SET @name='fish';"));

            Assert.That(
                queryBuilder.SQL, Does.Contain("..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction"));

            Console.WriteLine(queryBuilder.SQL);
        }
        finally
        {
            agg.DeleteInDatabase();
        }
    }
}