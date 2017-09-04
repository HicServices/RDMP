using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace QueryCaching.Aggregation.Arguments
{
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