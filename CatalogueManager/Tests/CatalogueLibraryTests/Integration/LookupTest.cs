using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class LookupTest : DatabaseTests
    {

        [Test]
        [ExpectedException]
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

                new Lookup(CatalogueRepository, child, child2, child3, ExtractionJoinType.Left, null);
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

        [Test]
        public void CreateLookup_linkWithOtherTable()
        {
            TableInfo parent = null;
            TableInfo parent2 = null;

            ColumnInfo child = null;
            ColumnInfo child2 = null;
            ColumnInfo child3 = null;

            try
            {
                parent = new TableInfo(CatalogueRepository, "unit_test_CreateLookup");
                parent2 = new TableInfo(CatalogueRepository, "unit_test_CreateLookupOther");
                child = new ColumnInfo(CatalogueRepository, "unit_test_CreateLookup", "int", parent); //lookup desc
                child2 = new ColumnInfo(CatalogueRepository, "unit_test_CreateLookup", "int", parent2); //fk in data table
                child3 = new ColumnInfo(CatalogueRepository, "unit_test_CreateLookup", "int", parent); //pk in lookup

                new Lookup(CatalogueRepository, child, child2, child3, ExtractionJoinType.Left, null);

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
                throw ex;
            }
            finally
            {
                //cleanup
                if(composite != null)
                    composite.DeleteInDatabase();
    
                lookup.DeleteInDatabase();

                desc.DeleteInDatabase();
                fk.DeleteInDatabase();
                pk.DeleteInDatabase();
                fk2.DeleteInDatabase(); 
                pk2.DeleteInDatabase();
                fkTable.DeleteInDatabase(); 
                pkTable.DeleteInDatabase();
            }
        }


        [Test]
        public void CompositeLookupTest_SQL()
        {
             
            //this only works for MSSQL Servers
            if (CatalogueRepository.DiscoveredServer.DatabaseType != DatabaseType.MicrosoftSQLServer)
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

                string joinSQL = JoinHelper.GetJoinSQL(lookup);

                Assert.AreEqual(joinSQL,"UnitTest_Biochemistry Left JOIN UnitTest_BiochemistryLookup ON UnitTest_BCTestCode = UnitTest_TestCode");

                //Create the composite lookup
                composite = new LookupCompositeJoinInfo(CatalogueRepository, lookup, fk2, pk2);

                string joinSQL_AfterAddingCompositeKey = JoinHelper.GetJoinSQL(lookup);

                Assert.AreEqual(joinSQL_AfterAddingCompositeKey, "UnitTest_Biochemistry Left JOIN UnitTest_BiochemistryLookup ON UnitTest_BCTestCode = UnitTest_TestCode AND UnitTest_BCHealthBoard = UnitTest_Healthboard");
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw ex;
            }
            finally
            {
                //cleanup
                if (composite != null)
                    composite.DeleteInDatabase();

                lookup.DeleteInDatabase();

                desc.DeleteInDatabase();
                fk.DeleteInDatabase();
                pk.DeleteInDatabase();
                fk2.DeleteInDatabase();
                pk2.DeleteInDatabase();
                fkTable.DeleteInDatabase();
                pkTable.DeleteInDatabase();
            }
        }

    }
}
