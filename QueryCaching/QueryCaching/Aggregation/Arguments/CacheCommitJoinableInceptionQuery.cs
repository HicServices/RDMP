using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DataTableExtension;

namespace QueryCaching.Aggregation.Arguments
{
    public class CacheCommitJoinableInceptionQuery:CacheCommitArguments
    {
        public CacheCommitJoinableInceptionQuery(AggregateConfiguration configuration, string sql, DataTable results, Dictionary<string, string> explicitTypesDictionary,int timeout) 
            : base(AggregateOperation.JoinableInceptionQuery, configuration, sql, results, explicitTypesDictionary,timeout)
        {
        }

        public override void CommitTableDataCompleted(DiscoveredServer server,string tableName, DataTableHelper helper, DbConnection con, DbTransaction transaction)
        {
            
        }
    }
}