// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

internal class CatalogueItemTests : DatabaseTests
{

    [Test]
    public void constructor_newTestCatalogueItem_pass()
    {

        var parent = new Catalogue(CatalogueRepository, "GROG");

        var child1 = new CatalogueItem(CatalogueRepository, parent, "GROG_ITEM1");
        var child2 = new CatalogueItem(CatalogueRepository, parent, "GROG_ITEM2");

        Assert.IsTrue(child1.Catalogue_ID == parent.ID);
        Assert.IsTrue(child2.Catalogue_ID == parent.ID);

        Assert.IsTrue(child1.ID != child2.ID);
            
        child1.DeleteInDatabase();
        child2.DeleteInDatabase();
        parent.DeleteInDatabase();
    }


    [Test]
    public void TestSettingColumnInfoToNull()
    {

        var parent = new Catalogue(CatalogueRepository, "GROG");

        var child1 = new CatalogueItem(CatalogueRepository, parent, "GROG_ITEM1");
        child1.SetColumnInfo(null);
            
        Assert.IsNull(child1.ColumnInfo_ID);
        child1.DeleteInDatabase();
        parent.DeleteInDatabase();
    }

    [Test]
    public void GetAllCatalogueItemsForCatalogueID_NewCatalogue_pass()
    {
        var parent = new Catalogue(CatalogueRepository, "ZOMBIEMAN");

        var child1 = new CatalogueItem(CatalogueRepository, parent, "ZOMBIEMAN_ITEM1");
        var child2 = new CatalogueItem(CatalogueRepository, parent, "ZOMBIEMAN_ITEM2");

        var children = parent.CatalogueItems;

        Assert.AreEqual(children.Length,2);
        Assert.IsTrue(children[0].ID == child1.ID || children[1].ID == child1.ID);
        Assert.IsTrue(children[0].ID == child2.ID || children[1].ID == child2.ID);
        Assert.IsTrue(children[0].ID != children[1].ID);

        children[0].DeleteInDatabase();
        children[1].DeleteInDatabase();
        parent.DeleteInDatabase();
    }

    [Test]
    public void update_changeAllPropertiesOfCatalogueItem_passes()
    {
        var parent = new Catalogue(CatalogueRepository, "KONGOR");
        var child = new CatalogueItem(CatalogueRepository, parent, "KONGOR_SUPERKING")
        {
            Agg_method = "Adding SetUp",
            Comments = "do not change amagad super secret!",
            Limitations = "Extreme limitaitons",
            Description =
                "Exciting things are going down in the streets of new your this time of year it would be a great idea if you were to go there",
            Name = "KONGOR_MINIMAN",
            Periodicity = Catalogue.CataloguePeriodicity.Monthly,
            Research_relevance = "Highly relevant to all fields of subatomic particle study",
            Statistical_cons = "Dangerous cons frequent the areas that this stats is happening, be afraid",
            Topic = "nothing much, lots of stuff"
        };

        child.SaveToDatabase();
            
        var childAfter = CatalogueRepository.GetObjectByID<CatalogueItem>(child.ID);

        Assert.IsTrue(child.Name == childAfter.Name);
        Assert.IsTrue(child.Agg_method == childAfter.Agg_method);
        Assert.IsTrue(child.Comments == childAfter.Comments);
        Assert.IsTrue(child.Limitations == childAfter.Limitations);
        Assert.IsTrue(child.Description == childAfter.Description);
        Assert.IsTrue(child.Periodicity == childAfter.Periodicity);
        Assert.IsTrue(child.Research_relevance == childAfter.Research_relevance);
        Assert.IsTrue(child.Statistical_cons == childAfter.Statistical_cons);
        Assert.IsTrue(child.Topic == childAfter.Topic);

        child.DeleteInDatabase();
        parent.DeleteInDatabase();
    }

    [Test]
    public void clone_CloneCatalogueItemWithIDIntoCatalogue_passes()
    {
        var parent = new Catalogue(CatalogueRepository,"KONGOR");
        var parent2 = new Catalogue(CatalogueRepository, "KONGOR2");

        var child = new CatalogueItem(CatalogueRepository, parent, "KONGOR_SUPERKING")
        {
            Agg_method = "Adding SetUp",
            Comments = "do not change amagad super secret!",
            Limitations = "Extreme limitaitons",
            Description =
                "Exciting things are going down in the streets of new your this time of year it would be a great idea if you were to go there",
            Name = "KONGOR_MINIMAN",
            Periodicity = Catalogue.CataloguePeriodicity.Monthly,
            Research_relevance = "Highly relevant to all fields of subatomic particle study",
            Statistical_cons = "Dangerous cons frequent the areas that this stats is happening, be afraid",
            Topic = "nothing much, lots of stuff"
        };

        CatalogueItem cloneChild = null;
        try
        {
            child.SaveToDatabase();
            cloneChild = child.CloneCatalogueItemWithIDIntoCatalogue(parent2);

            //get the clone that was returned
            Assert.AreEqual(cloneChild.Catalogue_ID, parent2.ID); //it is in the second one
            Assert.AreNotEqual(cloneChild.Catalogue_ID, parent.ID); //it is not in the first one
            Assert.AreNotEqual(cloneChild.ID, child.ID); //it has a new ID

            Assert.AreEqual(cloneChild.Limitations, child.Limitations);
            Assert.AreEqual(cloneChild.Description, child.Description);
            Assert.AreEqual(cloneChild.Name, child.Name);
            Assert.AreEqual(cloneChild.Periodicity, child.Periodicity);
            Assert.AreEqual(cloneChild.Research_relevance, child.Research_relevance);
            Assert.AreEqual(cloneChild.Statistical_cons, child.Statistical_cons);
            Assert.AreEqual(cloneChild.Topic, child.Topic);
        }
        finally
        {
            cloneChild?.DeleteInDatabase();

            child.DeleteInDatabase();
            parent.DeleteInDatabase();
            parent2.DeleteInDatabase();

        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestDeleting_CascadesToExtractionInformations(bool makeOrphanFirst)
    {
        var c = new Catalogue(CatalogueRepository,"My new cata");
        var ci = new CatalogueItem(CatalogueRepository, c, "myci");

        var t = new TableInfo(CatalogueRepository, "myt");
        var col = new ColumnInfo(CatalogueRepository, "mycol", "varchar(10)", t);

        var ei = new ExtractionInformation(CatalogueRepository, ci, col,"fff");

        if(makeOrphanFirst)
        {
            col.DeleteInDatabase();
        }

        c.DeleteInDatabase();

        Assert.IsFalse(c.Exists());
        Assert.IsFalse(ci.Exists());
        Assert.IsFalse(ei.Exists());

        Assert.IsTrue(t.Exists());
        Assert.AreEqual(!makeOrphanFirst,col.Exists());

    }
}