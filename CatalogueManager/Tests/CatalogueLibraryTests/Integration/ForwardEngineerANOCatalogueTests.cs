using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnonymisationTests;
using CatalogueLibrary;
using CatalogueLibrary.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
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

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CreateANOVersion_TestSkippingTables(bool tableInfoAlreadyExistsForSkippedTable)
        {
            var dbFrom = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(TestDatabaseNames.GetConsistentName("CreateANOVersion_TestSkippingTables_From"));
            var dbTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(TestDatabaseNames.GetConsistentName("CreateANOVersion_TestSkippingTables_To"));

            dbFrom.Create(true);
            dbTo.Create(true);

            try
            {
                var tblFromHeads = dbFrom.CreateTable("Heads", new[]
                {
                    new DatabaseColumnRequest("SkullColor", "varchar(10)"),
                    new DatabaseColumnRequest("Vertebrae", "varchar(25)")
                });

                var cols = new[]
                {
                    new DatabaseColumnRequest("SpineColor", "varchar(10)"),
                    new DatabaseColumnRequest("Vertebrae", "varchar(25)")
                };

                var tblFromNeck = dbFrom.CreateTable("Necks",cols);

                //Necks table already exists in the destination so will be skipped for migration but still needs to be imported
                var tblToNeck = dbTo.CreateTable("Necks", cols);


                TableInfo fromHeadsTableInfo;
                ColumnInfo[] fromHeadsColumnInfo;
                TableInfo fromNeckTableInfo;
                ColumnInfo[] fromNeckColumnInfo;
                TableInfo toNecksTableInfo = null;
                ColumnInfo[] toNecksColumnInfo = null;

                TableInfoImporter i1 = new TableInfoImporter(CatalogueRepository, tblFromHeads);
                i1.DoImport(out fromHeadsTableInfo,out fromHeadsColumnInfo);

                TableInfoImporter i2 = new TableInfoImporter(CatalogueRepository, tblFromNeck);
                i2.DoImport(out fromNeckTableInfo,out fromNeckColumnInfo);
                
                //Table already exists but does the in Catalogue reference exist?
                if(tableInfoAlreadyExistsForSkippedTable)
                {
                    TableInfoImporter i3 = new TableInfoImporter(CatalogueRepository, tblToNeck);
                    i3.DoImport(out toNecksTableInfo,out toNecksColumnInfo);
                }

                var cataEngineer = new ForwardEngineerCatalogue(fromHeadsTableInfo, fromHeadsColumnInfo, true);
                Catalogue cata;
                CatalogueItem[] cataItems;
                ExtractionInformation[] extractionInformations;
                cataEngineer.ExecuteForwardEngineering(out cata,out cataItems,out extractionInformations);

                var cataEngineer2 = new ForwardEngineerCatalogue(fromNeckTableInfo, fromNeckColumnInfo, true);
                cataEngineer2.ExecuteForwardEngineering(cata);

                //4 extraction informations in from Catalogue (2 from Heads and 2 from Necks)
                Assert.AreEqual(cata.GetAllExtractionInformation(ExtractionCategory.Any).Count(),4);

                //setup ANOTable on head
                var anoTable = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOSkullColor", "C");
                anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 10;
                anoTable.SaveToDatabase();
                anoTable.PushToANOServerAsNewTable("varchar(10)",new ThrowImmediatelyCheckNotifier());
                try
                {
                    //////////////////The actual test!/////////////////
                    var plan = new ForwardEngineerANOCataloguePlanManager(cata);
                
                    //ano the table SkullColor
                    plan.SetPlannedANOTable(fromHeadsColumnInfo.Single(col => col.GetRuntimeName().Equals("SkullColor")), anoTable);
                    plan.TargetDatabase = dbTo;
                    plan.SkippedTables.Add(fromNeckTableInfo);//skip the necks table because it already exists (ColumnInfos may or may not exist but physical table definetly does)

                    var engine =  new ForwardEngineerANOCatalogueEngine(CatalogueRepository, plan);
                    engine.Execute();
                
                    var newCata = CatalogueRepository.GetAllCatalogues().Single(c => c.Name.Equals("ANOHeads"));
                    Assert.IsTrue(newCata.Exists());

                    var newCataItems = newCata.CatalogueItems;
                    Assert.AreEqual(newCataItems.Count(),4);

                    //should be extraction informations
                    //all extraction informations should point to the new table location
                    Assert.IsTrue(newCataItems.All(ci => ci.ExtractionInformation.SelectSQL.Contains(dbTo.GetRuntimeName())));

                    //these columns should all exist
                    Assert.IsTrue(newCataItems.Any(ci => ci.ExtractionInformation.SelectSQL.Contains("SkullColor")));
                    Assert.IsTrue(newCataItems.Any(ci => ci.ExtractionInformation.SelectSQL.Contains("SpineColor")));
                    Assert.IsTrue(newCataItems.Any(ci => ci.ExtractionInformation.SelectSQL.Contains("Vertebrae"))); //actually there will be 2 copies of this one from Necks one from Heads

                    //new ColumnInfo should have a reference to the anotable
                    Assert.IsTrue(newCataItems.Single(ci => ci.Name.Equals("SkullColor")).ColumnInfo.ANOTable_ID == anoTable.ID);


                    var newSpineColorColumnInfo = newCataItems.Single(ci => ci.Name.Equals("SpineColor")).ColumnInfo;

                    if (tableInfoAlreadyExistsForSkippedTable)
                    {
                        //table info already existed, make sure the new CatalogueItems point to the same columninfos / table infos
                        Assert.IsTrue(toNecksColumnInfo.Contains(newSpineColorColumnInfo));
                    }
                    else
                        Assert.IsTrue(newSpineColorColumnInfo != null);
                }
                finally
                {
                    foreach (var export in CatalogueRepository.GetAllObjects<ObjectExport>())
                        export.DeleteInDatabase();

                    //cleanup
                    foreach (var t in CatalogueRepository.GetAllObjects<TableInfo>())
                        t.DeleteInDatabase();

                    foreach (var c in CatalogueRepository.GetAllObjects<Catalogue>())
                        c.DeleteInDatabase();
                    
                    anoTable.DeleteInDatabase();
                }
                
            }
            finally
            {
                dbFrom.ForceDrop();
                dbTo.ForceDrop();
            }

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