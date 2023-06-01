// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class LookupTest : DatabaseTests
{

    [Test]
    public void Test_MultipleLookupReferences()
    {
        BlitzMainDataTables();

        var tiHeader = new TableInfo(CatalogueRepository,"Head");
        var tiHeader_Code = new ColumnInfo(CatalogueRepository,"code","",tiHeader);
                       
        var tiLookup = new TableInfo(CatalogueRepository,"z_HeadLookup");
        var tiLookup_Code = new ColumnInfo(CatalogueRepository,"code","",tiLookup);
        var tiLookup_Desc = new ColumnInfo(CatalogueRepository,"desc","",tiLookup);

        var lookup = new Lookup(CatalogueRepository,tiLookup_Desc,tiHeader_Code,tiLookup_Code,ExtractionJoinType.Left,null);

        var cata1 = new Catalogue(CatalogueRepository,"Catalogue1");
        var cata2 = new Catalogue(CatalogueRepository,"Catalogue2");

        var cata1_code = new CatalogueItem(CatalogueRepository,cata1,"code");
        var cata1_desc = new CatalogueItem(CatalogueRepository,cata1,"desc");
        new ExtractionInformation(CatalogueRepository,cata1_code,tiHeader_Code,"[tbl]..[code]");
        new ExtractionInformation(CatalogueRepository,cata1_desc,tiLookup_Desc,"[lookup]..[desc]");

        var cata2_code = new CatalogueItem(CatalogueRepository,cata2,"code");
        var cata2_desc = new CatalogueItem(CatalogueRepository,cata2,"desc");
        new ExtractionInformation(CatalogueRepository,cata2_code,tiHeader_Code,"[tbl]..[code]");
        new ExtractionInformation(CatalogueRepository,cata2_desc,tiLookup_Desc,"[lookup]..[desc]");
            
        new CatalogueChildProvider(CatalogueRepository,null, new ThrowImmediatelyCheckNotifier(){ThrowOnWarning=true},null);
            
    }

    [Test]
    public void CreateLookup_linkWithSelfThrowsException()
    {

        TableInfo parent=null;
        ColumnInfo child=null;
        ColumnInfo child2=null;
        ColumnInfo child3=null;

        try
        {
            parent = new TableInfo(CatalogueRepository, "unit_test_CreateLookup");
            child = new ColumnInfo(CatalogueRepository, "unit_test_CreateLookup", "int", parent);
            child2 = new ColumnInfo(CatalogueRepository, "unit_test_CreateLookup", "int", parent);
            child3 = new ColumnInfo(CatalogueRepository, "unit_test_CreateLookup", "int", parent);

            Assert.Throws<ArgumentException>(()=>new Lookup(CatalogueRepository, child, child2, child3, ExtractionJoinType.Left, null));
        }
        finally 
        {
            //cleanup
            try{child.DeleteInDatabase();}catch (Exception){}
            try{child2.DeleteInDatabase();}catch (Exception){}
            try{child3.DeleteInDatabase();}catch (Exception){}
            try{parent.DeleteInDatabase();}catch (Exception){}
                
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CreateLookup_linkWithOtherTable(bool memoryRepo)
    {
        var repo = memoryRepo? (ICatalogueRepository)new MemoryCatalogueRepository():CatalogueRepository;

        TableInfo parent = null;
        TableInfo parent2 = null;

        ColumnInfo child = null;
        ColumnInfo child2 = null;
        ColumnInfo child3 = null;

        try
        {
            parent = new TableInfo(repo, "unit_test_CreateLookup");
            parent2 = new TableInfo(repo, "unit_test_CreateLookupOther");
            child = new ColumnInfo(repo, "unit_test_CreateLookup", "int", parent); //lookup desc
            child2 = new ColumnInfo(repo, "unit_test_CreateLookup", "int", parent2); //fk in data table
            child3 = new ColumnInfo(repo, "unit_test_CreateLookup", "int", parent); //pk in lookup

            new Lookup(repo, child, child2, child3, ExtractionJoinType.Left, null);

            Assert.AreEqual(child.GetAllLookupForColumnInfoWhereItIsA(LookupType.Description).Length, 1);
            Assert.AreEqual(child2.GetAllLookupForColumnInfoWhereItIsA(LookupType.Description).Length, 0);
            Assert.AreEqual(child.GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey).Length, 0);
            Assert.AreEqual(child2.GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey).Length, 1);
            Assert.AreEqual(child3.GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey).Length, 1);


            Assert.IsTrue(parent.IsLookupTable());
            Assert.IsFalse(parent2.IsLookupTable());
        }
        finally
        {
            //cleanup
            try { child.DeleteInDatabase(); }catch (Exception) { }
            try { child2.DeleteInDatabase(); }catch (Exception) { }
            try { child3.DeleteInDatabase(); }catch (Exception) { }
            try { parent.DeleteInDatabase(); }catch (Exception) { }
            try { parent2.DeleteInDatabase(); }catch (Exception) { }

        }
    }

    [Test]
    public void CompositeLookupTest()
    {
           

        TableInfo fkTable = null;
        TableInfo pkTable = null;
        ColumnInfo desc = null;
        ColumnInfo fk = null;
        ColumnInfo pk = null;

        ColumnInfo fk2 = null;
        ColumnInfo pk2 = null;

        Lookup lookup = null;
        LookupCompositeJoinInfo composite=null;

        try
        {

            //table 1 - the dataset table, it has 2 foreign keys e.g. TestCode, Healthboard
            fkTable = new TableInfo(CatalogueRepository, "UnitTest_Biochemistry");
            fk = new ColumnInfo(CatalogueRepository, "UnitTest_BCTestCode", "int", fkTable);
            fk2 = new ColumnInfo(CatalogueRepository, "UnitTest_BCHealthBoard", "int", fkTable);

            //table 2 - the lookup table, it has 2 primary keys e.g. TestCode,Healthboard and 1 description e.g. TestDescription (the Healthboard makes it a composite JOIN which allows for the same TestCode being mapped to a different discription in Tayside vs Fife (healthboard)
            pkTable = new TableInfo(CatalogueRepository, "UnitTest_BiochemistryLookup");
            pk = new ColumnInfo(CatalogueRepository, "UnitTest_TestCode", "int", pkTable);
            pk2 = new ColumnInfo(CatalogueRepository, "UnitTest_Healthboard", "int", pkTable);
            desc = new ColumnInfo(CatalogueRepository, "UnitTest_TestDescription", "int", pkTable);
            lookup = new Lookup(CatalogueRepository, desc, fk, pk, ExtractionJoinType.Left, null);

            Assert.AreEqual(lookup.PrimaryKey.Name, pk.Name);
            Assert.AreEqual(lookup.PrimaryKey.ID, pk.ID);

            Assert.AreEqual(lookup.ForeignKey.Name, fk.Name);
            Assert.AreEqual(lookup.ForeignKey.ID, fk.ID);

            Assert.AreEqual(lookup.Description.Name, desc.Name);
            Assert.AreEqual(lookup.Description.ID, desc.ID);

            //Create the composite lookup
            composite = new LookupCompositeJoinInfo(CatalogueRepository, lookup, fk2, pk2);

            Assert.AreEqual(composite.OriginalLookup_ID, lookup.ID);
                    
            Assert.AreEqual(composite.PrimaryKey.ID, pk2.ID);
            Assert.AreEqual(composite.PrimaryKey_ID, pk2.ID);
            Assert.AreEqual(composite.PrimaryKey.Name, pk2.Name);

            Assert.AreEqual(composite.ForeignKey.ID, fk2.ID);
            Assert.AreEqual(composite.ForeignKey_ID, fk2.ID);
            Assert.AreEqual(composite.ForeignKey.Name, fk2.Name);

            //get a fresh copy out of memory now that we have created the Lookup composite key, confirm the integrity of that relationship
            Assert.AreEqual(lookup.GetSupplementalJoins().Count() , 1);
            Assert.AreEqual(lookup.GetSupplementalJoins().Cast<LookupCompositeJoinInfo>().First().ID, composite.ID);

            composite.DeleteInDatabase();
            composite = null;

            Assert.AreEqual(lookup.GetSupplementalJoins().Count(), 0);
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            throw;
        }
        finally
        {
            //cleanup
            if(composite != null)
                composite.DeleteInDatabase();
    
            lookup?.DeleteInDatabase();

            desc?.DeleteInDatabase();
            fk?.DeleteInDatabase();
            pk?.DeleteInDatabase();
            fk2?.DeleteInDatabase(); 
            pk2?.DeleteInDatabase();
            fkTable?.DeleteInDatabase(); 
            pkTable?.DeleteInDatabase();
        }
    }


    [Test]
    public void CompositeLookupTest_SQL()
    {
             
        //this only works for MSSQL Servers
        if (CatalogueTableRepository.DiscoveredServer.DatabaseType != DatabaseType.MicrosoftSQLServer)
            Assert.Ignore("This test only targets Microsft SQL Servers");

        TableInfo fkTable = null;
        TableInfo pkTable = null;
        ColumnInfo desc = null;
        ColumnInfo fk = null;
        ColumnInfo pk = null;

        ColumnInfo fk2 = null;
        ColumnInfo pk2 = null;

        Lookup lookup = null;
        LookupCompositeJoinInfo composite = null;

        try
        {

            //table 1 - the dataset table, it has 2 foreign keys e.g. TestCode, Healthboard
            fkTable = new TableInfo(CatalogueRepository, "UnitTest_Biochemistry");
            fk = new ColumnInfo(CatalogueRepository, "UnitTest_BCTestCode", "int", fkTable);
            fk2 = new ColumnInfo(CatalogueRepository, "UnitTest_BCHealthBoard", "int", fkTable);

            //table 2 - the lookup table, it has 2 primary keys e.g. TestCode,Healthboard and 1 description e.g. TestDescription (the Healthboard makes it a composite JOIN which allows for the same TestCode being mapped to a different discription in Tayside vs Fife (healthboard)
            pkTable = new TableInfo(CatalogueRepository, "UnitTest_BiochemistryLookup");
            pk = new ColumnInfo(CatalogueRepository, "UnitTest_TestCode", "int", pkTable);
            pk2 = new ColumnInfo(CatalogueRepository, "UnitTest_Healthboard", "int", pkTable);
            desc = new ColumnInfo(CatalogueRepository, "UnitTest_TestDescription", "int", pkTable);
            lookup = new Lookup(CatalogueRepository, desc, fk, pk, ExtractionJoinType.Left, null);

            var joinSQL = JoinHelper.GetJoinSQL(lookup);

            Assert.AreEqual(joinSQL,"UnitTest_Biochemistry Left JOIN UnitTest_BiochemistryLookup ON UnitTest_BCTestCode = UnitTest_TestCode");

            //Create the composite lookup
            composite = new LookupCompositeJoinInfo(CatalogueRepository, lookup, fk2, pk2);

            var joinSQL_AfterAddingCompositeKey = JoinHelper.GetJoinSQL(lookup);

            Assert.AreEqual(joinSQL_AfterAddingCompositeKey, "UnitTest_Biochemistry Left JOIN UnitTest_BiochemistryLookup ON UnitTest_BCTestCode = UnitTest_TestCode AND UnitTest_BCHealthBoard = UnitTest_Healthboard");
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            throw;
        }
        finally
        {
            //cleanup
            if (composite != null)
                composite.DeleteInDatabase();

            lookup?.DeleteInDatabase();

            desc?.DeleteInDatabase();
            fk?.DeleteInDatabase();
            pk?.DeleteInDatabase();
            fk2?.DeleteInDatabase();
            pk2?.DeleteInDatabase();
            fkTable?.DeleteInDatabase();
            pkTable?.DeleteInDatabase();
        }
    }


    [Test]
    public void LookupTest_CustomSql()
    {

        //this only works for MSSQL Servers
        if (CatalogueTableRepository.DiscoveredServer.DatabaseType != DatabaseType.MicrosoftSQLServer)
            Assert.Ignore("This test only targets Microsft SQL Servers");

        TableInfo fkTable = null;
        TableInfo pkTable = null;
        ColumnInfo desc = null;
        ColumnInfo fk = null;
        ColumnInfo pk = null;

        ColumnInfo fk2 = null;
        ColumnInfo pk2 = null;

        Lookup lookup = null;

        try
        {

            //table 1 - the dataset table, it has 2 foreign keys e.g. TestCode, Healthboard
            fkTable = new TableInfo(CatalogueRepository, "UnitTest_Biochemistry");
            fk = new ColumnInfo(CatalogueRepository, "One", "int", fkTable);
            fk2 = new ColumnInfo(CatalogueRepository, "Two", "int", fkTable);

            //table 2 - the lookup table, it has 2 primary keys e.g. TestCode,Healthboard and 1 description e.g. TestDescription (the Healthboard makes it a composite JOIN which allows for the same TestCode being mapped to a different discription in Tayside vs Fife (healthboard)
            pkTable = new TableInfo(CatalogueRepository, "UnitTest_BiochemistryLookup");
            pk = new ColumnInfo(CatalogueRepository, "One", "int", pkTable);
            pk2 = new ColumnInfo(CatalogueRepository, "Two", "int", pkTable);
            desc = new ColumnInfo(CatalogueRepository, "UnitTest_TestDescription", "int", pkTable);
            lookup = new Lookup(CatalogueRepository, desc, fk, pk, ExtractionJoinType.Left, null);

            var joinSQL = JoinHelper.GetJoinSQL(lookup);

            Assert.AreEqual(joinSQL, "UnitTest_Biochemistry Left JOIN UnitTest_BiochemistryLookup ON One = One");

            //Create the custom lookup
            var cmd = new ExecuteCommandSetExtendedProperty(new ThrowImmediatelyActivator(RepositoryLocator),
                new[] { lookup },
                ExtendedProperty.CustomJoinSql,
                @"{0}.One={1}.One AND
({0}.Two = {0}.Two OR 
    ({0}.{Two} is null AND {1}.Two is null)
)");
            cmd.Execute();


            var joinSQL_AfterAddingCompositeKey = JoinHelper.GetJoinSQL(lookup);

            Assert.AreEqual(joinSQL_AfterAddingCompositeKey,
                "UnitTest_Biochemistry Left JOIN UnitTest_BiochemistryLookup ON UnitTest_Biochemistry.One=UnitTest_BiochemistryLookup.One AND (UnitTest_Biochemistry.Two = UnitTest_Biochemistry.Two OR      (UnitTest_Biochemistry.{Two} is null AND UnitTest_BiochemistryLookup.Two is null) )");
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            throw;
        }
        finally
        {
            lookup?.DeleteInDatabase();

            desc?.DeleteInDatabase();
            fk?.DeleteInDatabase();
            pk?.DeleteInDatabase();
            fk2?.DeleteInDatabase();
            pk2?.DeleteInDatabase();
            fkTable?.DeleteInDatabase();
            pkTable?.DeleteInDatabase();
        }
    }

    [TestCase(LookupTestCase.SingleKeySingleDescriptionNoVirtualColumn)]
    [TestCase(LookupTestCase.SingleKeySingleDescription)]
    public void TestLookupCommand(LookupTestCase testCase)
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var dt = new DataTable();
        dt.Columns.Add("ID");
        dt.Columns.Add("SendingLocation");
        dt.Columns.Add("DischargeLocation");
        dt.Columns.Add("Country");

        var maintbl = db.CreateTable("MainDataset", dt);

        var mainCata = Import(maintbl);

        var dtLookup = new DataTable();
        dtLookup.Columns.Add("LocationCode");
        dtLookup.Columns.Add("Line1");
        dtLookup.Columns.Add("Line2");
        dtLookup.Columns.Add("Postcode");
        dtLookup.Columns.Add("Country");

        var lookuptbl = db.CreateTable("Lookup", dtLookup);

        var lookupCata = Import(lookuptbl);

        var fkEi = mainCata.GetAllExtractionInformation(ExtractionCategory.Any).Single(n => n.GetRuntimeName() == "SendingLocation");
        var fk = mainCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "SendingLocation");
        var pk = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "LocationCode");

        var descLine1 = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Line1");
        var descLine2 = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Line2");

        ExecuteCommandCreateLookup cmd = null;

        var sqlBefore = GetSql(mainCata);

        switch (testCase)
        {
            case LookupTestCase.SingleKeySingleDescriptionNoVirtualColumn:
                cmd = new ExecuteCommandCreateLookup(CatalogueRepository, fkEi, descLine1, pk,null, false);
                cmd.Execute();

                //sql should not have changed because we didn't create an new ExtractionInformation virtual column
                Assert.AreEqual(sqlBefore,GetSql(mainCata));
                break;
            case LookupTestCase.SingleKeySingleDescription:
                cmd = new ExecuteCommandCreateLookup(CatalogueRepository, fkEi, descLine1, pk,null, true);
                cmd.Execute();

                //should have the lookup join and the virtual column _Desc
                var sqlAfter = GetSql(mainCata);
                Assert.IsTrue(sqlAfter.Contains("JOIN"));
                Assert.IsTrue(sqlAfter.Contains("SendingLocation_Desc"));
                break;
            default:
                throw new ArgumentOutOfRangeException("testCase");
        }
            
        foreach (var d in CatalogueRepository.GetAllObjects<Lookup>())
            d.DeleteInDatabase();
        foreach (var d in CatalogueRepository.GetAllObjects<LookupCompositeJoinInfo>())
            d.DeleteInDatabase();
        foreach (var d in CatalogueRepository.GetAllObjects<TableInfo>())
            d.DeleteInDatabase();
        foreach (var d in CatalogueRepository.GetAllObjects<Catalogue>())
            d.DeleteInDatabase();

        maintbl.Drop();
        lookuptbl.Drop();
    }

    private string GetSql(ICatalogue mainCata)
    {
        mainCata.ClearAllInjections();

        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(mainCata.GetAllExtractionInformation(ExtractionCategory.Any));
        return qb.SQL;
    }
}

public enum LookupTestCase
{
    SingleKeySingleDescriptionNoVirtualColumn,
    SingleKeySingleDescription,
}