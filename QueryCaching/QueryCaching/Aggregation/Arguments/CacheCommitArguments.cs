using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data.Aggregation;
using ReusableLibraryCode.DataTableExtension;

namespace QueryCaching.Aggregation.Arguments
{
    public abstract class CacheCommitArguments
    {
        protected readonly int Timeout;
        public AggregateOperation Operation { get; private set; }
        public AggregateConfiguration Configuration { get; set; }
        public string SQL { get; private set; }
        public DataTable Results { get; private set; }
        public Dictionary<string, string> ExplicitTypesDictionary { get; private set; }

        protected CacheCommitArguments(AggregateOperation operation, AggregateConfiguration configuration, string sql, DataTable results, Dictionary<string, string> explicitTypesDictionary, int timeout)
        {
            Timeout = timeout;
            Operation = operation;
            Configuration = configuration;
            SQL = sql;
            Results = results;
            ExplicitTypesDictionary = explicitTypesDictionary;

            if (results == null)
                throw new Exception("DataTable results must have a value");

        }

        public abstract void CommitTableDataCompleted(string tableName, DataTableHelper helper, DbConnection con, DbTransaction transaction);
    }
}