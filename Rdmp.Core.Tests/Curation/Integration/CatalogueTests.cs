// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class CatalogueTests : UnitTests
    {
        [Test]
        public void Test_GetObjects_Catalogue()
        {
            Catalogue catalogueWithId = new Catalogue(Repository, "bob");
            Catalogue[] catas = Repository.GetAllObjects<Catalogue>();

            Assert.IsTrue(catas.Length > 0);

            catalogueWithId.DeleteInDatabase();
        }

        [Test]
        public void SettingPropertyViaRelationshipDoesntSave_NoticeHowYouHaveToCacheThePropertyCatalogueToSetIt()
        {
            Catalogue c = new Catalogue(Repository,"frank");
            CatalogueItem ci = new CatalogueItem(Repository,c,"bob");


            var cata = ci.Catalogue;
            cata.Name = "fish2";
            cata.SaveToDatabase();
            Assert.AreEqual("fish2", ci.Catalogue.Name);

            //now thanks to lazy this works... but it's ambiguous (it works if the property is referenced via IInjectKnown<T>)
            ci.Catalogue.Name = "fish";
            ci.Catalogue.SaveToDatabase();
            Assert.AreEqual("fish",ci.Catalogue.Name);

            c.DeleteInDatabase();
        }
        
        
        [Test]
        public void update_changeNameOfCatalogue_passes()
        {
            //create a new one
            var cata = new Catalogue(Repository, "fishing");
            int expectedID = cata.ID;

            //find it and change it's name
            Catalogue[] catas = Repository.GetAllObjects<Catalogue>().ToArray();

            foreach (var catalogue in catas)
            {
                if (catalogue.ID == expectedID)
                {
                    catalogue.Name = "fish";
                    catalogue.SaveToDatabase();
                }
            }

            //find it again and see if it's name has changed - then delete it so we don't polute the db
            Catalogue[] catasAfter = Repository.GetAllObjects<Catalogue>().ToArray();

            foreach (var catalogue in catasAfter)
            {
                if (catalogue.ID == expectedID)
                {
                    Assert.AreEqual(catalogue.Name, "fish");
                    catalogue.DeleteInDatabase();
                }
            }
        }

        [Test]
        public void update_changeAllProperties_pass()
        {
            //create a new one
            var cata = new Catalogue(Repository, "fishing");
            int expectedID = cata.ID;

            //find it and change it's name
            Catalogue[] catas = Repository.GetAllObjects<Catalogue>().ToArray();

            foreach (var catalogue in catas)
            {
                if (catalogue.ID == expectedID)
                {
                    catalogue.Access_options = "backwards,frontwards";
                    catalogue.API_access_URL = new Uri("http://API.html");
                    catalogue.Acronym = "abc";
                    catalogue.Attribution_citation = "belongs to dave";
                    catalogue.Browse_URL = new Uri("http://browse.html");
                    catalogue.Bulk_Download_URL = new Uri("http://bulk.html");
                    catalogue.Contact_details = "thomasnind";
                    catalogue.Geographical_coverage = "fullspectrum";
                    catalogue.Resource_owner = "blackhole";
                    catalogue.Description = "exciting stuff of great excitement";
                    catalogue.Detail_Page_URL = new Uri("http://detail.html");
                    catalogue.Last_revision_date = DateTime.Parse("01/01/01");
                    catalogue.Name = "kaptainshield";
                    catalogue.Background_summary = "£50 preferred";
                    catalogue.Periodicity = Catalogue.CataloguePeriodicity.Monthly;
                    catalogue.Query_tool_URL = new Uri("http://querier.html");
                    catalogue.Source_URL =  new Uri("http://blackholeSun.html");
                    catalogue.Time_coverage = "comprehensive";
                    catalogue.Search_keywords = "excitement,fishmongery";
                    catalogue.Type = Catalogue.CatalogueType.ResearchStudy;
                    catalogue.Update_freq = "Every darmn second!";
                    catalogue.Update_sched = "periodically on request";

                    catalogue.Country_of_origin = "United Kingdom";
                    catalogue.Data_standards = "Highly Standardised";
                    catalogue.Administrative_contact_address = "Candyland";
                    catalogue.Administrative_contact_email = "big@brother.com";
                    catalogue.Administrative_contact_name = "Uncle Sam";
                    catalogue.Administrative_contact_telephone = "12345 67890";
                    catalogue.Explicit_consent = true;
                    catalogue.Ethics_approver = "Tayside Supernatural Department";
                    catalogue.Source_of_data_collection = "Invented by Unit Test";
                    catalogue.SubjectNumbers = "100,000,000";

                    catalogue.SaveToDatabase();
                }
            }



            //find it again and see if it has changed - then delete it so we don't polute the db
            Catalogue[] catasAfter = Repository.GetAllObjects<Catalogue>().ToArray();

            foreach (var catalogue in catasAfter)
            {
                if (catalogue.ID == expectedID)
                {
                    Assert.AreEqual(catalogue.Access_options , "backwards,frontwards");
                    Assert.AreEqual(catalogue.API_access_URL , new Uri("http://API.html"));
                    Assert.AreEqual(catalogue.Acronym , "abc");
                    Assert.AreEqual(catalogue.Attribution_citation , "belongs to dave");
                    Assert.AreEqual(catalogue.Browse_URL , new Uri("http://browse.html"));
                    Assert.AreEqual(catalogue.Bulk_Download_URL , new Uri("http://bulk.html"));
                    Assert.AreEqual(catalogue.Contact_details , "thomasnind");
                    Assert.AreEqual(catalogue.Geographical_coverage, "fullspectrum");
                    Assert.AreEqual(catalogue.Resource_owner, "blackhole");
                    Assert.AreEqual(catalogue.Description , "exciting stuff of great excitement");
                    Assert.AreEqual(catalogue.Detail_Page_URL , new Uri("http://detail.html"));
                    Assert.AreEqual(catalogue.Last_revision_date , DateTime.Parse("01/01/01"));
                    Assert.AreEqual(catalogue.Name , "kaptainshield");
                    Assert.AreEqual(catalogue.Background_summary, "£50 preferred");
                    Assert.AreEqual(catalogue.Periodicity , Catalogue.CataloguePeriodicity.Monthly);
                    Assert.AreEqual(catalogue.Query_tool_URL , new Uri("http://querier.html"));
                    Assert.AreEqual(catalogue.Source_URL , new Uri("http://blackholeSun.html"));
                    Assert.AreEqual(catalogue.Time_coverage , "comprehensive");
                    Assert.AreEqual(catalogue.Search_keywords, "excitement,fishmongery");
                    Assert.AreEqual(catalogue.Type , Catalogue.CatalogueType.ResearchStudy);
                    Assert.AreEqual(catalogue.Update_freq , "Every darmn second!");
                    Assert.AreEqual(catalogue.Update_sched , "periodically on request");


                    Assert.AreEqual(catalogue.Country_of_origin , "United Kingdom");
                    Assert.AreEqual(catalogue.Data_standards , "Highly Standardised");
                    Assert.AreEqual(catalogue.Administrative_contact_address , "Candyland");
                    Assert.AreEqual(catalogue.Administrative_contact_email , "big@brother.com");
                    Assert.AreEqual(catalogue.Administrative_contact_name , "Uncle Sam");
                    Assert.AreEqual(catalogue.Administrative_contact_telephone , "12345 67890");
                    Assert.AreEqual(catalogue.Explicit_consent , true);
                    Assert.AreEqual(catalogue.Ethics_approver , "Tayside Supernatural Department");
                    Assert.AreEqual(catalogue.Source_of_data_collection , "Invented by Unit Test");
                    Assert.AreEqual(catalogue.SubjectNumbers, "100,000,000");


                    catalogue.DeleteInDatabase();
                }
            }
        }

        [Test]
        public void create_blankConstructorCatalogue_createsNewInDatabase()
        {
            int before = Repository.GetAllObjects<Catalogue>().Count();

            var newCatalogue = new Catalogue(Repository, "fishing");
            int expectedID = newCatalogue.ID;

            Assert.IsTrue(expectedID > 1);


            Catalogue[] catasAfter = Repository.GetAllObjects<Catalogue>().ToArray();
            int after = catasAfter.Count();

            Assert.AreEqual(before, after - 1);

            int numberDeleted = 0;
            foreach (Catalogue cata in catasAfter)
            {
                if (cata.ID == expectedID)
                {
                    cata.DeleteInDatabase();
                    numberDeleted++;
                }
            }

            Assert.AreEqual(numberDeleted, 1);
        }

        [Test]
        public void GetCatalogueWithID_InvalidID_throwsException()
        {
            Assert.Throws<KeyNotFoundException>(() => Repository.GetObjectByID<Catalogue>(-1));
        }

        [Test]
        public void GetCatalogueWithID_validID_pass()
        {
            Catalogue c = new Catalogue(Repository, "TEST");

            Assert.NotNull(c);
            Assert.True(c.Name == "TEST");
            
            c.DeleteInDatabase();
        }


        [Test]
        public void TestGetTablesAndLookupTables()
        {
            //One catalogue
            Catalogue cata = new Catalogue(Repository, "TestGetTablesAndLookupTables");

            //6 virtual columns
            CatalogueItem ci1 = new CatalogueItem(Repository, cata, "Col1");
            CatalogueItem ci2 = new CatalogueItem(Repository, cata, "Col2");
            CatalogueItem ci3 = new CatalogueItem(Repository, cata, "Col3");
            CatalogueItem ci4 = new CatalogueItem(Repository, cata, "Col4");
            CatalogueItem ci5 = new CatalogueItem(Repository, cata, "Description");
            CatalogueItem ci6 = new CatalogueItem(Repository, cata, "Code");

            //2 columns come from table 1
            TableInfo t1 = new TableInfo(Repository, "Table1");
            ColumnInfo t1_c1 = new ColumnInfo(Repository, "Col1","varchar(10)",t1);
            ColumnInfo t1_c2 = new ColumnInfo(Repository, "Col2", "int", t1);

            //2 columns come from table 2
            TableInfo t2 = new TableInfo(Repository, "Table2");
            ColumnInfo t2_c1 = new ColumnInfo(Repository, "Col3", "varchar(10)", t2);
            ColumnInfo t2_c2 = new ColumnInfo(Repository, "Col4", "int", t2);

            //2 columns come from the lookup table
            TableInfo t3 = new TableInfo(Repository, "Table3");
            ColumnInfo t3_c1 = new ColumnInfo(Repository, "Description", "varchar(10)", t3);
            ColumnInfo t3_c2 = new ColumnInfo(Repository, "Code", "int", t3);

            //wire SetUp virtual columns to underlying columns
            ci1.SetColumnInfo(t1_c1);
            ci2.SetColumnInfo( t1_c2);
            ci3.SetColumnInfo( t2_c1);
            ci4.SetColumnInfo( t2_c2);
            ci5.SetColumnInfo( t3_c1);
            ci6.SetColumnInfo( t3_c2);

            //configure the lookup relationship
            var lookup = new Lookup(Repository, t3_c1, t1_c2, t3_c2,ExtractionJoinType.Left, "");
            try
            {
                var allTables = cata.GetTableInfoList(true).ToArray();
                Assert.Contains(t1,allTables);
                Assert.Contains(t2, allTables);
                Assert.Contains(t3, allTables);
            
                var normalTablesOnly = cata.GetTableInfoList(false).ToArray();
                Assert.AreEqual(2,normalTablesOnly.Length);
                Assert.Contains(t1,normalTablesOnly);
                Assert.Contains(t2, normalTablesOnly);

                var lookupTablesOnly = cata.GetLookupTableInfoList();
                Assert.AreEqual(1,lookupTablesOnly.Length);
                Assert.Contains(t3,lookupTablesOnly);

                List<ITableInfo> normalTables, lookupTables;
                cata.GetTableInfos(out normalTables, out lookupTables);
                Assert.AreEqual(2,normalTables.Count);
                Assert.AreEqual(1, lookupTables.Count);

                Assert.Contains(t1,normalTables);
                Assert.Contains(t2, normalTables);
                Assert.Contains(t3,lookupTables);
            }
            finally
            {
                lookup.DeleteInDatabase();
                
                t1.DeleteInDatabase();
                t2.DeleteInDatabase();
                t3.DeleteInDatabase();

                cata.DeleteInDatabase();
            }
        }

        [Test]
        public void CatalogueFolder_DefaultIsRoot()
        {
            var c = new Catalogue(Repository, "bob");
            try
            {
                Assert.AreEqual("\\",c.Folder);
            }
            finally
            {
                c.DeleteInDatabase();
            }
        }
        [Test]
        public void CatalogueFolder_ChangeAndSave()
        {
            var c = new Catalogue(Repository, "bob"); 
            try
            {
                c.Folder = "\\Research\\Important";
                Assert.AreEqual("\\research\\important\\", c.Folder);
                c.SaveToDatabase();

                var c2 = Repository.GetObjectByID<Catalogue>(c.ID);
                Assert.AreEqual("\\research\\important\\", c2.Folder);
            }
            finally
            {
                c.DeleteInDatabase();
            }
        }


        [Test]
        public void CatalogueFolder_CannotSetToNonRoot()
        {
            var c = new Catalogue(Repository, "bob");
            try
            {
                var ex = Assert.Throws<NotSupportedException>(()=>c.Folder = "fish");
                Assert.AreEqual(@"All catalogue paths must start with \.  Invalid path was:fish",ex.Message);
            }
            finally
            {
                c.DeleteInDatabase();
            }
        }

        [Test]
        public void CatalogueFolder_CannotSetToNull()
        {
            var c = new Catalogue(Repository, "bob");
            try
            {
                var ex = Assert.Throws<NotSupportedException>(()=>c.Folder = null);
                Assert.AreEqual(@"An attempt was made to set Catalogue Folder to null, every Catalogue must have a folder, set it to \ if you want the root", ex.Message);
            }
            finally
            {
                c.DeleteInDatabase();
            }
        }
        
        [Test]
        public void CatalogueFolder_CannotHaveDoubleSlashes()
        {
            var c = new Catalogue(Repository, "bob");
            try
            {
                //notice the @ symbol that makes the double slashes actual double slashes - common error we might make and what this test is designed to prevent
                var ex = Assert.Throws<NotSupportedException>(()=>c.Folder = @"\\bob\\");
                Assert.AreEqual(@"Catalogue paths cannot contain double slashes '\\', Invalid path was:\\bob\\", ex.Message);
            }
            finally
            {
                c.DeleteInDatabase();
            }
        }

        [Test]
        public void CatalogueFolder_Subfoldering()
        {
            var c1 = new Catalogue(Repository, "C1");
            var c2 = new Catalogue(Repository, "C2");

            try
            {
                c1.Folder = "\\Research\\";
                Assert.AreEqual("\\research\\", c1.Folder);
                c1.SaveToDatabase();

                c2.Folder = "\\Research\\Methodology";
                Assert.AreEqual( "\\research\\methodology\\",c2.Folder);

                c2.SaveToDatabase();

                Assert.IsTrue(CatalogueFolder.IsSubFolderOf(c2.Folder,c1.Folder));

            }
            finally
            {
                c1.DeleteInDatabase();
                c2.DeleteInDatabase();
            }
        }


        [Test]
        public void CatalogueFolder_SubfolderingDuplicateNames()
        {
            var c1 = new Catalogue(Repository, "C1");
            var c2 = new Catalogue(Repository, "C2");
            var c3 = new Catalogue(Repository, "C3");

            try
            {
                c1.Folder = @"\A\B\C\";
                c1.SaveToDatabase();

                c2.Folder = @"\B\C\";
                c2.SaveToDatabase();

                c3.Folder = @"\A\B\";
                c3.SaveToDatabase();
                
                //c1 is a subfolder of c3
                Assert.IsFalse(CatalogueFolder.IsSubFolderOf(c1.Folder,c2.Folder));
                Assert.IsTrue(CatalogueFolder.IsSubFolderOf(c1.Folder,c3.Folder));

                //c2 is nobodies subfolder
                Assert.IsFalse(CatalogueFolder.IsSubFolderOf(c2.Folder, c1.Folder));
                Assert.IsFalse(CatalogueFolder.IsSubFolderOf(c2.Folder, c3.Folder));

                //c2 is nobodies subfolder
                Assert.IsFalse(CatalogueFolder.IsSubFolderOf(c3.Folder,c1.Folder));
                Assert.IsFalse(CatalogueFolder.IsSubFolderOf(c3.Folder,c2.Folder));

            }
            finally
            {
                c1.DeleteInDatabase();
                c2.DeleteInDatabase();
            }
        }
        [Test]
        public void CatalogueFolder_SubfolderingAdvanced()
        {
            var c1 = new Catalogue(Repository, "C1");
            var c2 = new Catalogue(Repository, "C2");
            var c3 = new Catalogue(Repository, "C3");
            var c4 = new Catalogue(Repository, "C4");
            var c5 = new Catalogue(Repository, "C5");
            var c6 = new Catalogue(Repository, "C6");


            // 
            // Pass in 
            // CatalogueA - \2005\Research\
            // CatalogueB - \2006\Research\
            // 
            // This is Root (\)
            // Returns:
            //     \2005\ - empty 
            //     \2006\ - empty
            // 

            try
            {
                c1.Folder = @"\2005\Research\Current";
                c1.SaveToDatabase();

                c2.Folder = @"\2005\Research\Previous";
                c2.SaveToDatabase();


                c3.Folder = @"\2001\Research\Current";
                c3.SaveToDatabase();

                c4.Folder = @"\Homeland\Research\Current";
                c4.SaveToDatabase();
                
                c5.Folder = @"\Homeland\Research\Current";
                c5.SaveToDatabase();
                
                c6.Folder = @"\Homeland\Research\Current";
                c6.SaveToDatabase();

                var collection = new[] {c1, c2, c3, c4,c5,c6};

                var results = CatalogueFolder.GetImmediateSubFoldersUsing(CatalogueFolder.Root,collection);

                Assert.AreEqual(3,results.Length);
                string TwoThousandFive = results.Single(f => f.Equals(@"\2005\"));
                string TwoThousandOne = results.Single(f => f.Equals(@"\2001\"));
                string Homeland = results.Single(f => f.Equals(@"\homeland\"));
                
                Assert.AreEqual(1,CatalogueFolder.GetImmediateSubFoldersUsing(Homeland,collection).Length);
                Assert.AreEqual(1, CatalogueFolder.GetImmediateSubFoldersUsing(Homeland,collection).Count(f=>f.Equals(@"\homeland\research\")));

                Assert.AreEqual(1, CatalogueFolder.GetImmediateSubFoldersUsing(TwoThousandOne,collection).Length);
                Assert.AreEqual(1, CatalogueFolder.GetImmediateSubFoldersUsing(TwoThousandOne,collection).Count(f => f.Equals(@"\2001\research\")));

                var sub = CatalogueFolder.GetImmediateSubFoldersUsing(TwoThousandFive, collection).Single();

                string[] finalResult = CatalogueFolder.GetImmediateSubFoldersUsing(sub,collection);
                Assert.AreEqual(2, finalResult.Length);
                Assert.AreEqual(1, finalResult.Count(c => c.Equals(@"\2005\research\current\")));
                Assert.AreEqual(1, finalResult.Count(c => c.Equals(@"\2005\research\previous\")));

                Assert.AreEqual(0, CatalogueFolder.GetImmediateSubFoldersUsing(finalResult[0],collection).Length);
            }
            finally 
            {
                c1.DeleteInDatabase();
                c2.DeleteInDatabase();
                c3.DeleteInDatabase();
                c4.DeleteInDatabase();
                c5.DeleteInDatabase();
                c6.DeleteInDatabase();
                
            }
        }

        [Test]
        public void RelatedCatalogueTest_NoCatalogues()
        {
            TableInfo t = new TableInfo(Repository,"MyTable");
            try
            {
                Assert.AreEqual(0,t.GetAllRelatedCatalogues().Length);
            }
            finally
            {
                t.DeleteInDatabase();
            }
            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RelatedCatalogueTest_OneCatalogue(bool createExtractionInformation)
        {
            TableInfo t = new TableInfo(Repository, "MyTable");
            ColumnInfo c = new ColumnInfo(Repository,"MyCol","varchar(10)",t);
            
            Catalogue cata = new Catalogue(Repository,"MyCata");
            CatalogueItem ci = new CatalogueItem(Repository,cata,"MyCataItem");

            try
            {
                if (createExtractionInformation)
                    new ExtractionInformation(Repository, ci, c, "dbo.SomeFunc('Bob') as MySelectLine");
                else
                    ci.SetColumnInfo(c);

                var catas = t.GetAllRelatedCatalogues();
                Assert.AreEqual(1, catas.Length);
                Assert.AreEqual(cata,catas[0]);
            }
            finally
            {
                ci.DeleteInDatabase();
                cata.DeleteInDatabase();
                t.DeleteInDatabase();
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RelatedCatalogueTest_TwoCatalogues_TwoColumnsEach(bool createExtractionInformation)
        {
            TableInfo t = new TableInfo(Repository, "MyTable");
            ColumnInfo c1 = new ColumnInfo(Repository, "MyCol1", "varchar(10)", t);
            ColumnInfo c2 = new ColumnInfo(Repository, "MyCol2", "varchar(10)", t);
            
            Catalogue cata1 = new Catalogue(Repository, "cata1");
            CatalogueItem ci1_1 = new CatalogueItem(Repository, cata1, "MyCataItem1_1");
            CatalogueItem ci1_2 = new CatalogueItem(Repository, cata1, "MyCataItem1_2");

            Catalogue cata2 = new Catalogue(Repository, "cata2");
            CatalogueItem ci2_1 = new CatalogueItem(Repository, cata2, "MyCataItem2_1");
            CatalogueItem ci2_2 = new CatalogueItem(Repository, cata2, "MyCataItem2_2");
            try
            {
                if (createExtractionInformation)
                {
                    new ExtractionInformation(Repository, ci1_1, c1, "dbo.SomeFunc('Bob') as MySelectLine");
                    new ExtractionInformation(Repository, ci1_2, c2, "dbo.SomeFunc('Bob') as MySelectLine");
                    new ExtractionInformation(Repository, ci2_1, c2, "dbo.SomeFunc('Bob') as MySelectLine");
                    new ExtractionInformation(Repository, ci2_2, c1, "dbo.SomeFunc('Bob') as MySelectLine");
                }
                else
                {
                    ci1_1.SetColumnInfo(c1);
                    ci1_2.SetColumnInfo(c2);
                    ci2_1.SetColumnInfo(c2);
                    ci2_2.SetColumnInfo(c1);
                    
                }
                
                

                var catas = t.GetAllRelatedCatalogues();
                Assert.AreEqual(2, catas.Length);
                Assert.IsTrue(catas.Contains(cata1));
                Assert.IsTrue(catas.Contains(cata2));
            }
            finally
            {
                cata1.DeleteInDatabase();
                cata2.DeleteInDatabase();
                t.DeleteInDatabase();
            }

        }
    }
}
    