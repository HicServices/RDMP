using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using FAnsi.Discovery;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;

namespace CohortManagerLibrary.Execution
{
    /// <summary>
    /// Any ICompileable which can be cached once finished.  Typically any ICompileable in a CohortCompiler can be cached unless it is composed of multiple discrete
    /// sub queries (i.e. an AggregationContainerTask.) 
    /// </summary>
    public interface ICacheableTask:ICompileable
    {
        AggregateConfiguration GetAggregateConfiguration();
        CacheCommitArguments GetCacheArguments(string sql, DataTable results,DatabaseColumnRequest[] explicitTypes);
        void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager);

        bool IsCacheableWhenFinished();
        bool CanDeleteCache();
    }
}
