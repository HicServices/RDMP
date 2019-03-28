using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using NUnit.Framework;
using ReusableLibraryCode.Checks;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class ChildProviderTests : UITests
    {
        [Test]
        public void ChildProviderGiven_TableInfoWith_NullServer()
        {
            var ti = WhenIHaveA<TableInfo>();
            ti.Server = null;
            ti.SaveToDatabase();

            //creating a child provider when there are TableInfos with null servers should not crash the API!
            var provider = new CatalogueChildProvider(Repository.CatalogueRepository, null, new ThrowImmediatelyCheckNotifier());
            var desc = provider.GetDescendancyListIfAnyFor(ti);
            Assert.IsNotNull(desc);

            //instead we should get a parent node with the name "Null Server"
            var parent = (TableInfoServerNode) desc.Parents[desc.Parents.Length - 1];
            Assert.AreEqual(TableInfoServerNode.NullServerNode, parent.ServerName);
        }
    }
}
