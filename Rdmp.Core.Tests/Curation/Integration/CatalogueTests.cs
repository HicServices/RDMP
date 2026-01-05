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

namespace Rdmp.Core.Tests.Curation.Integration;

public class CatalogueTests : UnitTests
{
    [Test]
    public void Test_GetObjects_Catalogue()
    {
        var catalogueWithId = new Catalogue(Repository, "bob");
        var catas = Repository.GetAllObjects<Catalogue>();

        Assert.That(catas, Is.Not.Empty);

        catalogueWithId.DeleteInDatabase();
    }

    [Test]
    public void SettingPropertyViaRelationshipDoesntSave_NoticeHowYouHaveToCacheThePropertyCatalogueToSetIt()
    {
        var c = new Catalogue(Repository, "frank");
        var ci = new CatalogueItem(Repository, c, "bob");


        var cata = ci.Catalogue;
        cata.Name = "fish2";
        cata.SaveToDatabase();
        Assert.That(ci.Catalogue.Name, Is.EqualTo("fish2"));

        //now thanks to lazy this works... but it's ambiguous (it works if the property is referenced via IInjectKnown<T>)
        ci.Catalogue.Name = "fish";
        ci.Catalogue.SaveToDatabase();
        Assert.That(ci.Catalogue.Name, Is.EqualTo("fish"));

        c.DeleteInDatabase();
    }


    [Test]
    public void update_changeNameOfCatalogue_passes()
    {
        //create a new one
        var cata = new Catalogue(Repository, "fishing");
        var expectedID = cata.ID;

        //find it and change its name
        var catas = Repository.GetAllObjects<Catalogue>().ToArray();

        foreach (var catalogue in catas)
            if (catalogue.ID == expectedID)
            {
                catalogue.Name = "fish";
                catalogue.SaveToDatabase();
            }

        //find it again and see if its name has changed - then delete it so we don't pollute the db
        var catasAfter = Repository.GetAllObjects<Catalogue>().ToArray();

        foreach (var catalogue in catasAfter)
            if (catalogue.ID == expectedID)
            {
                Assert.That(catalogue.Name, Is.EqualTo("fish"));
                catalogue.DeleteInDatabase();
            }
    }

