using BadMedicine;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using System;
using System.Data;
using System.Text;
using TypeGuesser;

namespace Rdmp.Core.CohortCreation.Execution
{
    class ExamplePluginCohortCompiler : PluginCohortCompiler
    {
        public const string ExampleAPIName = ApiPrefix + "GenerateRandomChisExample";

        public override void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
        {
            // generate random chi numbers
            using var dt = new DataTable();
            dt.Columns.Add("chi");

            int toGenerate = 5;
            if (int.TryParse(ac.Description, out int result))
            {
                toGenerate = result;
            }

            var pc = new PersonCollection();
            pc.GeneratePeople(toGenerate, new Random());
            
            foreach(var p in pc.People)
            {
                dt.Rows.Add(p.CHI);
            }

            // this is how you commit the results to the cache
            var args = new CacheCommitIdentifierList(ac, ac.Description ?? "none", dt,
                new DatabaseColumnRequest("chi", new DatabaseTypeRequest(typeof(string), 10), false), 5000);

            cache.CommitResults(args);
        }

        public override bool ShouldRun(ICatalogue cata)
        {
            return cata.Name.Equals(ExampleAPIName);
        }
        
    }
}
