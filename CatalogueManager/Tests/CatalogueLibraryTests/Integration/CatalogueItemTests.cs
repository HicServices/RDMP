using System.Linq;
using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    class CatalogueItemTests : DatabaseTests
    {

        [Test]
        public void constructor_newTestCatalogueItem_pass()
        {

            var parent = new Catalogue(CatalogueRepository, "GROG");

            CatalogueItem child1 = new CatalogueItem(CatalogueRepository, parent, "GROG_ITEM1");
            CatalogueItem child2 = new CatalogueItem(CatalogueRepository, parent, "GROG_ITEM2");

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

            CatalogueItem child1 = new CatalogueItem(CatalogueRepository, parent, "GROG_ITEM1");
            child1.SetColumnInfo(null);
            
            Assert.IsNull(child1.ColumnInfo_ID);
            child1.DeleteInDatabase();
            parent.DeleteInDatabase();
        }

        [Test]
        public void GetAllCatalogueItemsForCatalogueID_NewCatalogue_pass()
        {
            Catalogue parent = new Catalogue(CatalogueRepository, "ZOMBIEMAN");

            var child1 = new CatalogueItem(CatalogueRepository, parent, "ZOMBIEMAN_ITEM1");
            var child2 = new CatalogueItem(CatalogueRepository, parent, "ZOMBIEMAN_ITEM2");

            CatalogueItem[] children = parent.CatalogueItems;

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
            Catalogue parent = new Catalogue(CatalogueRepository, "KONGOR");
            CatalogueItem child = new CatalogueItem(CatalogueRepository, parent, "KONGOR_SUPERKING")
            {
                Agg_method = "Adding up",
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
            
            CatalogueItem childAfter = CatalogueRepository.GetObjectByID<CatalogueItem>(child.ID);

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
            Catalogue parent = new Catalogue(CatalogueRepository,"KONGOR");
            Catalogue parent2 = new Catalogue(CatalogueRepository, "KONGOR2");

            CatalogueItem child = new CatalogueItem(CatalogueRepository, parent, "KONGOR_SUPERKING")
            {
                Agg_method = "Adding up",
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
                if (cloneChild != null)
                    cloneChild.DeleteInDatabase();

                child.DeleteInDatabase();
                parent.DeleteInDatabase();
                parent2.DeleteInDatabase();
                
            }
        }
    }
}
