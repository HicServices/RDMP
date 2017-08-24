using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data.Aggregation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataTableExtension;

namespace QueryCaching.Aggregation.Arguments
{
    public class CacheCommitIdentifierList : CacheCommitArguments
    {
        public CacheCommitIdentifierList(AggregateConfiguration configuration,string sql, DataTable results, Dictionary<string, string> explicitTypesDictionary,int timeout) : base(AggregateOperation.IndexedExtractionIdentifierList,configuration,sql,results,explicitTypesDictionary,timeout)
        {
            //advise them if they are trying to cache an identifier list but the DataTable has more than 1 column
            if (results.Columns.Count != 1)
                throw new NotSupportedException("The DataTable did not have exactly 1 column (it had " + results.Columns.Count +" columns).  This makes it incompatible with committing to the Cache as an IdentifierList");

            //advise them if they are trying to cache a cache query itself!
            if (sql.Trim().StartsWith(CachedAggregateConfigurationResultsManager.CachingPrefix))
                throw new NotSupportedException("Sql for the query started with '" + CachedAggregateConfigurationResultsManager.CachingPrefix + "' which implies you ran some SQL code to fetch some stuff from the cache and then committed it back into the cache (obliterating the record of what the originally executed query was).  This is referred to as Inception Caching and isn't allowed.  Note to developers: this happens if user caches a query then runs the query again (fetching it from the cache) and somehow tries to commit the cache fetch request back into the cache as an overwrite");

        }

        public override void CommitTableDataCompleted(string tableName, DataTableHelper helper, DbConnection con, DbTransaction transaction)
        {
            //ask the helper what datatype it used for the identifier column
            string sqlDbTypeForColumn = helper.GetTypeDictionary()[Results.Columns[0]].GetSqlDBType();
            string colName = Results.Columns[0].ColumnName;

            //if user has an explicit type to use for the column (probably a good idea to have all extraction idetntifiers of the same data type
            if (ExplicitTypesDictionary != null && ExplicitTypesDictionary.ContainsKey(colName))
                sqlDbTypeForColumn = ExplicitTypesDictionary[colName];//use that instead

            CreateIndex(tableName, colName, sqlDbTypeForColumn, Configuration.ToString(), con, transaction);
        }


        private void CreateIndex(string tableName, string columnName, string sqlDbTypeForColumn, string configurationName, DbConnection con, DbTransaction transaction)
        {
            string notNull = "ALTER TABLE " + tableName + " ALTER COLUMN " + columnName + " " + sqlDbTypeForColumn + " NOT NULL";
            try
            {
                var cmdMakeNotNull = DatabaseCommandHelper.GetCommand(notNull, con, transaction);
                cmdMakeNotNull.CommandTimeout = Timeout;
                cmdMakeNotNull.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Failed when trying to make column " + columnName +
                    " into NotNull for AggregateConfiguration " + configurationName + ".  The SQL that failed was:" +
                    Environment.NewLine + notNull, e);
            }

            string pkCreationSql = "ALTER TABLE " + tableName + " ADD CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED (" + columnName + ")";
            try
            {
                var cmdCreateIndex = DatabaseCommandHelper.GetCommand(pkCreationSql, con, transaction);
                cmdCreateIndex.CommandTimeout = Timeout;
                cmdCreateIndex.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create unique primary key on the results of AggregateConfiguration " + configurationName + ".  The SQL that failed was:" + Environment.NewLine + pkCreationSql, e);
            }
        }
    }
}