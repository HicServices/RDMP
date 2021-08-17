using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.CohortCreation.Execution
{
    public abstract class PluginCohortCompiler : IPluginCohortCompiler
    {
        /// <summary>
        /// The prefix that should be on <see cref="Catalogue"/> names if they reflect API calls.
        /// Each <see cref="IPluginCohortCompiler"/> should expand upon this to identify it's specific
        /// responsibilities (e.g. if you have 2+ Types of API available)
        /// </summary>
        public const string ApiPrefix = "API_";

        public abstract void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache);
        
        public virtual bool ShouldRun(AggregateConfiguration ac)
        {
            return ShouldRun(ac.Catalogue);
        }
        public abstract bool ShouldRun(ICatalogue catalogue);
    }
}