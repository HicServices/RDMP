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
            Assert.That(pipeline.Name, Is.EqualTo("Bob"));

            var pipelineComponent =
                new PipelineComponent(CatalogueRepository, pipeline, typeof(BasicAnonymisationEngine), 0);

            try
            {
                Assert.That(typeof(BasicAnonymisationEngine).FullName, Is.EqualTo(pipelineComponent.Class));

                var argument1 = (PipelineComponentArgument)pipelineComponent.CreateNewArgument();
                var argument2 = new PipelineComponentArgument(CatalogueRepository, pipelineComponent);

                try
                {
                    argument1.SetType(typeof(string));
                    argument1.SetValue("bob");
                    argument1.SaveToDatabase();

                    var dt = DateTime.Now;
                    dt = new DateTime(dt.Ticks - dt.Ticks % TimeSpan.TicksPerSecond,
                        dt.Kind); //get rid of the milliseconds

                    argument2.SetType(typeof(DateTime));
                    argument2.SetValue(dt);
                    argument2.SaveToDatabase();

                    var argument2Copy = CatalogueRepository.GetObjectByID<PipelineComponentArgument>(argument2.ID);
                    Assert.That(argument2Copy.GetValueAsSystemType(), Is.EqualTo(dt));
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

        Assert.That(clone1.Name, Is.EqualTo("My Pipe (Clone)"));
        Assert.That(clone2.Name, Is.EqualTo("My Pipe (Clone2)"));
        Assert.That(clone3.Name, Is.EqualTo("My Pipe (Clone3)"));

        var cloneOfClone1 = clone3.Clone();
        var cloneOfClone2 = clone3.Clone();

        Assert.That(cloneOfClone1.Name, Is.EqualTo("My Pipe (Clone3) (Clone)"));
        Assert.That(cloneOfClone2.Name, Is.EqualTo("My Pipe (Clone3) (Clone2)"));
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

        Assert.That(p, Is.Not.EqualTo(p2));
        Assert.That(p.ID, Is.Not.EqualTo(p2.ID));

        Assert.That($"{p.Name} (Clone)", Is.EqualTo(p2.Name));

        Assert.That(RepositoryLocator.CatalogueRepository.GetAllObjects<PipelineComponent>(), Has.Length.EqualTo(componentsBefore * 2));
        Assert.That(RepositoryLocator.CatalogueRepository.GetAllObjects<PipelineComponentArgument>(), Has.Length.EqualTo(argumentsBefore * 2));

        //p the original should have a pipeline component that has the value we set earlier
        Assert.That(
p.PipelineComponents.Single(static c => c.Class == typeof(ColumnRenamer).ToString()).PipelineComponentArguments
                .Single(static a => a.Name == "ColumnNameToFind").Value, Is.EqualTo(            "MyMostCoolestColumnEver"
));

        //p2 the clone should have a pipeline component too since it's a clone
        Assert.That(
p2.PipelineComponents.Single(static c => c.Class == typeof(ColumnRenamer).ToString()).PipelineComponentArguments
                .Single(static a => a.Name == "ColumnNameToFind").Value, Is.EqualTo(            "MyMostCoolestColumnEver"
));

        //both should have source and destination components
        Assert.That(p2.DestinationPipelineComponent_ID, Is.Not.Null);
        Assert.That(p2.SourcePipelineComponent_ID, Is.Not.Null);

        //but with different IDs because they are clones
        Assert.That(p2.DestinationPipelineComponent_ID, Is.Not.EqualTo(p.DestinationPipelineComponent_ID));
        Assert.That(p2.SourcePipelineComponent_ID, Is.Not.EqualTo(p.SourcePipelineComponent_ID));

        p.DeleteInDatabase();
        p2.DeleteInDatabase();
    }

    [Test]
    public void CloneAPipeline_BrokenPipes()
    {
        var p = new Pipeline(CatalogueRepository);

        //Setup a pipeline with a source component type that doesn't exist
        var source = new PipelineComponent(CatalogueRepository, p, typeof(DelimitedFlatFileAttacher), 0)
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

        Assert.That(p.Source.GetAllArguments().Single().Type, Is.EqualTo("fffffzololz"));

        var clone = p.Clone();

        Assert.That(p.Source.Class, Is.EqualTo(clone.Source.Class));
        Assert.That(clone.Source.GetAllArguments().Single().Type, Is.EqualTo("fffffzololz"));

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
        Assert.That(p.SourcePipelineComponent_ID, Is.Null);
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
        Assert.That(p.DestinationPipelineComponent_ID, Is.Null);
    }
}