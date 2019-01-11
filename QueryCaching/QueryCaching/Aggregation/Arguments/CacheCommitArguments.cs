using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data.Aggregation;
using FAnsi.Discovery;

namespace QueryCaching.Aggregation.Arguments
{
    public abstract class CacheCommitArguments
    {
        protected readonly int Timeout;
        public AggregateOperation Operation { get; private set; }
        public AggregateConfiguration Configuration { get; set; }
        public string SQL { get; private set; }
        public DataTable Results { get; private set; }
        public DatabaseColumnRequest[] ExplicitColumns { get; private set; }

        protected CacheCommitArguments(AggregateOperation operation, AggregateConfiguration configuration, string sql, DataTable results, int timeout, DatabaseColumnRequest[] explicitColumns = null)
        {
            Timeout = timeout;
            Operation = operation;
            Configuration = configuration;
            SQL = sql;
            Results = results;
            ExplicitColumns = explicitColumns;

            if (results == null)
                throw new Exception("DataTable results must have a value");

        }

        public abstract void CommitTableDataCompleted(DiscoveredTable resultingTable);
    }
}