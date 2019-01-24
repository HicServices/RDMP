using System.Collections.Generic;
using System.Data;
using CatalogueLibrary.Data.Aggregation;
using FAnsi.Discovery;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;

namespace CohortManagerLibrary.Execution
{
    public abstract class CacheableTask : Compileable, ICacheableTask
    {
        protected CacheableTask(CohortCompiler compiler) : base(compiler)
        {
        }

        public abstract AggregateConfiguration GetAggregateConfiguration();
        public abstract CacheCommitArguments GetCacheArguments(string sql, DataTable results, DatabaseColumnRequest[] explicitTypes);
        public abstract void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager);
        
        public bool IsCacheableWhenFinished()
        {
            if (!_compiler.Tasks.ContainsKey(this))
                return false;

            return _compiler.Tasks[this].SubQueries > _compiler.Tasks[this].SubqueriesCached;
        }

        public bool CanDeleteCache()
        {
            return _compiler.Tasks[this].SubqueriesCached > 0;
        }

    }
}