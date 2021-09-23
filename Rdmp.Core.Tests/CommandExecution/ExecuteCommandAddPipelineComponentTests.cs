// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandAddPipelineComponentTests : CommandCliTests
    {
        [Test]
        public void TestCreatePipelineWithCommands()
        {
            var p = WhenIHaveA<Pipeline>();

            Assert.IsNull(p.Source);
            Assert.IsNull(p.Destination);
            Assert.IsEmpty(p.PipelineComponents);

            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(DelimitedFlatFileDataFlowSource));

            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(CleanStrings),"2");
            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ColumnSwapper),"1");

            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ExecuteFullExtractionToDatabaseMSSql));

            p.ClearAllInjections();

            Assert.IsNotNull(p.Source);
            Assert.AreEqual(typeof(DelimitedFlatFileDataFlowSource), p.Source.GetClassAsSystemType());
            Assert.IsNotEmpty(p.Source.GetAllArguments());

            Assert.AreEqual(4, p.PipelineComponents.Count);

            Assert.AreEqual(1, p.PipelineComponents[1].Order);
            Assert.AreEqual(typeof(ColumnSwapper), p.PipelineComponents[1].GetClassAsSystemType());

            Assert.AreEqual(2, p.PipelineComponents[2].Order);
            Assert.AreEqual(typeof(CleanStrings), p.PipelineComponents[2].GetClassAsSystemType());

            Assert.IsNotNull(p.Destination);
            Assert.AreEqual(typeof(ExecuteFullExtractionToDatabaseMSSql), p.Destination.GetClassAsSystemType());
            Assert.IsNotEmpty(p.Destination.GetAllArguments());


        }

        [Test]
        public void TestCreatePipeline_TooManySources()
        {
            var p = WhenIHaveA<Pipeline>();

            Assert.IsNull(p.Source);

            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(DelimitedFlatFileDataFlowSource));
            var ex = Assert.Throws<Exception>(()=>Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(DelimitedFlatFileDataFlowSource)));

            Assert.AreEqual("Pipeline 'My Pipeline' already has a source",ex.Message);
        }
        [Test]
        public void TestCreatePipeline_TooManyDestinations()
        {
            var p = WhenIHaveA<Pipeline>();

            Assert.IsNull(p.Source);

            Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ExecuteFullExtractionToDatabaseMSSql));
            var ex = Assert.Throws<Exception>(() => Run("AddPipelineComponent", $"Pipeline:{p.ID}", nameof(ExecuteFullExtractionToDatabaseMSSql)));

            Assert.AreEqual("Pipeline 'My Pipeline' already has a destination", ex.Message);
        }
    }
}
