using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data.Aggregation;
using FAnsi.Discovery;

namespace QueryCaching.Aggregation.Arguments
{
    /// <summary>
    /// Request to cache an AggregateConfiguration that is a 'patient index table' (See JoinableCohortAggregateConfiguration).  This will include patient 
    /// identifier and some useful columns (e.g. 'prescription dates for methadone by patient id').  The resulting cached DataTable will be joined against
    /// patient identifier lists to answer questions such as 'who has been hospitalised (SMR01) within 6 months of a prescription for methadone'.
    /// 
    /// <para>When doing such a join on two large datasets you can end up with a query that will never complete without intermediate caching.</para>
    /// 
    /// <para>Serves as an input to CachedAggregateConfigurationResultsManager.</para>
    ///</summary>
    public class CacheCommitJoinableInceptionQuery:CacheCommitArguments
    {
        public CacheCommitJoinableInceptionQuery(AggregateConfiguration configuration, string sql, DataTable results, DatabaseColumnRequest[] explicitTypes,int timeout)
            : base(AggregateOperation.JoinableInceptionQuery, configuration, sql, results, timeout, explicitTypes)
        {
        }

        public override void CommitTableDataCompleted(DiscoveredTable resultingTable)
        {
            
        }
    }
}
