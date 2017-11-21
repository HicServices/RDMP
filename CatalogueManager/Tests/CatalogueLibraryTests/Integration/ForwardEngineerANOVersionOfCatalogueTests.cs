using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnonymisationTests;
using CatalogueLibrary.ANOEngineering;
using CatalogueLibrary.Data.DataLoad;
using Diagnostics.TestData;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class ForwardEngineerANOVersionOfCatalogueTests:TestsRequiringANOStore
    {
        [Test]
        public void PreRunChecks()
        {
            var dbName = TestDatabaseNames.GetConsistentName("ForwardEngineerANOVersionOfCatalogueTests");

            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);
            
            db.Create(true);
            
            BulkTestsData bulk = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn,100);
            bulk.SetupTestData();
            bulk.ImportAsCatalogue();

            var migrator = new ForwardEngineerANOVersionOfCatalogue(bulk.catalogue);
            migrator.TargetDatabase = db;

            //no operations are as yet configured
            Assert.DoesNotThrow(()=>migrator.Check(new ThrowImmediatelyCheckNotifier()));

            //create a table with the same name in the endpoint database to confirm that that's a problem
            db.CreateTable(bulk.tableInfo.GetRuntimeName(), new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("fish", "varchar(100)")
            });

            //throws because table already exists
            Assert.Throws<Exception>(() => migrator.Check(new ThrowImmediatelyCheckNotifier()));

            db.ExpectTable(bulk.tableInfo.GetRuntimeName()).Drop();

            //back to being fine again
            Assert.DoesNotThrow(() => migrator.Check(new ThrowImmediatelyCheckNotifier()));

            //setup test rules for migrator
            CreateMigrationRules(migrator,bulk);

            //rules should pass
            Assert.DoesNotThrow(() => migrator.Check(new ThrowImmediatelyCheckNotifier()));

            var chi_num_of_curr_record = bulk.GetColumnInfo("chi_num_of_curr_record");
            Assert.Throws<ArgumentException>(() => migrator.SetPlan(chi_num_of_curr_record, ForwardEngineerANOVersionOfCatalogue.Plan.Drop));



            db.ForceDrop();
        }

        private void CreateMigrationRules(ForwardEngineerANOVersionOfCatalogue migrator, BulkTestsData bulk)
        {
            
            var chi = bulk.GetColumnInfo("chi");

            var anoChi = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOCHI", "C");
            anoChi.NumberOfIntegersToUseInAnonymousRepresentation = 9;
            anoChi.NumberOfCharactersToUseInAnonymousRepresentation = 1;
            anoChi.SaveToDatabase();
            anoChi.PushToANOServerAsNewTable(chi.Data_type,new ThrowImmediatelyCheckNotifier());
            
            migrator.SetPlan(chi,ForwardEngineerANOVersionOfCatalogue.Plan.ANO);
            migrator.SetPlannedANOTable(chi,anoChi);

            var dob = bulk.GetColumnInfo("date_of_birth");
            migrator.SetPlan(dob, ForwardEngineerANOVersionOfCatalogue.Plan.Dillute);
            migrator.SetPlannedDilution(dob,new RoundDateToMiddleOfQuarter());

            var postcode = bulk.GetColumnInfo("current_postcode");
            migrator.SetPlan(postcode, ForwardEngineerANOVersionOfCatalogue.Plan.Dillute);
            migrator.SetPlannedDilution(postcode,new ExcludeRight3OfUKPostcodes());


        }
    }
}
