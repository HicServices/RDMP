using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using CatalogueLibraryTests.Mocks;
using DataExportLibrary.Repositories;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using LoadModules.Generic.Attachers;
using NUnit.Framework;
using Rhino.Mocks;
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
        
        /// <summary>
        /// Tests sharing a basic process task load metadata
        /// </summary>
        [Test]
        public void GatherAndShare_LoadMetadata_WithProcessTasks1()
        {
            //create an object
            LoadMetadata lmd1;
            var lmd2 = ShareToNewRepository(lmd1=WhenIHaveA<ProcessTaskArgument>().ProcessTask.LoadMetadata);
            
            var pt1 = lmd1.ProcessTasks.Single();
            var pt2 = lmd2.ProcessTasks.Single();
            
            //different repos so not identical
            Assert.IsFalse(ReferenceEquals(lmd1,lmd2));
            AssertAreEqual(lmd1,lmd2);
            
            Assert.IsFalse(ReferenceEquals(pt1,pt2));
            AssertAreEqual(pt1,pt2);
            
            Assert.IsFalse(ReferenceEquals(pt1.ProcessTaskArguments.Single(),pt2.ProcessTaskArguments.Single()));
            AssertAreEqual(pt1.ProcessTaskArguments.Single(),pt2.ProcessTaskArguments.Single());
        }

        /// <summary>
        /// Tests sharing a more advanced loadmetadata with an actual class behind the ProcessTask
        /// </summary>
        [Test]
        public void GatherAndShare_LoadMetadata_WithProcessTasks2()
        {
            //create an object
            LoadMetadata lmd1 = WhenIHaveA<LoadMetadata>();

            SetupMEF();
            
            var pt1 = new ProcessTask(Repository, lmd1, LoadStage.Mounting);
            pt1.ProcessTaskType = ProcessTaskType.Attacher;
            pt1.LoadStage = LoadStage.Mounting;
            pt1.Path = typeof(AnySeparatorFileAttacher).FullName;
            pt1.SaveToDatabase();

            pt1.CreateArgumentsForClassIfNotExists(typeof(AnySeparatorFileAttacher));
            var pta = pt1.ProcessTaskArguments.Single(pt => pt.Name == "Separator");
            pta.SetValue(",");
            pta.SaveToDatabase();


            var lmd2 = ShareToNewRepository(lmd1);
            
            //different repos so not identical
            Assert.IsFalse(ReferenceEquals(lmd1,lmd2));
            AssertAreEqual(lmd1,lmd2);

            var pt2 = lmd2.ProcessTasks.Single();

            Assert.IsFalse(ReferenceEquals(pt1,pt2));
            AssertAreEqual(pt1,pt2);

            AssertAreEqual(pt1.GetAllArguments(),pt2.GetAllArguments());

            RuntimeTaskFactory f = new RuntimeTaskFactory(Repository);

            var stg = MockRepository.GenerateMock<IStageArgs>();
            stg.Stub(x => x.LoadStage).Return(LoadStage.Mounting);

            f.Create(pt1, stg);

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