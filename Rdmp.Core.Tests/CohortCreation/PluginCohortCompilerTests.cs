// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.CohortCreation
{
    public class PluginCohortCompilerTests : CohortQueryBuilderWithCacheTests
    {
        [Test]
        public void TestIPluginCohortCompiler_PopulatesCacheCorrectly()
        {
            var activator = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true };

            // create a cohort config
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic");
            cic.QueryCachingServer_ID = externalDatabaseServer.ID;
            cic.SaveToDatabase();

            // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
            var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

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

            // 5 random chi numbers
            Assert.AreEqual(5, dt.Rows.Count);

            // test stale
            cmd.AggregateCreatedIfAny.Description = "2";
            cmd.AggregateCreatedIfAny.SaveToDatabase();

            // run the cic again
            source = new CohortIdentificationConfigurationSource();
            source.PreInitialize(cic, new ThrowImmediatelyDataLoadEventListener());
            dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            // because the rules changed to generate 2 chis only there should be a new result
            Assert.AreEqual(2, dt.Rows.Count);

            var results = new[] { (string)dt.Rows[0][0], (string)dt.Rows[1][0] };

            // run the cic again with no changes, the results should be unchanged since there is no config changed
            // I.e. no new chis should be generated and the cached values returned
            source = new CohortIdentificationConfigurationSource();
            source.PreInitialize(cic, new ThrowImmediatelyDataLoadEventListener());
            dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.AreEqual(2, dt.Rows.Count);
            var results2 = new[] { (string)dt.Rows[0][0], (string)dt.Rows[1][0] };

            Assert.AreEqual(results[0], results2[0]);
            Assert.AreEqual(results[1], results2[1]);

        }
        [Test]
        public void TestIPluginCohortCompiler_TestCloneCic()
        {
            var activator = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true };

            // create a cohort config
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic");
            cic.QueryCachingServer_ID = externalDatabaseServer.ID;
            cic.SaveToDatabase();

            // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
            var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

            // add it to the cohort config
            cic.CreateRootContainerIfNotExists();

            // create a use of the API as an AggregateConfiguration
            var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator, new CatalogueCombineable(myApi), cic.RootCohortAggregateContainer);
            Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);
            cmd.Execute();
            cmd.AggregateCreatedIfAny.Description = "33";
            cmd.AggregateCreatedIfAny.SaveToDatabase();

            // clone the cic
            var cmd2 = new ExecuteCommandCloneCohortIdentificationConfiguration(activator, cic);
            Assert.IsFalse(cmd2.IsImpossible, cmd2.ReasonCommandImpossible);
            cmd2.Execute();

            var cloneAc = cmd2.CloneCreatedIfAny.RootCohortAggregateContainer.GetAggregateConfigurations()[0];
            Assert.AreEqual("33", cloneAc.Description);
        }

        
        [Test]
        public void TestIPluginCohortCompiler_AsPatientIndexTable()
        {
            var activator = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true };

            // create a cohort config
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic");
            cic.QueryCachingServer_ID = externalDatabaseServer.ID;
            cic.SaveToDatabase();

            // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
            var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

            // add it to the cohort config
            cic.CreateRootContainerIfNotExists();

            // We need something in the root container otherwise the cic won't build
            IAtomicCommand cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator, new CatalogueCombineable(myApi), cic.RootCohortAggregateContainer);
            Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);
            cmd.Execute();

            // The thing we are wanting to test - creating a use of the API as a patient index table
            cmd = new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(
                activator, new CatalogueCombineable(myApi), cic);

            Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);
            cmd.Execute();

            var joinables = cic.GetAllJoinables();

            Assert.AreEqual(1, joinables.Length);

            // run the cic again
            var source = new CohortIdentificationConfigurationSource();
            source.PreInitialize(cic, new ThrowImmediatelyDataLoadEventListener());
            source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

        }
    }
}
