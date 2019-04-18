// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using CatalogueLibraryTests.Mocks;
using DataExportLibrary.Repositories;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Implementations.MicrosoftSQL;
using LoadModules.Generic.Attachers;
using LoadModules.Generic.Mutilators;
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
        public void GatherAndShare_LoadMetadata_WithProcessTask()
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
        public void GatherAndShare_LoadMetadata_WithRealProcessTask()
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

        
        /// <summary>
        /// Tests sharing a <see cref="LoadMetadata"/> with a <see cref="ProcessTask"/> which has a reference argument (to
        /// another object in the database.
        /// </summary>
        [Test]
        public void GatherAndShare_LoadMetadata_WithReferenceProcessTaskArgument()
        {
            //create an object
            LoadMetadata lmd1 = WhenIHaveA<LoadMetadata>();

            //setup Reflection / MEF
            SetupMEF();
            RuntimeTaskFactory f = new RuntimeTaskFactory(Repository);
            var stg = MockRepository.GenerateMock<IStageArgs>();
            stg.Stub(x => x.LoadStage).Return(LoadStage.Mounting);
            stg.Stub(x => x.DbInfo)
                .Return(new DiscoveredServer(new SqlConnectionStringBuilder()).ExpectDatabase("d"));
            
            //create a single process task for the load
            var pt1 = new ProcessTask(Repository, lmd1, LoadStage.Mounting);
            pt1.ProcessTaskType = ProcessTaskType.MutilateDataTable;
            pt1.LoadStage = LoadStage.AdjustRaw;
            pt1.Path = typeof(SafePrimaryKeyCollisionResolverMutilation).FullName;
            pt1.SaveToDatabase();

            //give it a reference to an (unshared) object (ColumnInfo)
            pt1.CreateArgumentsForClassIfNotExists(typeof(SafePrimaryKeyCollisionResolverMutilation));
            var pta = pt1.ProcessTaskArguments.Single(pt => pt.Name == "ColumnToResolveOn");
            pta.SetValue(WhenIHaveA<ColumnInfo>());
            pta.SaveToDatabase();

            //check that reflection can assemble the master ProcessTask
            MutilateDataTablesRuntimeTask t = (MutilateDataTablesRuntimeTask) f.Create(pt1, stg);
            Assert.IsNotNull(((SafePrimaryKeyCollisionResolverMutilation)t.MEFPluginClassInstance).ColumnToResolveOn);
            
            //share to the second repository (which won't have that ColumnInfo)
            var lmd2 = ShareToNewRepository(lmd1);
            
            //create a new reflection factory for the new repo
            RuntimeTaskFactory f2 = new RuntimeTaskFactory(lmd2.CatalogueRepository);
            lmd2.CatalogueRepository.MEF = MEF;
            
            //when we create the shared instance it should not have a valid value for ColumnInfo (since it wasn't - and shouldn't be shared)
            MutilateDataTablesRuntimeTask t2 = (MutilateDataTablesRuntimeTask) f2.Create(lmd2.ProcessTasks.Single(), stg);
            Assert.IsNull(((SafePrimaryKeyCollisionResolverMutilation)t2.MEFPluginClassInstance).ColumnToResolveOn);

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