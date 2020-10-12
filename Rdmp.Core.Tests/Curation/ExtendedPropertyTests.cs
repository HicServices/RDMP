using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation
{
    class ExtendedPropertyTests : DatabaseTests
    {
        [Test]
        public void ExtendedProperty_Catalogue()
        {
            var cata = new Catalogue(CatalogueRepository,"My cata");
            var prop = new ExtendedProperty(CatalogueRepository,cata,"Fish",5);

            Assert.AreEqual(5,prop.GetValueAsSystemType());
            Assert.IsTrue(prop.IsReferenceTo(cata));

            prop.SetValue(10);
            prop.SaveToDatabase();
            
            Assert.AreEqual(10,prop.GetValueAsSystemType());
            Assert.IsTrue(prop.IsReferenceTo(cata));
        }
    }
}
