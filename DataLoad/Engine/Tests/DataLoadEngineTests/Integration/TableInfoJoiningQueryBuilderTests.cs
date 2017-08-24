using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using LoadModules.Generic.Mutilators.QueryBuilders;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class TableInfoJoiningQueryBuilderTests:DatabaseTests
    {
        [Test]
        public void GetMultiTableJoinSQL()
        {
            TableInfo tableP = new TableInfo(CatalogueRepository, "PrimaryTable");
            ColumnInfo tablePColPk = new ColumnInfo(CatalogueRepository, "tablePColPk", "int", tableP);
            tableP.IsPrimaryExtractionTable = true;
            tableP.SaveToDatabase();
            
            TableInfo tableM = new TableInfo(CatalogueRepository, "MiddleTable");
            ColumnInfo tableMColFk = new ColumnInfo(CatalogueRepository, "tableMColFk", "int", tableM);
            ColumnInfo tableMColPk1 = new ColumnInfo(CatalogueRepository, "tableMColPk1", "int", tableM);
            ColumnInfo tableMColPk2 = new ColumnInfo(CatalogueRepository, "tableMColPk2", "int", tableM);


            TableInfo tableL = new TableInfo(CatalogueRepository, "LowestTable");
            ColumnInfo tableLColFk1 = new ColumnInfo(CatalogueRepository, "tableLColFk1", "int", tableL);
            ColumnInfo tableLColFk2 = new ColumnInfo(CatalogueRepository, "tableLColFk2", "int", tableL);


            CatalogueRepository.JoinInfoFinder.AddJoinInfo(tableMColFk,tablePColPk, ExtractionJoinType.Right,"");

            CatalogueRepository.JoinInfoFinder.AddJoinInfo(tableLColFk1, tableMColPk1, ExtractionJoinType.Right, "");
            CatalogueRepository.JoinInfoFinder.AddJoinInfo(tableLColFk2, tableMColPk2, ExtractionJoinType.Right, "");
            
            try
            {
                TableInfoJoiningQueryBuilder qb = new TableInfoJoiningQueryBuilder();
                string answer = qb.GetJoinSQL(tableP, tableM, tableL);

                Assert.AreEqual("PrimaryTable Left JOIN MiddleTable ON tableMColFk = tablePColPk Left JOIN LowestTable ON tableLColFk1 = tableMColPk1 AND tableLColFk2 = tableMColPk2",answer);

            }
            finally
            {
                CatalogueRepository.JoinInfoFinder.GetAllJoinInfoForColumnInfoWhereItIsAForeignKey(tableLColFk2).Single().DeleteInDatabase();
                CatalogueRepository.JoinInfoFinder.GetAllJoinInfoForColumnInfoWhereItIsAForeignKey(tableLColFk1).Single().DeleteInDatabase();
                CatalogueRepository.JoinInfoFinder.GetAllJoinInfoForColumnInfoWhereItIsAForeignKey(tableMColFk).Single().DeleteInDatabase();

                tableL.DeleteInDatabase();
                tableM.DeleteInDatabase();
                tableP.DeleteInDatabase();
            }
        }

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

            Assert.AreEqual(@"SELECT 
TestResultSetNumber,
Code
FROM 
[biochemistry]..[Result] Right JOIN Head ON FK = PK", queryBuilder.SQL.Trim());

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
