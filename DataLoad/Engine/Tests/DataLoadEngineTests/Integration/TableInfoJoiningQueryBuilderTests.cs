using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class TableInfoJoiningQueryBuilderTests:DatabaseTests
    {
        [Test]
        public void OpportunisticJoinRequired()
        {
            //tables and columns
            TableInfo head = new TableInfo(CatalogueRepository,"Head");
            ColumnInfo col1 = new ColumnInfo(CatalogueRepository,"TestResultSetNumber","int",head);
            ColumnInfo col2 = new ColumnInfo(CatalogueRepository, "PK", "int", head);

            TableInfo result = new TableInfo(CatalogueRepository, "[biochemistry]..[Result]");
            ColumnInfo col3 = new ColumnInfo(CatalogueRepository, "FK", "int", result);
            ColumnInfo col4 = new ColumnInfo(CatalogueRepository, "Code", "varchar(10)", result);
            ColumnInfo col5 = new ColumnInfo(CatalogueRepository, "[biochemistry]..[Result].[OmgBob]", "varchar(10)", result);

            //we can join on col2 = col3
            CatalogueRepository.JoinInfoFinder.AddJoinInfo(col3, col2, ExtractionJoinType.Right, "");

            //CASE 1 : Only 1 column used so no join needed
            var queryBuilder = new QueryBuilder(null, null);
            var icol1 = new ColumnInfoToIColumn(col1);
            icol1.Order = 1;
            queryBuilder.AddColumn(icol1);

            TableInfo primary;
            var tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out primary);
            
            Assert.AreEqual(1,tablesUsed.Count);
            Assert.AreEqual(head,tablesUsed[0]);

            //CASE 2 : 2 columns used one from each table so join is needed
            queryBuilder = new QueryBuilder(null, null);
            queryBuilder.AddColumn(new ColumnInfoToIColumn(col1));

            var icol4 = new ColumnInfoToIColumn(col4);
            icol4.Order = 2;
            queryBuilder.AddColumn(icol4);

            tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out primary);
            
            Assert.AreEqual(2, tablesUsed.Count);
            Assert.AreEqual(head, tablesUsed[0]);
            Assert.AreEqual(result, tablesUsed[1]);

            Assert.AreEqual(CollapseWhitespace(@"SELECT 
TestResultSetNumber,
Code
FROM 
[biochemistry]..[Result] Right JOIN Head ON FK = PK"),CollapseWhitespace(queryBuilder.SQL));

            var spontContainer = new SpontaneouslyInventedFilterContainer(null, null, FilterContainerOperation.AND);

            var spontFilter = new SpontaneouslyInventedFilter(spontContainer, "[biochemistry]..[Result].[OmgBob] = 'T'",
                "My Filter", "Causes spontaneous requirement for joining compeltely", null);
            spontContainer.AddChild(spontFilter);


            //CASE 3 : Only 1 column from Head but filter contains a reference to Result column
            queryBuilder = new QueryBuilder(null, null);
            queryBuilder.AddColumn(new ColumnInfoToIColumn(col1));

            //without the filter
            tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out primary);
            Assert.AreEqual(1, tablesUsed.Count);
            
            //set the filter
            queryBuilder.RootFilterContainer = spontContainer;

            //this is super sneaky but makes the queryBuilder populate it's Filters property... basically your not supposed to use SqlQueryBuilderHelper for this kind of thing
            Console.WriteLine(queryBuilder.SQL);
            queryBuilder.ParameterManager.ClearNonGlobals();

            //with the filter
            tablesUsed = SqlQueryBuilderHelper.GetTablesUsedInQuery(queryBuilder, out primary);
            Assert.AreEqual(2, tablesUsed.Count);

            

        }

    }
}
