// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using DataLoadEngineTests.Integration.RelationalBulkTestDataTests.TestData;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration.RelationalBulkTestDataTests
{
    public class RelationalBulkTestQueryBuilding:DatabaseTests
    {

        [Test]
        public void JoinInfosQueryBuilding()
        {
            var bulkData = GetBulkDataWithImportedCatalogues();

            try
            {
                var pk1 = bulkData.CIATestEventCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("PKAgencyCodename"));
                var colsFromTable2 = bulkData.CIATestReportCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).ToArray();

                QueryBuilder qb = new QueryBuilder("", "");
                qb.AddColumn(pk1);
                qb.AddColumnRange(colsFromTable2);

                //it should be impossible without a join info
                QueryBuildingException ex = Assert.Throws<QueryBuildingException>(() => Console.WriteLine(qb.SQL));
                Assert.IsTrue(ex.Message.Contains("There were 2 Tables involved in assembling this query ("));
                Assert.IsTrue(ex.Message.Contains("["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport]"));
                Assert.IsTrue(ex.Message.Contains("["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent]"));
                Assert.IsTrue(ex.Message.Contains("of which  0 were Lookups and 0 were JoinInfos, this leaves 2+ tables unjoined (no JoinInfo found)"));


                //add a join info
                var fk1 = colsFromTable2.Single(e => e.GetRuntimeName().Equals("PKFKAgencyCodename"));
                new JoinInfo(CatalogueRepository,fk1.ColumnInfo, pk1.ColumnInfo, ExtractionJoinType.Right, null);

                //reset the query builder
                qb.RegenerateSQL();

                //get new sql
                string sql = null;
                Assert.DoesNotThrow(() => sql = qb.SQL);


                Assert.AreEqual(CollapseWhitespace(@"
SELECT
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[PKAgencyCodename],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[PKID],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[ReportText],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[ReportDate],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[PKFKAgencyCodename],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[PKFKClearenceLevel],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory2],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory3]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport] Right JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent] ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[PKFKAgencyCodename] = ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[PKAgencyCodename]
"), CollapseWhitespace(sql));

                //now make it a combo join but in wrong direction
                var pk2 = bulkData.CIATestEventCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("PKClearenceLevel"));
                var fk2 = colsFromTable2.Single(e => e.GetRuntimeName().Equals("PKFKClearenceLevel"));
                new JoinInfo(CatalogueRepository,fk2.ColumnInfo, pk2.ColumnInfo, ExtractionJoinType.Left, null);//notice they are in different directions

                QueryBuildingException ex2 = Assert.Throws<QueryBuildingException>(qb.RegenerateSQL);

                Assert.IsTrue(ex2.Message.Contains(@"Found 2 possible Joins for "));

                Assert.IsTrue(ex2.Message.Contains(
@" It was not possible to configure a Composite Join because:
 Although joins are all between the same tables in the same direction, the ExtractionJoinTypes are different (e.g. LEFT and RIGHT) which prevents forming a Combo AND based join using both relationships"
                    ));


                //now delete it and recreate it in the right direction
                var j2 = CatalogueRepository.GetAllObjects<JoinInfo>().Single(j => j.ForeignKey_ID == fk2.ColumnInfo.ID);
                j2.DeleteInDatabase();
                new JoinInfo(CatalogueRepository,fk2.ColumnInfo, pk2.ColumnInfo, ExtractionJoinType.Right, null);//notice they are in different directions

                qb.RegenerateSQL();
                Assert.DoesNotThrow(() => sql = qb.SQL);
                Assert.IsTrue(
                    sql.Contains(
@"["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport] Right JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent] ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[PKFKAgencyCodename] = ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[PKAgencyCodename] AND ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[PKFKClearenceLevel] = ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[PKClearenceLevel]"
));

                //clean it up to allow for deletes in cleanup phase
                CatalogueRepository.GetAllObjects<JoinInfo>().Single(j => j.ForeignKey_ID == fk1.ColumnInfo.ID).DeleteInDatabase();
                CatalogueRepository.GetAllObjects<JoinInfo>().Single(j => j.ForeignKey_ID == fk2.ColumnInfo.ID).DeleteInDatabase();

            }
            finally
            {
                try
                {
                    bulkData.DeleteCatalogues();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cleanup failed" + e);
                }
            }
        }

        [Test]
        public void LookupTableQueryBuilding()
        {
            var bulkData = GetBulkDataWithImportedCatalogues();

            try
            {
                var dataset_fk1 =
                    bulkData.CIATestReportCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                    .Single(e => e.GetRuntimeName().Equals("CIATestInformantSignatory1"));
                var dataset_fk2 =
                    bulkData.CIATestReportCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                        .Single(e => e.GetRuntimeName().Equals("CIATestInformantSignatory2"));
                var dataset_fk3 =
                    bulkData.CIATestReportCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                        .Single(e => e.GetRuntimeName().Equals("CIATestInformantSignatory3"));

                var lookup_desc = bulkData.CIATestInformantCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                    .Single(e => e.GetRuntimeName().Equals("Name"));

                var lookup_pk = bulkData.CIATestInformantCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                    .Single(e => e.GetRuntimeName().Equals("ID"));

                dataset_fk1.Order = 1;
                dataset_fk1.SaveToDatabase();

                dataset_fk2.Order = 3;
                dataset_fk2.SaveToDatabase();

                dataset_fk3.Order = 5;
                dataset_fk3.SaveToDatabase();

                lookup_pk.Order = 7;
                lookup_pk.SaveToDatabase();

                lookup_desc.Order = 9;
                lookup_desc.SaveToDatabase();

                QueryBuilder qb = new QueryBuilder(null, null);
                qb.AddColumn(lookup_desc);
                qb.AddColumn(lookup_pk);

                Assert.AreEqual(CollapseWhitespace(@"
SELECT 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant].[ID],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant].[Name]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant]"), CollapseWhitespace(qb.SQL));


                //will be complaint about join info lookup tables missing
                qb.AddColumn(dataset_fk1);
                Assert.Throws<QueryBuildingException>(() => Console.WriteLine(qb.SQL));

                //remove the lookup primary key from the query - nobody wants to see IDs in their query twice
                qb.SelectColumns.Remove(qb.SelectColumns.Single(qtc => qtc.UnderlyingColumn.ID == lookup_pk.ColumnInfo.ID));

                //create the lookup relationship
                var cleanup1 = new Lookup(CatalogueRepository, lookup_desc.ColumnInfo, dataset_fk1.ColumnInfo, lookup_pk.ColumnInfo, ExtractionJoinType.Left, "");

                qb.RegenerateSQL();
                Assert.AreEqual(CollapseWhitespace(@"SELECT 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1],
lookup_1.[Name]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport] Left JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant] AS lookup_1 ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1] = lookup_1.[ID]"),CollapseWhitespace(qb.SQL));


                //now do it properly 
                qb.SelectColumns.Remove(qb.SelectColumns.Single(qtc => qtc.UnderlyingColumn == lookup_desc.ColumnInfo));
                var desc1 = new CatalogueItem(CatalogueRepository, bulkData.CIATestReportCatalogue, "NameOfSignatory1");
                var desc2 = new CatalogueItem(CatalogueRepository, bulkData.CIATestReportCatalogue, "NameOfSignatory2");
                var desc3 = new CatalogueItem(CatalogueRepository, bulkData.CIATestReportCatalogue, "NameOfSignatory3");

                var ei1 = new ExtractionInformation(CatalogueRepository, desc1, lookup_desc.ColumnInfo, lookup_desc.SelectSQL);
                var ei2 = new ExtractionInformation(CatalogueRepository, desc2, lookup_desc.ColumnInfo, lookup_desc.SelectSQL);
                var ei3 = new ExtractionInformation(CatalogueRepository, desc3, lookup_desc.ColumnInfo, lookup_desc.SelectSQL);

                //now we have 3 new virtual columns in the catalogue all mapped to the lookup description
                ei1.Alias = "NameOfSignatory1";
                ei1.SaveToDatabase();
                ei2.Alias = "NameOfSignatory2";
                ei2.SaveToDatabase();
                ei3.Alias = "NameOfSignatory3";
                ei3.SaveToDatabase();


                //create the lookup relationships
                var cleanup2 = new Lookup(CatalogueRepository, lookup_desc.ColumnInfo, dataset_fk2.ColumnInfo, lookup_pk.ColumnInfo, ExtractionJoinType.Left, "");
                var cleanup3 = new Lookup(CatalogueRepository, lookup_desc.ColumnInfo, dataset_fk3.ColumnInfo, lookup_pk.ColumnInfo, ExtractionJoinType.Left, "");

                ei1.Order = 2;
                ei2.Order = 4;
                ei3.Order = 6;

                qb.SelectColumns.Clear();
                
                qb.AddColumnRange(new IColumn[]
                {
                    dataset_fk1,
                    ei1,
                    dataset_fk2,
                    ei2,
                    dataset_fk3,
                    ei3
                });
                
                qb.RegenerateSQL();

                Assert.AreEqual(CollapseWhitespace(@"SELECT 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1],
lookup_1.[Name] AS NameOfSignatory1,
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory2],
lookup_2.[Name] AS NameOfSignatory2,
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory3],
lookup_3.[Name] AS NameOfSignatory3
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport] Left JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant] AS lookup_1 ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1] = lookup_1.[ID]
 Left JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant] AS lookup_2 ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory2] = lookup_2.[ID]
 Left JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant] AS lookup_3 ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory3] = lookup_3.[ID]"),CollapseWhitespace(qb.SQL));


                //now we remove one of the fks from the query 
                qb.SelectColumns.Remove(qb.SelectColumns.Single(qtc => qtc.IColumn.ID == dataset_fk2.ID));
                qb.RegenerateSQL();

                //notice how now it doesn't have the fk it will think that both the lookup descriptions refer to the fk it encounters first 
                Assert.AreEqual(CollapseWhitespace(@"SELECT 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1],
lookup_1.[Name] AS NameOfSignatory1,
lookup_1.[Name] AS NameOfSignatory2,
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory3],
lookup_2.[Name] AS NameOfSignatory3
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport] Left JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant] AS lookup_1 ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory1] = lookup_1.[ID]
 Left JOIN ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestInformant] AS lookup_2 ON ["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestReport].[CIATestInformantSignatory3] = lookup_2.[ID]"),
                CollapseWhitespace(qb.SQL));

                //remove 2 lookup descriptions
                qb.SelectColumns.Remove(qb.SelectColumns.Single(qtc => qtc.IColumn.ID == ei1.ID));
                qb.SelectColumns.Remove(qb.SelectColumns.Single(qtc => qtc.IColumn.ID == ei2.ID));
                qb.RegenerateSQL();


                cleanup1.DeleteInDatabase();
                cleanup2.DeleteInDatabase();
                cleanup3.DeleteInDatabase();
            }
            finally
            {
                try
                {

                    bulkData.DeleteCatalogues();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cleanup failed:" + e);
                }
            }
        }

        private RelationalBulkTestData GetBulkDataWithImportedCatalogues()
        {
            RelationalBulkTestData bulkData = new RelationalBulkTestData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn);
            bulkData.SetupTestData();
            bulkData.ImportCatalogues();
            return bulkData;
        }
    }
}
