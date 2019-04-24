// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.QueryBuilding;
using Rdmp.Core.Tests.DataLoad.Engine.Integration.RelationalBulkTestDataTests.TestData;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.RelationalBulkTestDataTests
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
            foreach (Catalogue remnant in CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name.Equals("CIATestEvent")))
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
                Assert.AreEqual(1, CatalogueRepository.GetAllObjects<Catalogue>().Count(c => c.Name.Equals("CIATestEvent")));

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
                Assert.IsFalse(CatalogueRepository.GetAllObjects<Catalogue>().Any(c => c.Name.Equals("CIATestEvent")));
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
