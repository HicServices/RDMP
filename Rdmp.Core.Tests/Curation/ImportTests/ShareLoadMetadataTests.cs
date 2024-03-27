// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Data.SqlClient;
using System.Linq;
using FAnsi.Discovery;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Rdmp.Core.Repositories;
using Rdmp.Core.Sharing.Dependency.Gathering;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.ImportTests;

public class ShareLoadMetadataTests : UnitTests
{
    [Test]
    public void GatherAndShare_LoadMetadata_EmptyLoadMetadata()
    {
        //create an object
        var lmd = new LoadMetadata(Repository, "MyLmd");

        var lmd2 = ShareToNewRepository(lmd);

        Assert.Multiple(() =>
        {
            //different repos so not identical
            Assert.That(ReferenceEquals(lmd, lmd2), Is.False);
            Assert.That(lmd2.Name, Is.EqualTo(lmd.Name));
        });
    }

    [Test]
    public void GatherAndShare_LoadMetadata_WithCatalogue()
    {
        //create an object
        LoadMetadata lmd1 = WhenIHaveA<LoadMetadata>();
        var lmd2 = ShareToNewRepository(lmd1);

        var cata1 = lmd1.GetAllCatalogues().Single();
        var cata2 = lmd2.GetAllCatalogues().Single();

        Assert.Multiple(() =>
        {
            //different repos so not identical
            Assert.That(ReferenceEquals(lmd1, lmd2), Is.False);
            Assert.That(ReferenceEquals(cata1, cata2), Is.False);

            Assert.That(lmd2.Name, Is.EqualTo(lmd1.Name));
            Assert.That(cata2.Name, Is.EqualTo(cata1.Name));
        });
    }

    /// <summary>
    /// Tests sharing a basic process task load metadata
    /// </summary>
    [Test]
    public void GatherAndShare_LoadMetadata_WithProcessTask()
    {
        //create an object
        LoadMetadata lmd1;
        var lmd2 = ShareToNewRepository(lmd1 = WhenIHaveA<ProcessTaskArgument>().ProcessTask.LoadMetadata);

        var pt1 = lmd1.ProcessTasks.Single();
        var pt2 = lmd2.ProcessTasks.Single();

        //different repos so not identical
        Assert.That(ReferenceEquals(lmd1, lmd2), Is.False);
        AssertAreEqual(lmd1, lmd2);

        Assert.That(ReferenceEquals(pt1, pt2), Is.False);
        AssertAreEqual(pt1, pt2);

        Assert.That(ReferenceEquals(pt1.ProcessTaskArguments.Single(), pt2.ProcessTaskArguments.Single()), Is.False);
        AssertAreEqual(pt1.ProcessTaskArguments.Single(), pt2.ProcessTaskArguments.Single());
    }

    /// <summary>
    /// Tests sharing a more advanced loadmetadata with an actual class behind the ProcessTask
    /// </summary>
    [Test]
    public void GatherAndShare_LoadMetadata_WithRealProcessTask()
    {
        //create an object
        var lmd1 = WhenIHaveA<LoadMetadata>();

        var pt1 = new ProcessTask(Repository, lmd1, LoadStage.Mounting)
        {
            ProcessTaskType = ProcessTaskType.Attacher,
            LoadStage = LoadStage.Mounting,
            Path = typeof(AnySeparatorFileAttacher).FullName
        };
        pt1.SaveToDatabase();

        pt1.CreateArgumentsForClassIfNotExists(typeof(AnySeparatorFileAttacher));
        var pta = pt1.ProcessTaskArguments.Single(pt => pt.Name == "Separator");
        pta.SetValue(",");
        pta.SaveToDatabase();


        var lmd2 = ShareToNewRepository(lmd1);

        //different repos so not identical
        Assert.That(ReferenceEquals(lmd1, lmd2), Is.False);
        AssertAreEqual(lmd1, lmd2);

        var pt2 = lmd2.ProcessTasks.Single();

        Assert.That(ReferenceEquals(pt1, pt2), Is.False);
        AssertAreEqual(pt1, pt2);

        AssertAreEqual(pt1.GetAllArguments(), pt2.GetAllArguments());

        var f = new RuntimeTaskFactory(Repository);

        var stg = Substitute.For<IStageArgs>();
        stg.LoadStage.Returns(LoadStage.Mounting);

        RuntimeTaskFactory.Create(pt1, stg);
    }


    /// <summary>
    /// Tests sharing a <see cref="LoadMetadata"/> with a <see cref="ProcessTask"/> which has a reference argument (to
    /// another object in the database.
    /// </summary>
    [Test]
    public void GatherAndShare_LoadMetadata_WithReferenceProcessTaskArgument()
    {
        //create an object
        var lmd1 = WhenIHaveA<LoadMetadata>();

        var f = new RuntimeTaskFactory(Repository);
        var stg = Substitute.For<IStageArgs>();
        stg.LoadStage.Returns(LoadStage.Mounting);
        stg.DbInfo.Returns(new DiscoveredServer(new SqlConnectionStringBuilder()).ExpectDatabase("d"));

        //create a single process task for the load
        var pt1 = new ProcessTask(Repository, lmd1, LoadStage.Mounting)
        {
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            LoadStage = LoadStage.AdjustRaw,
            Path = typeof(SafePrimaryKeyCollisionResolverMutilation).FullName
        };
        pt1.SaveToDatabase();

        //give it a reference to an (unshared) object (ColumnInfo)
        pt1.CreateArgumentsForClassIfNotExists(typeof(SafePrimaryKeyCollisionResolverMutilation));
        var pta = pt1.ProcessTaskArguments.Single(pt => pt.Name == "ColumnToResolveOn");
        pta.SetValue(WhenIHaveA<ColumnInfo>());
        pta.SaveToDatabase();

        //check that reflection can assemble the master ProcessTask
        var t = (MutilateDataTablesRuntimeTask)RuntimeTaskFactory.Create(pt1, stg);
        Assert.That(((SafePrimaryKeyCollisionResolverMutilation)t.MEFPluginClassInstance).ColumnToResolveOn, Is.Not.Null);

        //share to the second repository (which won't have that ColumnInfo)
        var lmd2 = ShareToNewRepository(lmd1);

        //create a new reflection factory for the new repo
        var f2 = new RuntimeTaskFactory(lmd2.CatalogueRepository);

        //when we create the shared instance it should not have a valid value for ColumnInfo (since it wasn't - and shouldn't be shared)
        var t2 = (MutilateDataTablesRuntimeTask)RuntimeTaskFactory.Create(lmd2.ProcessTasks.Single(), stg);
        Assert.That(((SafePrimaryKeyCollisionResolverMutilation)t2.MEFPluginClassInstance).ColumnToResolveOn, Is.Null);
    }

    private LoadMetadata ShareToNewRepository(LoadMetadata lmd)
    {
        var gatherer = new Gatherer(RepositoryLocator);

        Assert.That(gatherer.CanGatherDependencies(lmd));
        var rootObj = gatherer.GatherDependencies(lmd);

        var sm = new ShareManager(RepositoryLocator, null);
        var shareDefinition = rootObj.ToShareDefinitionWithChildren(sm);

        var repo2 = new MemoryDataExportRepository();
        var sm2 = new ShareManager(new RepositoryProvider(repo2));
        return sm2.ImportSharedObject(shareDefinition).OfType<LoadMetadata>().Single();
    }
}