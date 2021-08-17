// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System.Data;
using System.Linq;

namespace Rdmp.Core.Tests.CohortCreation
{
    class PluginCohortCompilerTests : CohortQueryBuilderWithCacheTests
    {
        [Test]
        public void TestIPluginCohortCompiler_PopulatesCacheCorrectly()
        {
            var activator = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true };

            // create a cohort config
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic");
            cic.QueryCachingServer_ID = externalDatabaseServer.ID;
            cic.SaveToDatabase();

            // this special Catalogue will be detected by GenRandom and interpreted as an API call
            var myApi = new Catalogue(CatalogueRepository,"API_myapi");


            // add it to the cohort config
            cic.CreateRootContainerIfNotExists();

            // create a use of the API as an AggregateConfiguration
            var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator, new CatalogueCombineable(myApi),cic.RootCohortAggregateContainer);

            Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);
            cmd.Execute();

            // run the cic
            var source = new CohortIdentificationConfigurationSource();
            source.PreInitialize(cic, new ThrowImmediatelyDataLoadEventListener());

            var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            Assert.AreEqual(2, dt.Rows.Count);

            var results = new []{ (string)dt.Rows[0][0],(string)dt.Rows[1][0] };

            Assert.Contains("0101010101", results);
            Assert.Contains("0202020202", results);
        }

        public class GenRandom : PluginCohortCompiler
        {
            public override void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
            {
                // simulate going to an API and getting 2 results
                SubmitIdentifierList("identifiers",new[] { "0101010101", "0202020202" }, ac, cache );
            }
            public override bool ShouldRun(ICatalogue catalogue)
            {
                return catalogue.Name.Equals(ApiPrefix+"myapi");
            }
        }
    }
}
