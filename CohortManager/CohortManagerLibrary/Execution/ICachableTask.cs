using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;

namespace CohortManagerLibrary.Execution
{
    public interface ICachableTask:ICompileable
    {
        AggregateConfiguration GetAggregateConfiguration();
        CacheCommitArguments GetCacheArguments(string sql, DataTable results, Dictionary<string, string> explicitTypingDictionary);
        void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager);

        bool IsCacheableWhenFinished();
        bool CanDeleteCache();
    }
}
