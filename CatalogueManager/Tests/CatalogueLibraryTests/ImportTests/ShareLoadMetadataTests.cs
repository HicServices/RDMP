using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Repositories;
using NUnit.Framework;
using Sharing.Dependency.Gathering;
using Tests.Common;

namespace CatalogueLibraryTests.ImportTests
{
    public class ShareLoadMetadataTests : UnitTests
    {
        [Test]
        public void GatherAndShare_LoadMetadata_EmptyLoadMetadata()
        {
            //create an object
            LoadMetadata lmd = new LoadMetadata(Repository, "MyLmd");

            var lmd2 = ShareToNewRepository(lmd);
            
            //different repos so not identical
            Assert.IsFalse(ReferenceEquals(lmd,lmd2));
            Assert.AreEqual(lmd.Name, lmd2.Name);
        }

        [Test]
        public void GatherAndShare_LoadMetadata_WithCatalogue()
        {
            //create an object
            LoadMetadata lmd1;
            var lmd2 = ShareToNewRepository(lmd1 = WhenIHaveA<LoadMetadata>());
            
            var cata1 = lmd1.GetAllCatalogues().Single();
            var cata2 = lmd2.GetAllCatalogues().Single();

            //different repos so not identical
            Assert.IsFalse(ReferenceEquals(lmd1,lmd2));
            Assert.IsFalse(ReferenceEquals(cata1,cata2));

            Assert.AreEqual(lmd1.Name, lmd2.Name);
            Assert.AreEqual(cata1.Name, cata2.Name);
        }

        private LoadMetadata ShareToNewRepository(LoadMetadata lmd)
        {
            var gatherer = new Gatherer(RepositoryLocator);
            
            Assert.IsTrue(gatherer.CanGatherDependencies(lmd));
            var rootObj = gatherer.GatherDependencies(lmd);

            var sm = new ShareManager(RepositoryLocator,null);
            var shareDefinition = rootObj.ToShareDefinitionWithChildren(sm);

            var repo2 = new MemoryDataExportRepository();
            var sm2  = new ShareManager(new RepositoryProvider(repo2));
            return sm2.ImportSharedObject(shareDefinition).OfType<LoadMetadata>().Single();
        }
    }
}