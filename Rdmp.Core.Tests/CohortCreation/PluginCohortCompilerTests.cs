using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.CohortCreation;
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

            Assert.AreEqual(1, activator.PluginCohortCompilers.Count);
            Assert.AreEqual(typeof(GenRandom), activator.PluginCohortCompilers.Single().GetType());

            // create a cohort config
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic");
            cic.QueryCachingServer_ID = externalDatabaseServer.ID;
            cic.SaveToDatabase();

            // this special Catalogue will be detected by GenRandom and interpreted as an API call
            var myApi = new Catalogue(CatalogueRepository,"myapi");

            // create a use of the API as an AggregateConfiguration
            var ac = new AggregateConfiguration(CatalogueRepository, myApi,"myac");

            // add it to the cohort config
            cic.CreateRootContainerIfNotExists();
            cic.RootCohortAggregateContainer.AddChild(ac,0);

            // run the cic
            var source = new CohortIdentificationConfigurationSource();
            source.PreInitialize(cic, new ThrowImmediatelyDataLoadEventListener());

            var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            Assert.AreEqual(2, dt);

            var results = new []{ (string)dt.Rows[0][0],(string)dt.Rows[1][0] };

            Assert.Contains("0101010101", results);
            Assert.Contains("0202020202", results);
        }

        public class GenRandom : IPluginCohortCompiler
        {
            public void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
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

            public bool ShouldRun(AggregateConfiguration ac)
            {
                return ac.Catalogue.Name.Equals("myapi");
            }
        }
    }
}
