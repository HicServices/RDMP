// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class TableInfoJoiningQueryBuilderTests : DatabaseTests
{
    [Test]
    public void OpportunisticJoinRequired()
    {
        var memory = new MemoryRepository();

        //tables and columns
        var head = new TableInfo(CatalogueRepository, "Head");
        var col1 = new ColumnInfo(CatalogueRepository, "TestResultSetNumber", "int", head);
        var col2 = new ColumnInfo(CatalogueRepository, "PK", "int", head);

        var result = new TableInfo(CatalogueRepository, "[biochemistry]..[Result]");
        var col3 = new ColumnInfo(CatalogueRepository, "FK", "int", result);
        var col4 = new ColumnInfo(CatalogueRepository, "Code", "varchar(10)", result);
        var col5 = new ColumnInfo(CatalogueRepository, "[biochemistry]..[Result].[OmgBob]", "varchar(10)", result);

        //we can join on col2 = col3
        new JoinInfo(CatalogueRepository, col3, col2, ExtractionJoinType.Right, "");

        //CASE 1 : Only 1 column used so no join needed
        var queryBuilder = new QueryBuilder(null, null);
        var icol1 = new ColumnInfoToIColumn(memory, col1)
        {
            Order = 1
        };
        queryBuilder.AddColumn(icol1);

        var tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out _, null);

        Assert.That(tablesUsed, Has.Count.EqualTo(1));
        Assert.That(tablesUsed[0], Is.EqualTo(head));

        //CASE 2 : 2 columns used one from each table so join is needed
        queryBuilder = new QueryBuilder(null, null);
        queryBuilder.AddColumn(new ColumnInfoToIColumn(memory, col1));

        var icol4 = new ColumnInfoToIColumn(memory, col4)
        {
            Order = 2
        };
        queryBuilder.AddColumn(icol4);

        tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out _, null);

        Assert.That(tablesUsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(tablesUsed[0], Is.EqualTo(head));
            Assert.That(tablesUsed[1], Is.EqualTo(result));

            Assert.That(CollapseWhitespace(queryBuilder.SQL), Is.EqualTo(CollapseWhitespace(@"SELECT 
TestResultSetNumber,
Code
FROM 
[biochemistry]..[Result] Right JOIN Head ON FK = PK")));
        });

        var memoryRepository = new MemoryCatalogueRepository();

        var spontContainer =
            new SpontaneouslyInventedFilterContainer(memoryRepository, null, null, FilterContainerOperation.AND);

        var spontFilter = new SpontaneouslyInventedFilter(memoryRepository, spontContainer,
            "[biochemistry]..[Result].[OmgBob] = 'T'",
            "My Filter", "Causes spontaneous requirement for joining completely", null);
        spontContainer.AddChild(spontFilter);


        //CASE 3 : Only 1 column from Head but filter contains a reference to Result column
        queryBuilder = new QueryBuilder(null, null);
        queryBuilder.AddColumn(new ColumnInfoToIColumn(memory, col1));

        //without the filter
        tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out _, null);
        Assert.That(tablesUsed, Has.Count.EqualTo(1));

        //set the filter
        queryBuilder.RootFilterContainer = spontContainer;

        //this is super sneaky but makes the queryBuilder populate its Filters property... basically your not supposed to use SqlQueryBuilderHelper for this kind of thing
        Console.WriteLine(queryBuilder.SQL);
        queryBuilder.ParameterManager.ClearNonGlobals();

        //with the filter
        tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out _, null);
        Assert.That(tablesUsed, Has.Count.EqualTo(2));
    }
}