// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class PipelineTests : DatabaseTests
{
    [Test]
    public void SetupAndSaveAPipeline()
    {
        var pipeline = new Pipeline(CatalogueRepository, "Bob");

        try
        {
            Assert.AreEqual(pipeline.Name, "Bob");

            var pipelineComponent =
                new PipelineComponent(CatalogueRepository, pipeline, typeof(BasicAnonymisationEngine), 0);

            try
            {
                Assert.AreEqual(pipelineComponent.Class, typeof(BasicAnonymisationEngine).FullName);

                var argument1 = (PipelineComponentArgument)pipelineComponent.CreateNewArgument();
                var argument2 = new PipelineComponentArgument(CatalogueRepository, pipelineComponent);

                try
                {
                    argument1.SetType(typeof(string));
                    argument1.SetValue("bob");
                    argument1.SaveToDatabase();

                    var dt = DateTime.Now ;
                    dt = new DateTime(dt.Ticks - dt.Ticks % TimeSpan.TicksPerSecond,dt.Kind);//get rid of the milliseconds

                    argument2.SetType(typeof(DateTime));
                    argument2.SetValue(dt);
                    argument2.SaveToDatabase();

                    var argument2Copy = CatalogueRepository.GetObjectByID<PipelineComponentArgument>(argument2.ID);
                    Assert.AreEqual(dt, argument2Copy.GetValueAsSystemType());
                }
                finally
                {
                    argument1.DeleteInDatabase();
                    argument2.DeleteInDatabase();
                }
            }
            finally
            {
                pipelineComponent.DeleteInDatabase();
            }
        }
        finally
        {
            pipeline.DeleteInDatabase();
        }
    }

    [Test]
    public void ClonePipelineNaming()
    {
        var p = new Pipeline(CatalogueRepository)
        {
            Name = "My Pipe"
        };
        p.SaveToDatabase();

        var clone1 = p.Clone();
        var clone2 = p.Clone();
        var clone3 = p.Clone();

        Assert.AreEqual("My Pipe (Clone)", clone1.Name);
        Assert.AreEqual("My Pipe (Clone2)", clone2.Name);
        Assert.AreEqual("My Pipe (Clone3)", clone3.Name);

        var cloneOfClone1 = clone3.Clone();
        var cloneOfClone2 = clone3.Clone();

        Assert.AreEqual("My Pipe (Clone3) (Clone)", cloneOfClone1.Name);
        Assert.AreEqual("My Pipe (Clone3) (Clone2)", cloneOfClone2.Name);
    }


    /// <summary>
    /// Tests the ability to clone a <see cref="Pipeline"/> including all
    /// components and arguments.
    /// </summary>
    /// <exception cref="InconclusiveException"></exception>
    [Test]
    public void CloneAPipeline()
    {
        var p = new Pipeline(CatalogueRepository);

        var source = new PipelineComponent(CatalogueRepository, p, typeof(DelimitedFlatFileAttacher), 0);
        source.CreateArgumentsForClassIfNotExists<DelimitedFlatFileAttacher>();

        var middle = new PipelineComponent(CatalogueRepository, p, typeof(ColumnRenamer), 1);
        middle.CreateArgumentsForClassIfNotExists<ColumnRenamer>();

        var middle2 = new PipelineComponent(CatalogueRepository, p, typeof(ColumnForbidder), 1);
        middle2.CreateArgumentsForClassIfNotExists<ColumnForbidder>();

        var destination = new PipelineComponent(CatalogueRepository, p, typeof(DataTableUploadDestination), 2);
        destination.CreateArgumentsForClassIfNotExists<DataTableUploadDestination>();

        p.SourcePipelineComponent_ID = source.ID;
        p.DestinationPipelineComponent_ID = destination.ID;
        p.SaveToDatabase();

        var componentsBefore = RepositoryLocator.CatalogueRepository.GetAllObjects<PipelineComponent>().Length;
        var argumentsBefore = RepositoryLocator.CatalogueRepository.GetAllObjects<PipelineComponentArgument>().Length;

        var arg = p.PipelineComponents.Single(c => c.Class == typeof(ColumnRenamer).ToString())
            .PipelineComponentArguments.Single(a => a.Name == "ColumnNameToFind");
        arg.SetValue("MyMostCoolestColumnEver");
        arg.SaveToDatabase();

        //Execute the cloning process
        var p2 = p.Clone();

        Assert.AreNotEqual(p2, p);
        Assert.AreNotEqual(p2.ID, p.ID);

        Assert.AreEqual(p2.Name, $"{p.Name} (Clone)");

        Assert.AreEqual(componentsBefore *2, RepositoryLocator.CatalogueRepository.GetAllObjects<PipelineComponent>().Length);
        Assert.AreEqual(argumentsBefore *2, RepositoryLocator.CatalogueRepository.GetAllObjects<PipelineComponentArgument>().Length);

        //p the original should have a pipeline component that has the value we set earlier
        Assert.AreEqual(
            p.PipelineComponents.Single(c => c.Class == typeof(ColumnRenamer).ToString()).PipelineComponentArguments
                .Single(a => a.Name == "ColumnNameToFind").Value,
            "MyMostCoolestColumnEver"
        );

        //p2 the clone should have a pipeline component too since it's a clone
        Assert.AreEqual(
            p2.PipelineComponents.Single(c => c.Class == typeof(ColumnRenamer).ToString()).PipelineComponentArguments
                .Single(a => a.Name == "ColumnNameToFind").Value,
            "MyMostCoolestColumnEver"
        );

        //both should have source and destination components
        Assert.NotNull(p2.DestinationPipelineComponent_ID);
        Assert.NotNull(p2.SourcePipelineComponent_ID);

        //but with different IDs because they are clones
        Assert.AreNotEqual(p.DestinationPipelineComponent_ID, p2.DestinationPipelineComponent_ID);
        Assert.AreNotEqual(p.SourcePipelineComponent_ID, p2.SourcePipelineComponent_ID);

        p.DeleteInDatabase();
        p2.DeleteInDatabase();
    }

    [Test]
    public void CloneAPipeline_BrokenPipes()
    {
        var p = new Pipeline(CatalogueRepository);

        //Setup a pipeline with a source component type that doesn't exist
        var source = new PipelineComponent(CatalogueRepository, p, typeof (DelimitedFlatFileAttacher), 0)
 {
     Class = "Trollololol"
 };
        source.SaveToDatabase();

        var arg = source.CreateNewArgument();

        //Also give the source component a non existent argument
        arg.GetType().GetProperty("Type").SetValue(arg, "fffffzololz");
        arg.SaveToDatabase();

        p.SourcePipelineComponent_ID = source.ID;
        p.SaveToDatabase();

        Assert.AreEqual("fffffzololz", p.Source.GetAllArguments().Single().Type);

        var clone = p.Clone();

        Assert.AreEqual(clone.Source.Class, p.Source.Class);
        Assert.AreEqual("fffffzololz", clone.Source.GetAllArguments().Single().Type);

        p.DeleteInDatabase();
        clone.DeleteInDatabase();
    }

    [Test]
    public void DeletePipelineSource_ClearsReference()
    {
        var p = new Pipeline(CatalogueRepository);

        //Setup a pipeline with a source component
        var source = new PipelineComponent(CatalogueRepository, p, typeof(DelimitedFlatFileAttacher), 0)
        {
            Class = "Trollololol"
        };
        p.SourcePipelineComponent_ID = source.ID;
        p.SaveToDatabase();

        // delete the source
        source.DeleteInDatabase();
        p.RevertToDatabaseState();

        // should also clear the reference
        Assert.IsNull(p.SourcePipelineComponent_ID);
    }

    [Test]
    public void DeletePipelineDestination_ClearsReference()
    {
        var p = new Pipeline(CatalogueRepository);

        //Setup a pipeline with a source component
        var dest = new PipelineComponent(CatalogueRepository, p, typeof(DelimitedFlatFileAttacher), 0)
        {
            Class = "Trollololol"
        };
        p.DestinationPipelineComponent_ID = dest.ID;
        p.SaveToDatabase();

        // delete the dest
        dest.DeleteInDatabase();
        p.RevertToDatabaseState();

        // should also clear the reference
        Assert.IsNull(p.DestinationPipelineComponent_ID);
    }
}