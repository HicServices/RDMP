// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.ANOEngineering;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution.Operations;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using Tests.Common.Scenarios;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.Integration;

public class ForwardEngineerANOCatalogueTests : TestsRequiringFullAnonymisationSuite
{
    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        BlitzMainDataTables();

        if(ANOStore_Database.Exists())
            DeleteTables(ANOStore_Database);
    }

    [Test]
    public void PlanManagementTest()
    {
        var dbName = TestDatabaseNames.GetConsistentName("PlanManagementTests");

        var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);

        db.Create(true);

        var bulk = new BulkTestsData(CatalogueRepository, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer), 100);
        bulk.SetupTestData();
        bulk.ImportAsCatalogue();

        var planManager = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator, bulk.catalogue)
 {
     TargetDatabase = db
 };

        //no operations are as yet configured
        Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

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
        CreateMigrationRules(planManager, bulk);

        //rules should pass
        Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

        var chi = bulk.GetColumnInfo("chi");
        Assert.Throws<Exception>(() =>
            {
                planManager.GetPlanForColumnInfo(chi).Plan = Plan.Drop;
                planManager.GetPlanForColumnInfo(chi).Check(new ThrowImmediatelyCheckNotifier());

            }

            ,"Should not be able to drop primary key column");

        db.Drop();
    }


    [Test]
    public void CreateANOVersionTest()
    {
        var dbName = TestDatabaseNames.GetConsistentName("CreateANOVersionTest");

        var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);

        db.Create(true);

        var bulk = new BulkTestsData(CatalogueRepository, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer), 100);
        bulk.SetupTestData();
        bulk.ImportAsCatalogue();

        var planManager = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator,bulk.catalogue)
 {
     TargetDatabase = db
 };

        //setup test rules for migrator
        CreateMigrationRules(planManager, bulk);

        //rules should pass checks
        Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

        var engine = new ForwardEngineerANOCatalogueEngine(RepositoryLocator, planManager);
        engine.Execute();

        var anoCatalogue = CatalogueRepository.GetAllObjects<Catalogue>().Single(c => c.Folder.StartsWith("\\ano"));
        Assert.IsTrue(anoCatalogue.Exists());

        db.Drop();

        var exports = CatalogueRepository.GetAllObjects<ObjectExport>().Length;
        var imports = CatalogueRepository.GetAllObjects<ObjectImport>().Length;

        Assert.AreEqual(exports, imports);
        Assert.IsTrue(exports > 0);
    }

    [Test]
    public void CreateANOVersionTest_IntIdentity()
    {
        var dbName = TestDatabaseNames.GetConsistentName("CreateANOVersionTest");

        //setup the anonymisation database (destination)
        var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);

        db.Create(true);

        //Create this table in the scratch database
        var tbl = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer).CreateTable("MyTable", new[]
        {
            new DatabaseColumnRequest("id", "int identity(1,1)", false) {IsPrimaryKey = true},
            new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof (string), 10), false)
        });

        var cata = Import(tbl,out var ti, out var cols);

        var planManager = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator, cata)
        {
            TargetDatabase = db
        };

        var nameCol = cols.Single(c => c.GetRuntimeName().Equals("Name"));

        //setup test rules for migrator
        planManager.Plans[nameCol].Plan = Plan.Drop;

        //rules should pass checks
        planManager.Check(new ThrowImmediatelyCheckNotifier());

        var engine = new ForwardEngineerANOCatalogueEngine(RepositoryLocator, planManager);
        engine.Execute();

        var anoCatalogue = CatalogueRepository.GetAllObjects<Catalogue>().Single(c => c.Folder.StartsWith("\\ano"));
        Assert.IsTrue(anoCatalogue.Exists());

        //should only be one (the id column
        Assert.AreEqual(1,anoCatalogue.CatalogueItems.Length);
        var idColInAnoDatabase = anoCatalogue.CatalogueItems[0].ColumnInfo;
        Assert.AreEqual("int", idColInAnoDatabase.Data_type);
            
        db.Drop();

        var exports = CatalogueRepository.GetAllObjects<ObjectExport>().Length;
        var imports = CatalogueRepository.GetAllObjects<ObjectImport>().Length;

        Assert.AreEqual(exports, imports);
        Assert.IsTrue(exports > 0);
    }



    [Test]
    [TestCase(false,false)]
    [TestCase(false, true)]
    [TestCase(true,false)]
    [TestCase(true,true)]
    public void CreateANOVersion_TestSkippingTables(bool tableInfoAlreadyExistsForSkippedTable,bool putPlanThroughSerialization)
    {
        var dbFrom = From;
        var dbTo = To;

        dbFrom.Create(true);
        dbTo.Create(true);

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

        var i1 = new TableInfoImporter(CatalogueRepository, tblFromHeads);
        i1.DoImport(out var fromHeadsTableInfo, out var fromHeadsColumnInfo);

        var i2 = new TableInfoImporter(CatalogueRepository, tblFromNeck);
        i2.DoImport(out var fromNeckTableInfo, out var fromNeckColumnInfo);
            
        //Table already exists but does the in Catalogue reference exist?
        if(tableInfoAlreadyExistsForSkippedTable)
        {
            var i3 = new TableInfoImporter(CatalogueRepository, tblToNeck);
            i3.DoImport(out var toNecksTableInfo, out var toNecksColumnInfo);
        }

        //Create a JoinInfo so the query builder knows how to connect the tables
        new JoinInfo(CatalogueRepository,
            fromHeadsColumnInfo.Single(c => c.GetRuntimeName().Equals("Vertebrae")),
            fromNeckColumnInfo.Single(c => c.GetRuntimeName().Equals("Vertebrae")), ExtractionJoinType.Inner, null
        );

        var cataEngineer = new ForwardEngineerCatalogue(fromHeadsTableInfo, fromHeadsColumnInfo);
        cataEngineer.ExecuteForwardEngineering(out var cata, out var cataItems, out var extractionInformations);

        var cataEngineer2 = new ForwardEngineerCatalogue(fromNeckTableInfo, fromNeckColumnInfo);
        cataEngineer2.ExecuteForwardEngineering(cata);

        //4 extraction informations in from Catalogue (2 from Heads and 2 from Necks)
        Assert.AreEqual(cata.GetAllExtractionInformation(ExtractionCategory.Any).Length, 4);

        //setup ANOTable on head
        var anoTable = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOSkullColor", "C")
            {
                NumberOfCharactersToUseInAnonymousRepresentation = 10
            };
        anoTable.SaveToDatabase();
        anoTable.PushToANOServerAsNewTable("varchar(10)",new ThrowImmediatelyCheckNotifier());

        //////////////////The actual test!/////////////////
        var planManager = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator,cata);

        //ano the table SkullColor
        var scPlan = planManager.GetPlanForColumnInfo(fromHeadsColumnInfo.Single(col => col.GetRuntimeName().Equals("SkullColor")));
        scPlan.ANOTable = anoTable;
        scPlan.Plan = Plan.ANO;

        if (putPlanThroughSerialization)
        {
            var asString = JsonConvertExtensions.SerializeObject(planManager, RepositoryLocator);

            planManager = (ForwardEngineerANOCataloguePlanManager)JsonConvertExtensions.DeserializeObject( asString, typeof(ForwardEngineerANOCataloguePlanManager), RepositoryLocator);
        }
            
        //not part of serialization
        planManager.TargetDatabase = dbTo;
        planManager.SkippedTables.Add(fromNeckTableInfo);//skip the necks table because it already exists (ColumnInfos may or may not exist but physical table definetly does)

        var engine =  new ForwardEngineerANOCatalogueEngine(RepositoryLocator, planManager);

        if (!tableInfoAlreadyExistsForSkippedTable)
        {
            var ex = Assert.Throws<Exception>(engine.Execute);
            Assert.IsTrue(Regex.IsMatch(ex.InnerException.Message, "Found '0' ColumnInfos called"));
            Assert.IsTrue(Regex.IsMatch(ex.InnerException.Message, "[Necks].[SpineColor]"));

            return;
        }

        engine.Execute();

        var newCata = CatalogueRepository.GetAllObjects<Catalogue>().Single(c => c.Name.Equals("ANOHeads"));
        Assert.IsTrue(newCata.Exists());

        var newCataItems = newCata.CatalogueItems;
        Assert.AreEqual(newCataItems.Length, 4);

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

    [Test]
    public void CreateANOVersionTest_LookupsAndExtractionInformations()
    {
        var dbName = TestDatabaseNames.GetConsistentName("CreateANOVersionTest");

        var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);

        db.Create(true);

        var bulk = new BulkTestsData(CatalogueRepository, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer), 100);
        bulk.SetupTestData();
        bulk.ImportAsCatalogue();

        //Create a lookup table on the server
        var lookupTbl = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer).CreateTable("z_sexLookup", new[]
        {
            new DatabaseColumnRequest("Code", "varchar(1)"){IsPrimaryKey = true},
            new DatabaseColumnRequest("hb_Code", "varchar(1)"){IsPrimaryKey = true},
            new DatabaseColumnRequest("Description", "varchar(100)")
        });

        //import a reference to the table
        var importer = new TableInfoImporter(CatalogueRepository,lookupTbl);
        importer.DoImport(out var lookupTableInfo, out var lookupColumnInfos);

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

        eiPostcode.SelectSQL = $"LEFT(10,{eiPostcode.ColumnInfo.TableInfo.Name}.[current_postcode])";
        eiPostcode.Alias = "MyMutilatedColumn";
        eiPostcode.SaveToDatabase();

        //add a combo transform
        var ciComboCol = new CatalogueItem(CatalogueRepository, bulk.catalogue, "ComboColumn");

        var colForename = bulk.columnInfos.Single(c => c.GetRuntimeName() == "forename");
        var colSurname =  bulk.columnInfos.Single(c => c.GetRuntimeName() == "surname");

        var eiComboCol = new ExtractionInformation(CatalogueRepository, ciComboCol, colForename,
            $"{colForename} + ' ' + {colSurname}")
        {
            Alias = "ComboColumn"
        };
        eiComboCol.SaveToDatabase();

        var eiDataLoadRunId = bulk.extractionInformations.Single(ei => ei.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID));
        eiDataLoadRunId.DeleteInDatabase();


        var lookup = new Lookup(CatalogueRepository, lookupColumnInfos[2], ciSex.ColumnInfo, lookupColumnInfos[0],ExtractionJoinType.Left, null);

        //now let's make it worse, let's assume the sex code changes per healthboard therefore the join to the lookup requires both fields sex and hb_extract
        var compositeLookup = new LookupCompositeJoinInfo(CatalogueRepository, lookup, ciHb.ColumnInfo, lookupColumnInfos[1]);

        //now let's make the _Desc field in the original Catalogue
        var orderToInsertDescriptionFieldAt = ciSex.ExtractionInformation.Order;

        //bump everyone down 1
        foreach (var toBumpDown in bulk.catalogue.CatalogueItems.Select(ci=>ci.ExtractionInformation).Where(e =>e != null && e.Order > orderToInsertDescriptionFieldAt))
        {
            toBumpDown.Order++;
            toBumpDown.SaveToDatabase();
        }

        var ciDescription = new CatalogueItem(CatalogueRepository, bulk.catalogue, "Sex_Desc");
        var eiDescription = new ExtractionInformation(CatalogueRepository, ciDescription, lookupColumnInfos[2],lookupColumnInfos[2].Name)
            {
                Alias = "Sex_Desc",
                Order = orderToInsertDescriptionFieldAt +1,
                ExtractionCategory = ExtractionCategory.Supplemental
            };
        eiDescription.SaveToDatabase();

        bulk.catalogue.ClearAllInjections();

        //check it worked
        var qb = new QueryBuilder(null,null);
        qb.AddColumnRange(bulk.catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
            
        //The query builder should be able to succesfully create SQL
        Console.WriteLine(qb.SQL);
            
        //there should be 2 tables involved in the query [z_sexLookup] and [BulkData]
        Assert.AreEqual(2,qb.TablesUsedInQuery.Count);

        //the query builder should have identified the lookup
        Assert.AreEqual(lookup,qb.GetDistinctRequiredLookups().Single());

        //////////////////////////////////////////////////////////////////////////////////////The Actual Bit Being Tested////////////////////////////////////////////////////
        var planManager = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator,bulk.catalogue)
 {
     TargetDatabase = db
 };

        //setup test rules for migrator
        CreateMigrationRules(planManager, bulk);
            
        //rules should pass checks
        Assert.DoesNotThrow(() => planManager.Check(new ThrowImmediatelyCheckNotifier()));

        var engine = new ForwardEngineerANOCatalogueEngine(RepositoryLocator, planManager);
        engine.Execute();
        //////////////////////////////////////////////////////////////////////////////////////End The Actual Bit Being Tested////////////////////////////////////////////////////

        var anoCatalogue = CatalogueRepository.GetAllObjects<Catalogue>().Single(c => c.Folder.StartsWith("\\ano"));
        Assert.IsTrue(anoCatalogue.Exists());

        //The new Catalogue should have the same number of ExtractionInformations
        var eiSource = bulk.catalogue.GetAllExtractionInformation(ExtractionCategory.Any).OrderBy(ei=>ei.Order).ToArray();
        var eiDestination = anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).OrderBy(ei=>ei.Order).ToArray();
            
        Assert.AreEqual(eiSource.Length,eiDestination.Length,"Both the new and the ANO catalogue should have the same number of ExtractionInformations (extractable columns)");

        for (var i = 0; i < eiSource.Length; i++)
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
        var qbdestination = new QueryBuilder(null, null);
        qbdestination.AddColumnRange(anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

        //The query builder should be able to succesfully create SQL
        Console.WriteLine(qbdestination.SQL);

        var anoEiPostcode = anoCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(ei => ei.GetRuntimeName().Equals("MyMutilatedColumn"));
            
        //The transform on postcode should have been refactored to the new table name and preserve the scalar function LEFT...
        Assert.AreEqual($"LEFT(10,{anoEiPostcode.ColumnInfo.TableInfo.GetFullyQualifiedName()}.[current_postcode])",anoEiPostcode.SelectSQL);

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

        db.Drop();

        var exports = CatalogueRepository.GetAllObjects<ObjectExport>().Length;
        var imports = CatalogueRepository.GetAllObjects<ObjectImport>().Length;

        Assert.AreEqual(exports, imports);
        Assert.IsTrue(exports > 0);

            
    }


    private void CreateMigrationRules(ForwardEngineerANOCataloguePlanManager planManager, BulkTestsData bulk)
    {
        var chi = bulk.GetColumnInfo("chi");

        var anoChi = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOCHI", "C")
            {
                NumberOfIntegersToUseInAnonymousRepresentation = 9,
                NumberOfCharactersToUseInAnonymousRepresentation = 1
            };
        anoChi.SaveToDatabase();
        anoChi.PushToANOServerAsNewTable(chi.Data_type,new ThrowImmediatelyCheckNotifier());

        planManager.GetPlanForColumnInfo(chi).Plan = Plan.ANO;
        planManager.GetPlanForColumnInfo(chi).ANOTable = anoChi;

        var dob = bulk.GetColumnInfo("date_of_birth");
        planManager.GetPlanForColumnInfo(dob).Plan = Plan.Dilute;
        planManager.GetPlanForColumnInfo(dob).Dilution = new RoundDateToMiddleOfQuarter();

        var postcode = bulk.GetColumnInfo("current_postcode");
        planManager.GetPlanForColumnInfo(postcode).Plan = Plan.Dilute;
        planManager.GetPlanForColumnInfo(postcode).Dilution = new ExcludeRight3OfUKPostcodes();
    }
}