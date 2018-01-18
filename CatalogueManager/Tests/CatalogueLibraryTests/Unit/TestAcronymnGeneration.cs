using CatalogueLibrary;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Unit
{
    public class TestAcronymGeneration : DatabaseTests
    {

        [Test]
        [TestCase("bob","bob")]
        [TestCase("Demography", "Demog")]
        [TestCase("DMPCatalogue", "DMPC")]
        [TestCase("Datasheet1", "D1")]
        [TestCase("Frank Bettie Cardinality", "FBC")]
        [TestCase("Datashet DMP 32", "DDMP32")]
        public void Predict(string name, string predictedAcronym)
        {
            DitaCatalogueExtractor extractor = new DitaCatalogueExtractor(CatalogueRepository, null);

            string suggestion = extractor.GetAcronymSuggestionFromCatalogueName(name);

            Assert.AreEqual(predictedAcronym,suggestion);
        }
    }
}
