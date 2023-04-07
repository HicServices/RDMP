// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using System;

namespace Rdmp.Core.Tests.Curation.Integration.QueryBuildingTests.AggregateBuilderTests;

public class MicrosoftAggregateBuilderTests:AggregateBuilderTestsBase
{
       
        
    [Test]
    public void TestAggregateBuilding_NoConfigurationOneDimension()
    {
        var builder = new AggregateBuilder(null, "count(*)", null);
        builder.AddColumn(_dimension1);

        Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
Col1"),CollapseWhitespace(builder.SQL));
    }
        
    /// <summary>
    /// Tests the systems ability to figure out the alias of the count column when it has " AS " (e.g. in a cast scalar function)
    /// </summary>
    [Test]
    public void TestAggregateBuilding_AS_InCount()
    {
        var builder = new AggregateBuilder(null, "count(cast(1 AS int))", null);
        builder.AddColumn(_dimension1);

        Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(cast(1 AS int)) AS MyCount
FROM 
T1
group by 
Col1
order by 
Col1"),CollapseWhitespace(builder.SQL));
    }

    [Test]
    public void TestAggregateBuilding_NoConfigurationTwoDimension()
    {
        var builder = new AggregateBuilder(null, "count(*)", null);
        builder.AddColumn(_dimension1);
        builder.AddColumn(_dimension2);

        Assert.AreEqual(CollapseWhitespace(CollapseWhitespace(@"/**/
SELECT
Col1,
Col2,
count(*) AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
Col1,
Col2")),CollapseWhitespace(builder.SQL));
    }

    [Test]
    public void TestAggregateBuilding_ConfigurationTwoDimension()
    {
        var builder = new AggregateBuilder(null, "count(*)", _configuration);
        builder.AddColumn(_dimension1);
        builder.AddColumn(_dimension2);

        Assert.AreEqual(CollapseWhitespace(@"/*MyConfig*/
SELECT 
Col1,
Col2,
count(*) AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
Col1,
Col2"), CollapseWhitespace(builder.SQL));
    }

    [Test]
    public void TwoTopXObjects()
    {
        var topX1 = new AggregateTopX(CatalogueRepository, _configuration, 10);
        var ex = Assert.Throws<Exception>(() => new AggregateTopX(CatalogueRepository, _configuration, 10));

        Assert.AreEqual("AggregateConfiguration MyConfig already has a TopX",ex.Message);
        topX1.DeleteInDatabase();
    }

    [TestCase("count(*)",true)]
    [TestCase("count(*)", false)]
    [TestCase("max(Col1)",true)]
    [TestCase("max(Col2)", false)]
    public void TestAggregateBuilding_NoConfigurationTwoDimension_Top10(string countColField,bool asc)
    {
        var topX = new AggregateTopX(CatalogueRepository, _configuration, 10);
        topX.OrderByDirection = asc
            ? AggregateTopXOrderByDirection.Ascending
            : AggregateTopXOrderByDirection.Descending;
        topX.SaveToDatabase();

        var beforeCountSQL = _configuration.CountSQL;
        _configuration.CountSQL = countColField;

        var builder = _configuration.GetQueryBuilder();
            
        Assert.AreEqual(CollapseWhitespace(@"/*MyConfig*/
SELECT 
TOP 10
Col1,
Col2,
"+countColField+@" AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
"+countColField+" " + (asc?"asc":"desc")),CollapseWhitespace(builder.SQL));

        _configuration.CountSQL = beforeCountSQL;
        topX.DeleteInDatabase();
    }


    [TestCase(true)]
    [TestCase(false)]
    public void TestAggregateBuilding_NoConfigurationTwoDimension_Top10DimensionOrder(bool asc)
    {
        var topX = new AggregateTopX(CatalogueRepository, _configuration, 10);
        topX.OrderByDimensionIfAny_ID = _dimension1.ID;
        topX.OrderByDirection = asc
            ? AggregateTopXOrderByDirection.Ascending
            : AggregateTopXOrderByDirection.Descending;
        topX.SaveToDatabase();
            
        var builder = _configuration.GetQueryBuilder();

        Assert.AreEqual(CollapseWhitespace(@"/*MyConfig*/
SELECT 
TOP 10
Col1,
Col2,
count(*) AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
Col1 " + (asc ? "asc" : "desc")), CollapseWhitespace(builder.SQL));
            
        topX.DeleteInDatabase();
    }

    [Test]
    public void TestAggregateBuilding_NoConfigurationNoDimensions()
    {
        var builder = new AggregateBuilder(null, "count(*)", null,new []{_ti});
            
        Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
count(*) AS MyCount
FROM 
T1"), CollapseWhitespace(builder.SQL));
    }

}