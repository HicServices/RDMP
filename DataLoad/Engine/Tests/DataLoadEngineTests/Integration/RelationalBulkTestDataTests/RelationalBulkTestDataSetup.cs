using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using Diagnostics.TestData.Relational;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration.RelationalBulkTestDataTests
{
    public class RelationalBulkTestDataSetup:DatabaseTests
    {
        [Test]
        public void SetupTables_Exists()
        {
            RelationalBulkTestData bulkData = new RelationalBulkTestData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn);
            bulkData.SetupTestData();

            Assert.IsTrue(bulkData.Database.ExpectTable("CIATestEvent").Exists());
        }

        [Test]
        public void ForwardEngineerCatalogue_Works()
        {
            foreach (Catalogue remnant in CatalogueRepository.GetAllCatalogues().Where(c => c.Name.Equals("CIATestEvent")))
            {
                List<ITableInfo> normalTables, lookupTables;
                remnant.GetTableInfos(out normalTables, out lookupTables);

                foreach (TableInfo normalTable in normalTables)
                    normalTable.DeleteInDatabase();

                remnant.DeleteInDatabase();
            }

            RelationalBulkTestData bulkData = new RelationalBulkTestData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn);
            bulkData.SetupTestData();


            Assert.IsNull(bulkData.CIATestEventCatalogue);
            bulkData.ImportCatalogues();
            Assert.NotNull(bulkData.CIATestEventCatalogue);
            try
            {
                Assert.AreEqual(1, CatalogueRepository.GetAllCatalogues().Count(c => c.Name.Equals("CIATestEvent")));

                QueryBuilder qb = new QueryBuilder("","");

                var extractionInformations = bulkData.CIATestEventCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Cast<IColumn>().ToArray();
                qb.AddColumnRange(extractionInformations);

                Assert.AreEqual(CollapseWhitespace(@"SELECT 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[PKAgencyCodename],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[PKClearenceLevel],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[EventName],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[TypeOfEvent],
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent].[EstimatedEventDate]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea]..[CIATestEvent]"),CollapseWhitespace(qb.SQL));

            }
            finally
            {
                bulkData.DeleteCatalogues();

                //shouldn't be any anymore
                Assert.IsFalse(CatalogueRepository.GetAllCatalogues().Any(c => c.Name.Equals("CIATestEvent")));
            }
        }        
  
        [Test]
        [Ignore("Not ready for prime time its all test data anyway for powering other tests in future")]
        public void DatabaseIsSame()
        {
            int seed = 500;

            RelationalBulkTestData bulkData = new RelationalBulkTestData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn, seed);
            bulkData.SetupTestData();

            CIATestInformant[] allInformants;
            var events = bulkData.GenerateEvents(DateTime.Now.AddYears(-5), DateTime.Now.AddYears(-3), 100, 50,20,out allInformants);
            bulkData.CommitToDatabase(events,allInformants);

            //regenerate with the same seed
            bulkData = new RelationalBulkTestData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn, seed);
            events = bulkData.GenerateEvents(DateTime.Now.AddYears(-5), DateTime.Now.AddYears(-3), 100, 50, 20, out allInformants);

            Assert.IsTrue(CIATestEvent.IsExactMatchToDatabase(events, bulkData.Database));

            Assert.IsTrue(bulkData.Database.ExpectTable("CIATestEvent").Exists());
            
        }
    }
}
