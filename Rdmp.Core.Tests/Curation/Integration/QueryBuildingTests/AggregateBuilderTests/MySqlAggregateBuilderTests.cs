// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Tests.Curation.Integration.QueryBuildingTests.AggregateBuilderTests;

public class MySqlAggregateBuilderTests : AggregateBuilderTestsBase
{
    [Test]
    public void Test_AggregateBuilder_MySql_Top32()
    {
        _ti.DatabaseType = DatabaseType.MySql;
        _ti.SaveToDatabase();

        var builder = new AggregateBuilder(null, "count(*)", null);
        builder.AddColumn(_dimension1);

        var topx = new AggregateTopX(CatalogueRepository, _configuration, 32);
        topx.OrderByDimensionIfAny_ID = _dimension1.ID;
        topx.SaveToDatabase();

        builder.AggregateTopX = topx;
            
            
        Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
Col1 desc
LIMIT 32"),CollapseWhitespace(builder.SQL.Trim()));


        topx.DeleteInDatabase();
    }
    [TestCase(true)]
    [TestCase(false)]
    public void Test_AggregateBuilder_MySql_Top31OrderByCountAsc(bool useAliasForGroupBy)
    {
        _ti.DatabaseType = DatabaseType.MySql;
        _ti.SaveToDatabase();

        UserSettings.UseAliasInsteadOfTransformInGroupByAggregateGraphs = useAliasForGroupBy;

        var builder = new AggregateBuilder(null, "count(*)", null);
        builder.AddColumn(_dimension1);

        var topx = new AggregateTopX(CatalogueRepository, _configuration, 31);
        topx.OrderByDirection = AggregateTopXOrderByDirection.Ascending;
        builder.AggregateTopX = topx;

        if (useAliasForGroupBy)
        {
            Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
MyCount asc
LIMIT 31"), CollapseWhitespace(builder.SQL));
        }
        else
        {
            Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
count(*) asc
LIMIT 31"), CollapseWhitespace(builder.SQL));
        }
            


        topx.DeleteInDatabase();

        UserSettings.UseAliasInsteadOfTransformInGroupByAggregateGraphs = false;
    }
}