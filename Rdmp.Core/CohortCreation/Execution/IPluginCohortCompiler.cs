using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.CohortCreation.Execution
{
    /// <summary>
    /// Interface for plugins that want to perform custom tasks when part of a cohort builder query is run
    /// e.g. call out to an external API and store the resulting identifier list in the query cache
    /// </summary>
    public interface IPluginCohortCompiler
    {
        /// <summary>
        /// Return true if the <paramref name="ac"/> is of a type that should be handled by your class.
        /// All aggregates will regularly be passed to this when run so ensure that your response is fast
        /// </summary>
        /// <param name="ac"></param>
        /// <returns></returns>
        bool ShouldRun(AggregateConfiguration ac);


        /// <summary>
        /// Must be implemented such that by the time the method completes the <paramref name="cache"/> 
        /// is populated with an identifier list that matches the expectations of <paramref name="ac"/>
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="cache"></param>
        void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache);
    }
}