    [Test]
    public void update_changeAllProperties_pass()
    {
        //create a new one
        var cata = new Catalogue(Repository, "fishing");
        var expectedID = cata.ID;

        //find it and change its name
        var catas = Repository.GetAllObjects<Catalogue>().ToArray();

        foreach (var catalogue in catas)
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
                catalogue.Source_URL = new Uri("http://blackholeSun.html");
                catalogue.Time_coverage = "comprehensive";
                catalogue.Search_keywords = "excitement,fishmongery";
                catalogue.Type = Catalogue.CatalogueType.ResearchStudy;
                catalogue.Update_freq =Catalogue.UpdateFrequencies.Daily;
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


        //find it again and see if it has changed - then delete it so we don't pollute the db
        var catasAfter = Repository.GetAllObjects<Catalogue>().ToArray();

        foreach (var catalogue in catasAfter)
            if (catalogue.ID == expectedID)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(catalogue.Access_options, Is.EqualTo("backwards,frontwards"));
                    Assert.That(catalogue.API_access_URL, Is.EqualTo(new Uri("http://API.html")));
                    Assert.That(catalogue.Acronym, Is.EqualTo("abc"));
                    Assert.That(catalogue.Attribution_citation, Is.EqualTo("belongs to dave"));
                    Assert.That(catalogue.Browse_URL, Is.EqualTo(new Uri("http://browse.html")));
                    Assert.That(catalogue.Bulk_Download_URL, Is.EqualTo(new Uri("http://bulk.html")));
                    Assert.That(catalogue.Contact_details, Is.EqualTo("thomasnind"));
                    Assert.That(catalogue.Geographical_coverage, Is.EqualTo("fullspectrum"));
                    Assert.That(catalogue.Resource_owner, Is.EqualTo("blackhole"));
                    Assert.That(catalogue.Description, Is.EqualTo("exciting stuff of great excitement"));
                    Assert.That(catalogue.Detail_Page_URL, Is.EqualTo(new Uri("http://detail.html")));
                    Assert.That(catalogue.Last_revision_date, Is.EqualTo(DateTime.Parse("01/01/01")));
                    Assert.That(catalogue.Name, Is.EqualTo("kaptainshield"));
                    Assert.That(catalogue.Background_summary, Is.EqualTo("£50 preferred"));
                    Assert.That(catalogue.Periodicity, Is.EqualTo(Catalogue.CataloguePeriodicity.Monthly));
                    Assert.That(catalogue.Query_tool_URL, Is.EqualTo(new Uri("http://querier.html")));
                    Assert.That(catalogue.Source_URL, Is.EqualTo(new Uri("http://blackholeSun.html")));
                    Assert.That(catalogue.Time_coverage, Is.EqualTo("comprehensive"));
                    Assert.That(catalogue.Search_keywords, Is.EqualTo("excitement,fishmongery"));
                    Assert.That(catalogue.Type, Is.EqualTo(Catalogue.CatalogueType.ResearchStudy));
                    Assert.That(catalogue.Update_freq, Is.EqualTo(Catalogue.UpdateFrequencies.Daily));
                    Assert.That(catalogue.Update_sched, Is.EqualTo("periodically on request"));


                    Assert.That(catalogue.Country_of_origin, Is.EqualTo("United Kingdom"));
                    Assert.That(catalogue.Data_standards, Is.EqualTo("Highly Standardised"));
                    Assert.That(catalogue.Administrative_contact_address, Is.EqualTo("Candyland"));
                    Assert.That(catalogue.Administrative_contact_email, Is.EqualTo("big@brother.com"));
                    Assert.That(catalogue.Administrative_contact_name, Is.EqualTo("Uncle Sam"));
                    Assert.That(catalogue.Administrative_contact_telephone, Is.EqualTo("12345 67890"));
                    Assert.That(catalogue.Explicit_consent, Is.EqualTo(true));
                    Assert.That(catalogue.Ethics_approver, Is.EqualTo("Tayside Supernatural Department"));
                    Assert.That(catalogue.Source_of_data_collection, Is.EqualTo("Invented by Unit Test"));
                    Assert.That(catalogue.SubjectNumbers, Is.EqualTo("100,000,000"));
                });


                catalogue.DeleteInDatabase();
            }
    }

    [Test]
    public void create_blankConstructorCatalogue_createsNewInDatabase()
    {
        var before = Repository.GetAllObjects<Catalogue>().Length;

        var newCatalogue = new Catalogue(Repository, "fishing");
        var expectedID = newCatalogue.ID;

        Assert.That(expectedID, Is.GreaterThan(1));


        var catasAfter = Repository.GetAllObjects<Catalogue>().ToArray();
        var after = catasAfter.Length;

        Assert.That(after - 1, Is.EqualTo(before));

        var numberDeleted = 0;
        foreach (var cata in catasAfter)
            if (cata.ID == expectedID)
            {
                cata.DeleteInDatabase();
                numberDeleted++;
            }

        Assert.That(numberDeleted, Is.EqualTo(1));
    }

    [Test]
    public void GetCatalogueWithID_InvalidID_throwsException()
    {
        Assert.Throws<KeyNotFoundException>(() => Repository.GetObjectByID<Catalogue>(-1));
    }

    [Test]
    public void GetCatalogueWithID_validID_pass()
    {
        var c = new Catalogue(Repository, "TEST");

        Assert.That(c, Is.Not.Null);
        Assert.That(c.Name, Is.EqualTo("TEST"));

        c.DeleteInDatabase();
    }


    [Test]
    public void TestGetTablesAndLookupTables()
    {
        //One catalogue
        var cata = new Catalogue(Repository, "TestGetTablesAndLookupTables");

        //6 virtual columns
        var ci1 = new CatalogueItem(Repository, cata, "Col1");
        var ci2 = new CatalogueItem(Repository, cata, "Col2");
        var ci3 = new CatalogueItem(Repository, cata, "Col3");
        var ci4 = new CatalogueItem(Repository, cata, "Col4");
        var ci5 = new CatalogueItem(Repository, cata, "Description");
        var ci6 = new CatalogueItem(Repository, cata, "Code");

        //2 columns come from table 1
        var t1 = new TableInfo(Repository, "Table1");
        var t1_c1 = new ColumnInfo(Repository, "Col1", "varchar(10)", t1);
        var t1_c2 = new ColumnInfo(Repository, "Col2", "int", t1);

        //2 columns come from table 2
        var t2 = new TableInfo(Repository, "Table2");
        var t2_c1 = new ColumnInfo(Repository, "Col3", "varchar(10)", t2);
        var t2_c2 = new ColumnInfo(Repository, "Col4", "int", t2);

        //2 columns come from the lookup table
        var t3 = new TableInfo(Repository, "Table3");
        var t3_c1 = new ColumnInfo(Repository, "Description", "varchar(10)", t3);
        var t3_c2 = new ColumnInfo(Repository, "Code", "int", t3);

        //wire SetUp virtual columns to underlying columns
        ci1.SetColumnInfo(t1_c1);
        ci2.SetColumnInfo(t1_c2);
        ci3.SetColumnInfo(t2_c1);
        ci4.SetColumnInfo(t2_c2);
        ci5.SetColumnInfo(t3_c1);
        ci6.SetColumnInfo(t3_c2);

        //configure the lookup relationship
        var lookup = new Lookup(Repository, t3_c1, t1_c2, t3_c2, ExtractionJoinType.Left, "");
        try
        {
            var allTables = cata.GetTableInfoList(true).ToArray();
            Assert.That(allTables, Does.Contain(t1));
            Assert.That(allTables, Does.Contain(t2));
            Assert.That(allTables, Does.Contain(t3));

            var normalTablesOnly = cata.GetTableInfoList(false).ToArray();
            Assert.That(normalTablesOnly, Has.Length.EqualTo(2));
            Assert.That(normalTablesOnly, Does.Contain(t1));
            Assert.That(normalTablesOnly, Does.Contain(t2));

            var lookupTablesOnly = cata.GetLookupTableInfoList();
            Assert.That(lookupTablesOnly, Has.Length.EqualTo(1));
            Assert.That(lookupTablesOnly, Does.Contain(t3));

            cata.GetTableInfos(out var normalTables, out var lookupTables);
            Assert.Multiple(() =>
            {
                Assert.That(normalTables, Has.Count.EqualTo(2));
                Assert.That(lookupTables, Has.Count.EqualTo(1));
            });

            Assert.That(normalTables, Does.Contain(t1));
            Assert.Multiple(() =>
            {
                Assert.That(normalTables, Does.Contain(t2));
                Assert.That(lookupTables, Does.Contain(t3));
            });
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
            Assert.That(c.Folder, Is.EqualTo("\\"));
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
            Assert.That(c.Folder, Is.EqualTo("\\research\\important"));
            c.SaveToDatabase();

            var c2 = Repository.GetObjectByID<Catalogue>(c.ID);
            Assert.That(c2.Folder, Is.EqualTo("\\research\\important"));
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
            var ex = Assert.Throws<NotSupportedException>(() => c.Folder = "fish");
            Assert.That(ex.Message, Is.EqualTo(@"All catalogue paths must start with \.  Invalid path was:fish"));
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
            var ex = Assert.Throws<NotSupportedException>(() => c.Folder = null);
            Assert.That(
                ex.Message, Is.EqualTo(@"An attempt was made to set Catalogue Folder to null, every Catalogue must have a folder, set it to \ if you want the root"));
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
            var ex = Assert.Throws<NotSupportedException>(() => c.Folder = @"\\bob\\");
            Assert.That(ex.Message, Is.EqualTo(@"Catalogue paths cannot contain double slashes '\\', Invalid path was:\\bob\\"));
        }
        finally
        {
            c.DeleteInDatabase();
        }
    }

    [Test]
    public void RelatedCatalogueTest_NoCatalogues()
    {
        var t = new TableInfo(Repository, "MyTable");
        try
        {
            Assert.That(t.GetAllRelatedCatalogues(), Is.Empty);
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
        var t = new TableInfo(Repository, "MyTable");
        var c = new ColumnInfo(Repository, "MyCol", "varchar(10)", t);

        var cata = new Catalogue(Repository, "MyCata");
        var ci = new CatalogueItem(Repository, cata, "MyCataItem");

        try
        {
            if (createExtractionInformation)
                new ExtractionInformation(Repository, ci, c, "dbo.SomeFunc('Bob') as MySelectLine");
            else
                ci.SetColumnInfo(c);

            var catas = t.GetAllRelatedCatalogues();
            Assert.That(catas, Has.Length.EqualTo(1));
            Assert.That(catas[0], Is.EqualTo(cata));
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
        var t = new TableInfo(Repository, "MyTable");
        var c1 = new ColumnInfo(Repository, "MyCol1", "varchar(10)", t);
        var c2 = new ColumnInfo(Repository, "MyCol2", "varchar(10)", t);

        var cata1 = new Catalogue(Repository, "cata1");
        var ci1_1 = new CatalogueItem(Repository, cata1, "MyCataItem1_1");
        var ci1_2 = new CatalogueItem(Repository, cata1, "MyCataItem1_2");

        var cata2 = new Catalogue(Repository, "cata2");
        var ci2_1 = new CatalogueItem(Repository, cata2, "MyCataItem2_1");
        var ci2_2 = new CatalogueItem(Repository, cata2, "MyCataItem2_2");
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
            Assert.That(catas, Has.Length.EqualTo(2));
            Assert.That(catas, Does.Contain(cata1));
            Assert.That(catas, Does.Contain(cata2));
        }
        finally
        {
            cata1.DeleteInDatabase();
            cata2.DeleteInDatabase();
            t.DeleteInDatabase();
        }
    }


    [TestCase("\\", "\\")]
    [TestCase("\\fish", "fish")]
    [TestCase("\\fish\\dog\\cat", "cat")]
    public void TestTreeNode_FullName_CleanPaths(string fullName, string expectedName)
    {
        var r1 = WhenIHaveA<Catalogue>();
        r1.Folder = fullName;

        var tree = FolderHelper.BuildFolderTree(new[] { r1 });

        var bottomFolder = tree;

        while (bottomFolder.ChildFolders.Any()) bottomFolder = bottomFolder.ChildFolders.Single();

        Assert.Multiple(() =>
        {
            Assert.That(bottomFolder.Name, Is.EqualTo(expectedName));
            Assert.That(bottomFolder.FullName, Is.EqualTo(fullName));
        });
    }

    [TestCase("\\admissions\\", "\\admissions")]
    [TestCase("\\ADMissions\\", "\\admissions")]
    public void TestFolderHelperAdjust(string input, string expectedOutput)
    {
        Assert.That(FolderHelper.Adjust(input), Is.EqualTo(expectedOutput));
    }

    [Test]
    public void TestBuildFolderTree()
    {
        var r1 = WhenIHaveA<Catalogue>();
        r1.Folder = "\\";

        var r2 = WhenIHaveA<Catalogue>();
        r2.Folder = "\\";

        var cat = WhenIHaveA<Catalogue>();
        cat.Folder = "\\dog\\fish\\cat";

        // give it some malformed ones too
        var fun = WhenIHaveA<Catalogue>();
        fun.Folder = "\\fun";

        var morefun = WhenIHaveA<Catalogue>();
        morefun.Folder = "\\fun";

        var objects = new IHasFolder[]
        {
            r1, r2, cat, fun, morefun
        };


        var tree = FolderHelper.BuildFolderTree(objects);
        Assert.That(tree.ChildObjects, Does.Contain(r1));
        Assert.Multiple(() =>
        {
            Assert.That(tree.ChildObjects, Does.Contain(r2));

            Assert.That(tree["dog"]["fish"]["cat"].ChildObjects, Does.Contain(cat));

            Assert.That(tree["fun"].ChildObjects, Does.Contain(fun));
        });
        Assert.That(tree["fun"].ChildObjects, Does.Contain(morefun));
    }


    /// <summary>
    /// Tests when you have
    /// \
    /// \ somefolder
    ///   +cata1
    ///   \ somesub
    ///     +cata2
    /// </summary>
    [Test]
    public void TestBuildFolderTree_MiddleBranches()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        cata1.Folder = "\\somefolder";

        var cata2 = WhenIHaveA<Catalogue>();
        cata2.Folder = "\\somefolder\\somesub";

        var objects = new IHasFolder[]
        {
            cata1, cata2
        };

        var tree = FolderHelper.BuildFolderTree(objects);
        Assert.Multiple(() =>
        {
            Assert.That(tree.ChildObjects, Is.Empty, "Should be no Catalogues on the root");

            Assert.That(tree.ChildFolders, Has.Count.EqualTo(1));
            Assert.That(tree["somefolder"].ChildFolders, Has.Count.EqualTo(1));
            Assert.That(tree["somefolder"]["somesub"].ChildFolders, Is.Empty);

            Assert.That(tree["somefolder"].ChildObjects, Does.Contain(cata1));
            Assert.That(tree["somefolder"]["somesub"].ChildObjects, Does.Contain(cata2));
        });
    }
}