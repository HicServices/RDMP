using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Migration;
using DataQualityEngine.Data;
using DataQualityEngine.Reports;
using Diagnostics.TestData;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;
using RDMPAutomationService.Logic.DQE;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests
{
    public class DQERunFinderTests:DatabaseTests
    {
        [Test]
        public void TestSuggestCatalogue_NoCatalogues()
        {
            var finder = new DQERunFinder(CatalogueRepository, AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults, 1, new ThrowImmediatelyDataLoadEventListener());
            Assert.IsNull(finder.SuggestRun());
        }

        [Test]
        [TestCase(AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults)]
        [TestCase(AutomationDQEJobSelectionStrategy.MostRecentlyLoadedDataset)]
        public void TestSuggestCatalogue_CatalogueReadyBecauseNeverRun(AutomationDQEJobSelectionStrategy strategy)
        {
            foreach (Catalogue cataRemnant in CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name.Equals("BulkData")))
            {
                DQERepository dqe = new DQERepository(CatalogueRepository);

                foreach (var evaluationRemnant in dqe.GetAllEvaluationsFor(cataRemnant))
                    evaluationRemnant.DeleteInDatabase();
                
                cataRemnant.DeleteInDatabase();
            }
            
            //create some test data and import it as a catalogue
            BulkTestsData testData = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn, 100); 
            testData.SetupTestData();
            testData.ImportAsCatalogue();
            try
            {
                var cata = testData.catalogue;

                //make sure bulk test data resulted in a catalogue being created
                Assert.IsNotNull(cata);
            
                //finder shouldn't currently be suggesting it because it's validation won't be set
                var finder = new DQERunFinder(CatalogueRepository, strategy, 365, new ThrowImmediatelyDataLoadEventListener());
                Assert.IsNull(finder.SuggestRun());

                testData.SetupValidationOnCatalogue();
                
                //finder should now suggest this catalogue
                Assert.AreEqual(cata,finder.SuggestRun());

                var firstTable = cata.GetTableInfoList(false).First();
                firstTable.IsTableValuedFunction = true;
                firstTable.SaveToDatabase();

                //should no longer suggest because catalogue contains a table valued function table
                Assert.IsNull(finder.SuggestRun());

                firstTable.IsTableValuedFunction = false;
                firstTable.SaveToDatabase();

                //manually run a DQE run on the dataset!
                CatalogueConstraintReport report = new CatalogueConstraintReport(cata, SpecialFieldNames.DataLoadRunID);
                report.GenerateReport(cata, new ThrowImmediatelyDataLoadEventListener(), new CancellationTokenSource().Token);

                //finder shouldn't suggest this catalogue anymore because DQE has been run recently (within 100 days!)
                Assert.IsNull(finder.SuggestRun());
            }
            finally
            {

                testData.Destroy();
                testData.DeleteCatalogue();
            }

        }
    }
}
