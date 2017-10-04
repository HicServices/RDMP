using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CohortManagerLibrary.Execution
{
    public interface ICachableTask:ICompileable
    {
        AggregateConfiguration GetAggregateConfiguration();
        CacheCommitArguments GetCacheArguments(string sql, DataTable results,DatabaseColumnRequest[] explicitTypes);
        void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager);

        bool IsCacheableWhenFinished();
        bool CanDeleteCache();
    }
}
