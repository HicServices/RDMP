// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Data;
using FAnsi;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.AggregateBuilderTests
{
    public class MySqlAggregateBuilderTests : AggregateBuilderTestsBase
    {
        [Test]
        public void Test_AggregateBuilder_MySql_Top32()
        {
            _ti.DatabaseType = DatabaseType.MySql;
            _ti.SaveToDatabase();

            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null);
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
        [Test]
        public void Test_AggregateBuilder_MySql_Top31OrderByCountAsc()
        {
            _ti.DatabaseType = DatabaseType.MySql;
            _ti.SaveToDatabase();

            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);

            var topx = new AggregateTopX(CatalogueRepository, _configuration, 31);
            topx.OrderByDirection = AggregateTopXOrderByDirection.Ascending;
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
count(*) asc
LIMIT 31"), CollapseWhitespace(builder.SQL));


            topx.DeleteInDatabase();
        }
    }
}
