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

        Assert.Multiple(() =>
        {
            Assert.That(child1.Catalogue_ID, Is.EqualTo(parent.ID));
            Assert.That(child2.Catalogue_ID, Is.EqualTo(parent.ID));

            Assert.That(child1.ID, Is.Not.EqualTo(child2.ID));
        });

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

        Assert.That(child1.ColumnInfo_ID, Is.Null);
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

        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].ID == child1.ID || children[1].ID == child1.ID);
            Assert.That(children[0].ID == child2.ID || children[1].ID == child2.ID);
            Assert.That(children[0].ID, Is.Not.EqualTo(children[1].ID));
        });

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
            Limitations = "Extreme limitations",
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

        Assert.Multiple(() =>
        {
            Assert.That(child.Name, Is.EqualTo(childAfter.Name));
            Assert.That(child.Agg_method, Is.EqualTo(childAfter.Agg_method));
            Assert.That(child.Comments, Is.EqualTo(childAfter.Comments));
            Assert.That(child.Limitations, Is.EqualTo(childAfter.Limitations));
            Assert.That(child.Description, Is.EqualTo(childAfter.Description));
            Assert.That(child.Periodicity, Is.EqualTo(childAfter.Periodicity));
            Assert.That(child.Research_relevance, Is.EqualTo(childAfter.Research_relevance));
            Assert.That(child.Statistical_cons, Is.EqualTo(childAfter.Statistical_cons));
            Assert.That(child.Topic, Is.EqualTo(childAfter.Topic));
        });

        child.DeleteInDatabase();
        parent.DeleteInDatabase();
    }

    [Test]
    public void clone_CloneCatalogueItemWithIDIntoCatalogue_passes()
    {
        var parent = new Catalogue(CatalogueRepository, "KONGOR");
        var parent2 = new Catalogue(CatalogueRepository, "KONGOR2");

        var child = new CatalogueItem(CatalogueRepository, parent, "KONGOR_SUPERKING")
        {
            Agg_method = "Adding SetUp",
            Comments = "do not change amagad super secret!",
            Limitations = "Extreme limitations",
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

            Assert.Multiple(() =>
            {
                //get the clone that was returned
                Assert.That(parent2.ID, Is.EqualTo(cloneChild.Catalogue_ID)); //it is in the second one
                Assert.That(parent.ID, Is.Not.EqualTo(cloneChild.Catalogue_ID)); //it is not in the first one
                Assert.That(child.ID, Is.Not.EqualTo(cloneChild.ID)); //it has a new ID

                Assert.That(child.Limitations, Is.EqualTo(cloneChild.Limitations));
                Assert.That(child.Description, Is.EqualTo(cloneChild.Description));
                Assert.That(child.Name, Is.EqualTo(cloneChild.Name));
                Assert.That(child.Periodicity, Is.EqualTo(cloneChild.Periodicity));
                Assert.That(child.Research_relevance, Is.EqualTo(cloneChild.Research_relevance));
                Assert.That(child.Statistical_cons, Is.EqualTo(cloneChild.Statistical_cons));
                Assert.That(child.Topic, Is.EqualTo(cloneChild.Topic));
            });
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
        var c = new Catalogue(CatalogueRepository, "My new cata");
        var ci = new CatalogueItem(CatalogueRepository, c, "myci");

        var t = new TableInfo(CatalogueRepository, "myt");
        var col = new ColumnInfo(CatalogueRepository, "mycol", "varchar(10)", t);

        var ei = new ExtractionInformation(CatalogueRepository, ci, col, "fff");

        if (makeOrphanFirst) col.DeleteInDatabase();

        c.DeleteInDatabase();

        Assert.Multiple(() =>
        {
            Assert.That(c.Exists(), Is.False);
            Assert.That(ci.Exists(), Is.False);
            Assert.That(ei.Exists(), Is.False);

            Assert.That(t.Exists());
            Assert.That(col.Exists(), Is.EqualTo(!makeOrphanFirst));
        });
    }
}