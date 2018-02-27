using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnonymisationTests;
using CatalogueLibrary;
using CatalogueLibrary.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using Diagnostics.TestData;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class ForwardEngineerANOCatalogueTests : TestsRequiringFullAnonymisationSuite
    {
        [SetUp]
        public void ClearRemnants()
        {
            foreach (var e in CatalogueRepository.GetAllObjects<ObjectExport>())
                e.DeleteInDatabase();

            foreach (var i in CatalogueRepository.GetAllObjects<ObjectImport>())
                i.DeleteInDatabase();

            foreach (var j in CatalogueRepository.JoinInfoFinder.GetAllJoinInfos())
                j.DeleteInDatabase();

            foreach (var p in CatalogueRepository.GetAllObjects<PreLoadDiscardedColumn>())
                p.DeleteInDatabase();

            foreach (var l in CatalogueRepository.GetAllObjects<Lookup>())
                l.DeleteInDatabase();

            //cleanup
            foreach (var t in CatalogueRepository.GetAllObjects<TableInfo>())
                t.DeleteInDatabase();

            foreach (var c in CatalogueRepository.GetAllObjects<Catalogue>())
                c.DeleteInDatabase();

            foreach (var a in CatalogueRepository.GetAllObjects<ANOTable>())
                a.DeleteInDatabase();
        }

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

                //Create a JoinInfo so the query builder knows how to connect the tables
                CatalogueRepository.JoinInfoFinder.AddJoinInfo(
                    fromHeadsColumnInfo.Single(c => c.GetRuntimeName().Equals("Vertebrae")),
                    fromNeckColumnInfo.Single(c => c.GetRuntimeName().Equals("Vertebrae")), ExtractionJoinType.Inner, null
                    );

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
             
                //////////////////The actual test!/////////////////
                var plan = new ForwardEngineerANOCataloguePlanManager(cata);
                
                //ano the table SkullColor
                plan.SetPlannedANOTable(fromHeadsColumnInfo.Single(col => col.GetRuntimeName().Equals("SkullColor")), anoTable);
                plan.TargetDatabase = dbTo;
                plan.SkippedTables.Add(fromNeckTableInfo);//skip the necks table because it already exists (ColumnInfos may or may not exist but physical table definetly does)

                var engine =  new ForwardEngineerANOCatalogueEngine(CatalogueRepository, plan);

                if (!tableInfoAlreadyExistsForSkippedTable)
                {
                    var ex = Assert.Throws<Exception>(engine.Execute);
                    Assert.IsTrue(Regex.IsMatch(ex.InnerException.Message, "Found '0' ColumnInfos called 'SpineColor'"));
                    return;
                }
                else
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
                Assert.IsTrue(newCataItems.Single(ci => ci.Name.Equals("ANOSkullColor")).ColumnInfo.ANOTable_ID == anoTable.ID);


                var newSpineColorColumnInfo = newCataItems.Single(ci => ci.Name.Equals("ANOSkullColor")).ColumnInfo;

                //table info already existed, make sure the new CatalogueItems point to the same columninfos / table infos
                Assert.IsTrue(newCataItems.Select(ci=>ci.ColumnInfo).Contains(newSpineColorColumnInfo));
                
            }
            finally
            {
                dbFrom.ForceDrop();
                dbTo.ForceDrop();
            }
        }
        
        [Test]
        public void CreateANOVersionTest_LookupsAndExtractionInformations()
        {
            var dbName = TestDatabaseNames.GetConsistentName("CreateANOVersionTest");

            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);

            db.Create(true);

            BulkTestsData bulk = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn, 100);
            bulk.SetupTestData();
            bulk.ImportAsCatalogue();

            //Create a lookup table on the server
            var lookupTbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("z_sexLookup", new[]
            {
                new DatabaseColumnRequest("Code", "varchar(1)"){IsPrimaryKey = true},
                new DatabaseColumnRequest("hb_Code", "varchar(1)"){IsPrimaryKey = true},
                new DatabaseColumnRequest("Description", "varchar(100)")
            });

            //import a reference to the table
            TableInfoImporter importer = new TableInfoImporter(CatalogueRepository,lookupTbl);

            ColumnInfo[] lookupColumnInfos;
            TableInfo lookupTableInfo;
            importer.DoImport(out lookupTableInfo,out lookupColumnInfos);

            //Create a Lookup reference
            var ciSex = bulk.catalogue.CatalogueItems.Single(c => c.Name == "sex");
            var ciHb = bulk.catalogue.CatalogueItems.Single(c => c.Name == "hb_extract");
            
            var eiChi = bulk.extractionInformations.Single(ei => ei.GetRuntimeName() == "chi");
            eiChi.IsExtractionIdentifier = true;
            eiChi.SaveToDatabase();

            var eiCentury = bulk.extractionInformations.Single(ei => ei.GetRuntimeName() == "century");
            eiCentury.HashOnDataRelease = true;
            eiCentury.ExtractionCategory = ExtractionCategory.Internal;
            eiCentury.SaveToDatabase();

            //add a transform
            var eiPostcode = bulk.extractionInformations.Single(ei => ei.GetRuntimeName() == "current_postcode");

            eiPostcode.SelectSQL = string.Format("LEFT(10,{0}.[current_postcode])", eiPostcode.ColumnInfo.TableInfo.Name);
            eiPostcode.Alias = "MyMutilatedColumn";
            eiPostcode.SaveToDatabase();

            //add a combo transform
            var ciComboCol = new CatalogueItem(CatalogueRepository, bulk.catalogue, "ComboColumn");

            var colForename = bulk.columnInfos.Single(c => c.GetRuntimeName() == "forename");
            var colSurname =  bulk.columnInfos.Single(c => c.GetRuntimeName() == "surname");

            var eiComboCol = new ExtractionInformation(CatalogueRepository, ciComboCol, colForename,colForename + " + ' ' + " + colSurname );
            eiComboCol.Alias = "ComboColumn";
            eiComboCol.SaveToDatabase();

            var lookup = new Lookup(CatalogueRepository, lookupColumnInfos[2], ciSex.ColumnInfo, lookupColumnInfos[0],ExtractionJoinType.Left, null);
            
            //now lets make it worse, lets assume the sex code changes per healthboard therefore the join to the lookup requires both fields sex and hb_extract
            var compositeLookup = new LookupCompositeJoinInfo(CatalogueRepository, lookup, ciHb.ColumnInfo, lookupColumnInfos[1]);

            //now lets make the _Desc field in the original Catalogue
            int orderToInsertDescriptionFieldAt = ciSex.ExtractionInformation.Order;

            //bump everyone down 1
            foreach (var toBumpDown in bulk.catalogue.CatalogueItems.Select(ci=>ci.ExtractionInformation).Where(e => e.Order > orderToInsertDescriptionFieldAt))
            {
                toBumpDown.Order++;
                toBumpDown.SaveToDatabase();
            }
            
            var ciDescription = new CatalogueItem(CatalogueRepository, bulk.catalogue, "Sex_Desc");
            var eiDescription = new ExtractionInformation(CatalogueRepository, ciDescription, lookupColumnInfos[2],lookupColumnInfos[2].Name);
            eiDescription.Alias = "Sex_Desc";
            eiDescription.Order = orderToInsertDescriptionFieldAt +1;
            eiDescription.ExtractionCategory = ExtractionCategory.Supplemental;
            eiDescription.SaveToDatabase();

            //check it worked
            QueryBuilder qb = new QueryBuilder(null,null);
            qb.AddColumnRange(bulk.catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
            
            //The query builder should be able to succesfully create SQL
            Console.WriteLine(qb.SQL);
            
            //there should be 2 tables involved in the query [z_sexLookup] and [BulkData]
            Assert.AreEqual(2,qb.TablesUsedInQuery.Count);

            //the query builder should have identified the lookup
            Assert.AreEqual(lookup,qb.GetDistinctRequiredLookups().Single());
            
            //////////////////////////////////////////////////////////////////////////////////////The Actual Bit Being Tested////////////////////////////////////////////////////
            var planManager = new ForwardEngineerANOCataloguePlanManager(bulk.catalogue);
            planManager.TargetDatabase = db;

            //setup test rules for migrator
            CreateMigrationRules(planManager, bulk);

            //rules should pass checks
            Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

            var engine = new ForwardEngineerANOCatalogueEngine(CatalogueRepository, planManager);
            engine.Execute();
            //////////////////////////////////////////////////////////////////////////////////////End The Actual Bit Being Tested////////////////////////////////////////////////////

            var anoCatalogue = CatalogueRepository.GetAllCatalogues().Single(c => c.Folder.Path.StartsWith("\\ano"));
            Assert.IsTrue(anoCatalogue.Exists());

            //The new Catalogue should have the same number of ExtractionInformations
            var eiSource = bulk.catalogue.GetAllExtractionInformation(ExtractionCategory.Any).OrderBy(ei=>ei.Order).ToArray();
            var eiDestination = anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).OrderBy(ei=>ei.Order).ToArray();

            Assert.AreEqual(eiSource.Length,eiDestination.Length,"Both the new and the ANO catalogue should have the same number of ExtractionInformations (extractable columns)");

            for (int i = 0; i < eiSource.Length; i++)
            {
                Assert.AreEqual(eiSource[i].Order , eiDestination[i].Order,"ExtractionInformations in the source and destination Catalogue should have the same order");
                
                Assert.AreEqual(eiSource[i].GetRuntimeName(), 
                    eiDestination[i].GetRuntimeName().Replace("ANO",""), "ExtractionInformations in the source and destination Catalogue should have the same names (excluding ANO prefix)");

                Assert.AreEqual(eiSource[i].ExtractionCategory, eiDestination[i].ExtractionCategory, "Old / New ANO ExtractionInformations did not match on ExtractionCategory");
                Assert.AreEqual(eiSource[i].IsExtractionIdentifier, eiDestination[i].IsExtractionIdentifier, "Old / New ANO ExtractionInformations did not match on IsExtractionIdentifier");
                Assert.AreEqual(eiSource[i].HashOnDataRelease, eiDestination[i].HashOnDataRelease, "Old / New ANO ExtractionInformations did not match on HashOnDataRelease");
                Assert.AreEqual(eiSource[i].IsPrimaryKey, eiDestination[i].IsPrimaryKey, "Old / New ANO ExtractionInformations did not match on IsPrimaryKey");
            }

            //check it worked
            QueryBuilder qbdestination = new QueryBuilder(null, null);
            qbdestination.AddColumnRange(anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            //The query builder should be able to succesfully create SQL
            Console.WriteLine(qbdestination.SQL);

            var anoEiPostcode = anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(ei => ei.GetRuntimeName().Equals("MyMutilatedColumn"));
            
            //The transform on postcode should have been refactored to the new table name and preserve the scalar function LEFT...
            Assert.AreEqual(string.Format("LEFT(10,{0}.[current_postcode])", anoEiPostcode.ColumnInfo.TableInfo.GetFullyQualifiedName()),anoEiPostcode.SelectSQL);

            var anoEiComboCol = anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(ei => ei.GetRuntimeName().Equals("ComboColumn"));

            //The transform on postcode should have been refactored to the new table name and preserve the scalar function LEFT...
            Assert.AreEqual(string.Format("{0}.[forename] + ' ' + {0}.[surname]", anoEiPostcode.ColumnInfo.TableInfo.GetFullyQualifiedName()), anoEiComboCol.SelectSQL);

            //there should be 2 tables involved in the query [z_sexLookup] and [BulkData]
            Assert.AreEqual(2, qbdestination.TablesUsedInQuery.Count);

            //the query builder should have identified the lookup but it should be the new one not the old one
            Assert.AreEqual(1, qbdestination.GetDistinctRequiredLookups().Count(), "New query builder for ano catalogue did not correctly identify that there was a Lookup");
            Assert.AreNotEqual(lookup, qbdestination.GetDistinctRequiredLookups().Single(), "New query builder for ano catalogue identified the OLD Lookup!");
            
            Assert.AreEqual(1, qbdestination.GetDistinctRequiredLookups().Single().GetSupplementalJoins().Count(),"The new Lookup did not have the composite join key (sex/hb_extract)");
            Assert.AreNotEqual(compositeLookup, qbdestination.GetDistinctRequiredLookups().Single().GetSupplementalJoins(), "New query builder for ano catalogue identified the OLD LookupCompositeJoinInfo!");

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
            planManager.SetPlan(dob, ForwardEngineerANOCataloguePlanManager.Plan.Dilute);
            planManager.SetPlannedDilution(dob,new RoundDateToMiddleOfQuarter());

            var postcode = bulk.GetColumnInfo("current_postcode");
            planManager.SetPlan(postcode, ForwardEngineerANOCataloguePlanManager.Plan.Dilute);
            planManager.SetPlannedDilution(postcode,new ExcludeRight3OfUKPostcodes());
        }
    }
}