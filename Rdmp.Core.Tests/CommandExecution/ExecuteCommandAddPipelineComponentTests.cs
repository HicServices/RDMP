// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using System;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandAddPipelineComponentTests : CommandCliTests
{
    [Test]
    public void TestCreatePipelineWithCommands()
    {
        var p = WhenIHaveA<Pipeline>();

        Assert.That(p.Source, Is.Null);
        Assert.That(p.Destination, Is.Null);
        Assert.That(p.PipelineComponents, Is.Empty);

        Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(DelimitedFlatFileDataFlowSource));

        Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(CleanStrings), "2");
        Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ColumnSwapper), "1");

        Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ExecuteFullExtractionToDatabaseMSSql));

        p.ClearAllInjections();

        Assert.That(p.Source, Is.Not.Null);
        Assert.That(p.Source.GetClassAsSystemType(), Is.EqualTo(typeof(DelimitedFlatFileDataFlowSource)));
        Assert.IsNotEmpty(p.Source.GetAllArguments());

        Assert.That(p.PipelineComponents, Has.Count.EqualTo(4));

        Assert.That(p.PipelineComponents[1].Order, Is.EqualTo(1));
        Assert.That(p.PipelineComponents[1].GetClassAsSystemType(), Is.EqualTo(typeof(ColumnSwapper)));

        Assert.That(p.PipelineComponents[2].Order, Is.EqualTo(2));
        Assert.That(p.PipelineComponents[2].GetClassAsSystemType(), Is.EqualTo(typeof(CleanStrings)));

        Assert.That(p.Destination, Is.Not.Null);
        Assert.That(p.Destination.GetClassAsSystemType(), Is.EqualTo(typeof(ExecuteFullExtractionToDatabaseMSSql)));
        Assert.IsNotEmpty(p.Destination.GetAllArguments());
    }

    [Test]
    public void TestCreatePipeline_TooManySources()
    {
        var p = WhenIHaveA<Pipeline>();

        Assert.That(p.Source, Is.Null);

        Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(DelimitedFlatFileDataFlowSource));
        var ex = Assert.Throws<Exception>(() =>
            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(DelimitedFlatFileDataFlowSource)));

        Assert.That(ex.Message, Is.EqualTo("Pipeline 'My Pipeline' already has a source"));
    }

    [Test]
    public void TestCreatePipeline_TooManyDestinations()
    {
        var p = WhenIHaveA<Pipeline>();

        Assert.That(p.Source, Is.Null);

        Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ExecuteFullExtractionToDatabaseMSSql));
        var ex = Assert.Throws<Exception>(() =>
            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ExecuteFullExtractionToDatabaseMSSql)));

        Assert.That(ex.Message, Is.EqualTo("Pipeline 'My Pipeline' already has a destination"));
    }
}