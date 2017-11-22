using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnonymisationTests;
using CatalogueLibrary.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using Diagnostics.TestData;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class ForwardEngineerANOCatalogueTests:TestsRequiringANOStore
    {
        [Test]
        public void PlanManagementTest()
        {
            var dbName = TestDatabaseNames.GetConsistentName("PlanManagementTests");

            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);
            
            db.Create(true);
            
            BulkTestsData bulk = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn,100);
            bulk.SetupTestData();
            bulk.ImportAsCatalogue();

            var planManager = new ForwardEngineerANOCataloguePlanManager(bulk.catalogue);
            planManager.TargetDatabase = db;

            //no operations are as yet configured
            Assert.DoesNotThrow(()=>planManager.Check(new ThrowImmediatelyCheckNotifier()));

            //create a table with the same name in the endpoint database to confirm that that's a problem
            db.CreateTable(bulk.tableInfo.GetRuntimeName(), new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("fish", "varchar(100)")
            });

            //throws because table already exists
            Assert.Throws<Exception>(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

            db.ExpectTable(bulk.tableInfo.GetRuntimeName()).Drop();

            //back to being fine again
            Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

            //setup test rules for migrator
            CreateMigrationRules(planManager,bulk);

            //rules should pass
            Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

            var chi_num_of_curr_record = bulk.GetColumnInfo("chi_num_of_curr_record");
            Assert.Throws<ArgumentException>(() => planManager.SetPlan(chi_num_of_curr_record, ForwardEngineerANOCataloguePlanManager.Plan.Drop));

            db.ForceDrop();
        }


        [Test]
        public void CreateANOVersionTest()
        {
            var dbName = TestDatabaseNames.GetConsistentName("CreateANOVersionTest");

            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);

            db.Create(true);

            BulkTestsData bulk = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn, 100);
            bulk.SetupTestData();
            bulk.ImportAsCatalogue();

            var planManager = new ForwardEngineerANOCataloguePlanManager(bulk.catalogue);
            planManager.TargetDatabase = db;

            //setup test rules for migrator
            CreateMigrationRules(planManager, bulk);

            //rules should pass checks
            Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

            var engine = new ForwardEngineerANOCatalogueEngine(CatalogueRepository, planManager);
            engine.Execute();

            var anoCatalogue = CatalogueRepository.GetAllCatalogues().Single(c => c.Folder.Path.StartsWith("\\ano"));
            Assert.IsTrue(anoCatalogue.Exists());

            db.ForceDrop();

            var exports = CatalogueRepository.GetAllObjects<ObjectExport>().Count();
            var imports = CatalogueRepository.GetAllObjects<ObjectImport>().Count();

            Assert.AreEqual(exports, imports);
            Assert.IsTrue(exports > 0);
        }


        private void CreateMigrationRules(ForwardEngineerANOCataloguePlanManager planManager, BulkTestsData bulk)
        {
            
            var chi = bulk.GetColumnInfo("chi");

            var anoChi = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOCHI", "C");
            anoChi.NumberOfIntegersToUseInAnonymousRepresentation = 9;
            anoChi.NumberOfCharactersToUseInAnonymousRepresentation = 1;
            anoChi.SaveToDatabase();
            anoChi.PushToANOServerAsNewTable(chi.Data_type,new ThrowImmediatelyCheckNotifier());
            
            planManager.SetPlan(chi,ForwardEngineerANOCataloguePlanManager.Plan.ANO);
            planManager.SetPlannedANOTable(chi,anoChi);

            var dob = bulk.GetColumnInfo("date_of_birth");
            planManager.SetPlan(dob, ForwardEngineerANOCataloguePlanManager.Plan.Dillute);
            planManager.SetPlannedDilution(dob,new RoundDateToMiddleOfQuarter());

            var postcode = bulk.GetColumnInfo("current_postcode");
            planManager.SetPlan(postcode, ForwardEngineerANOCataloguePlanManager.Plan.Dillute);
            planManager.SetPlannedDilution(postcode,new ExcludeRight3OfUKPostcodes());
        }
    }
}