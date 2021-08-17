using FAnsi.Discovery;
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
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System.Data;
using System.Linq;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.CohortCreation
{
    class PluginCohortCompilerTests : CohortQueryBuilderWithCacheTests
    {
        [Test]
        public void TestIPluginCohortCompiler_PopulatesCacheCorrectly()
        {
            var activator = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true };

            Assert.GreaterOrEqual(activator.PluginCohortCompilers.Count,1);
            Assert.Contains(typeof(GenRandom), activator.PluginCohortCompilers.Select(t=>t.GetType()).ToArray());

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
                using var dt = new DataTable();
                dt.Columns.Add("identifiers");

                dt.Rows.Add("0101010101");
                dt.Rows.Add("0202020202");


                // this is how you commit the results to the cache
                var args = new CacheCommitIdentifierList(ac,ac.Description??"none",dt,
                    new DatabaseColumnRequest("identifiers",new DatabaseTypeRequest(typeof(string),10),false),5000);

                cache.CommitResults(args);
            }
            public override bool ShouldRun(ICatalogue catalogue)
            {
                return catalogue.Name.Equals(ApiPrefix+"myapi");
            }
        }
    }
}
