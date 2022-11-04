using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests
{
    public class CatalogueTests : UnitTests
    {
        [Test]
        public void Test_GetObjects_Catalogue()
        {
            CatalogueItem ci = WhenIHaveA<CatalogueItem>();

            ci.Name = "fish";

            Assert.AreEqual("fish", ci.Name);
            ci.RevertToDatabaseState();
            Assert.AreNotEqual("fish", ci.Name);
        }
    }
    
}
