using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class SupportingDocumentTests : DatabaseTests
    {
        [Test]
        public void test_SupportingDocument_CreateAndDestroy()
        {
            Catalogue cata = new Catalogue(CatalogueRepository, "deleteme");
            SupportingDocument doc = new SupportingDocument(CatalogueRepository, cata,"davesFile");

            Assert.AreEqual(doc.Name ,"davesFile");

            doc.DeleteInDatabase();
            cata.DeleteInDatabase();
        }

        [Test]
        public void test_SupportingDocument_CreateChangeSaveDestroy()
        {
            Catalogue cata = new Catalogue(CatalogueRepository, "deleteme");
            SupportingDocument doc = new SupportingDocument(CatalogueRepository, cata, "davesFile");
            doc.Description = "some exciting file that dave loves";
            doc.SaveToDatabase();

            Assert.AreEqual(doc.Name, "davesFile");
            Assert.AreEqual(doc.Description, "some exciting file that dave loves");

            SupportingDocument docAfterCommit = CatalogueRepository.GetObjectByID<SupportingDocument>(doc.ID);

            Assert.AreEqual(docAfterCommit.Description,doc.Description);

            doc.DeleteInDatabase();
            cata.DeleteInDatabase();
        }
    }
}
