using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using ReusableLibraryCode.DataTableExtension;

namespace QueryCaching.Aggregation.Arguments
{
    public class CacheCommitExtractableAggregate : CacheCommitArguments
    {
        public CacheCommitExtractableAggregate(AggregateConfiguration configuration, string sql, DataTable results, Dictionary<string, string> explicitTypesDictionary,int timeout) : base(AggregateOperation.ExtractableAggregateResults,configuration, sql, results, explicitTypesDictionary,timeout)
        {
            if (results.Columns.Count == 0)
                throw new ArgumentException("The DataTable that you claimed was an " + Operation + " had zero columns and therefore cannot be cached");

            string[] suspectDimensions =
                configuration.AggregateDimensions
                    .Where(d => d.IsExtractionIdentifier || d.HashOnDataRelease)
                    .Select(d => d.GetRuntimeName())
                    .ToArray();
            if (suspectDimensions.Any())
                throw new NotSupportedException("Aggregate " + configuration +
                                                " contains dimensions marked as IsExtractionIdentifier or HashOnDataRelease (" +
                                                string.Join(",", suspectDimensions) +
                                                ") so the aggregate cannot be cached.  This would/could result in private patient identifiers appearing on your website!");

            if (!configuration.IsExtractable)
                throw new NotSupportedException("Aggregate " + configuration + " is not marked as IsExtractable therefore cannot be cached for publication on website");
            
        }

        public override void CommitTableDataCompleted(string tableName, DataTableHelper helper, DbConnection con, DbTransaction transaction)
        {
            //no need to do anything here we dont need index or anything else
        }
    }
}