// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.QueryBuildingTests.QueryBuilderTests;

public class MySqlQueryBuilderTests : DatabaseTests
{
    [Test]
    public void TestQueryBuilder_MySql_Normal()
    {
        var t = new TableInfo(CatalogueRepository, "`db`.`tbl`");
        t.DatabaseType = DatabaseType.MySql;
        t.SaveToDatabase();

        var col = new ColumnInfo(CatalogueRepository, "`db`.`tbl`.`col`","varchar(10)",t);
        Assert.AreEqual("col",col.GetRuntimeName());

        var cata = new Catalogue(CatalogueRepository,"cata");
        var catalogueItem = new CatalogueItem(CatalogueRepository, cata, "col");
        var extractionInfo = new ExtractionInformation(CatalogueRepository, catalogueItem, col, col.Name);
            
        var qb = new QueryBuilder(null, null);
        qb.AddColumn(extractionInfo);
        Assert.AreEqual(CollapseWhitespace(
            @"SELECT 
`db`.`tbl`.`col`
FROM 
`db`.`tbl`"
        ),CollapseWhitespace(qb.SQL));

    }
    [Test]
    public void TestQueryBuilder_MySql_Top35()
    {
        var t = new TableInfo(CatalogueRepository, "`db`.`tbl`");
        t.DatabaseType = DatabaseType.MySql;
        t.SaveToDatabase();

        var col = new ColumnInfo(CatalogueRepository, "`db`.`tbl`.`col`", "varchar(10)", t);
        Assert.AreEqual("col", col.GetRuntimeName());

        var cata = new Catalogue(CatalogueRepository, "cata");
        var catalogueItem = new CatalogueItem(CatalogueRepository, cata, "col");
        var extractionInfo = new ExtractionInformation(CatalogueRepository, catalogueItem, col, col.Name);

        var qb = new QueryBuilder(null, null);
        qb.TopX = 35;
        qb.AddColumn(extractionInfo);
        Assert.AreEqual(
            CollapseWhitespace(
                @"SELECT 
`db`.`tbl`.`col`
FROM 
`db`.`tbl`
LIMIT 35")
            , CollapseWhitespace(qb.SQL));


        //editting the topX should invalidate the SQL automatically
        qb.TopX = 50;
        Assert.AreEqual(
            CollapseWhitespace(
                @"SELECT 
`db`.`tbl`.`col`
FROM 
`db`.`tbl`
LIMIT 50")
            , CollapseWhitespace(qb.SQL));

    }
}