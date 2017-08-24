using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using QueryCaching.Aggregation.Arguments;
using QueryCaching.Database;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DataTableExtension;

namespace QueryCaching.Aggregation
{
    public class CachedAggregateConfigurationResultsManager
    {
        private readonly DiscoveredServer _server;
        private DiscoveredDatabase _database;

        public static Type DbAssembly = typeof (Class1);

        public CachedAggregateConfigurationResultsManager(IExternalDatabaseServer server)
        {
            _server = DataAccessPortal.GetInstance().ExpectServer(server, DataAccessContext.InternalDataProcessing);
            _database = DataAccessPortal.GetInstance().ExpectDatabase(server, DataAccessContext.InternalDataProcessing);
        }

        public const string CachingPrefix = "/*Cached:";

        public IHasFullyQualifiedNameToo GetLatestResultsTableUnsafe(AggregateConfiguration configuration,AggregateOperation operation)
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                var r = DatabaseCommandHelper.GetCommand("Select TableName from [CachedAggregateConfigurationResults] WHERE AggregateConfiguration_ID = " + configuration.ID + " AND Operation = '" +operation + "'" , con).ExecuteReader();
                
                if (r.Read())
                {
                    string tableName =  r["TableName"].ToString();
                    return _database.ExpectTable(tableName);
                }
            }

            return null;
        }

        public IHasFullyQualifiedNameToo GetLatestResultsTable(AggregateConfiguration configuration, AggregateOperation operation, string currentSql)
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                var cmd = DatabaseCommandHelper.GetCommand("Select TableName,SqlExecuted from [CachedAggregateConfigurationResults] WHERE AggregateConfiguration_ID = " + configuration.ID + " AND Operation = '" + operation + "'", con);
                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    if (IsMatchOnSqlExecuted(r, currentSql))
                    {
                        string tableName = r["TableName"].ToString();
                        return _database.ExpectTable(tableName);
                    }
                    else
                        return null; //this means that there was outdated SQL, we could show this to user at some point
                }
            }

            return null;
        }

        private bool IsMatchOnSqlExecuted(DbDataReader r, string currentSql)
        {
            //replace all whitespace with single whitespace 
            string standardisedDatabaseSql = Regex.Replace(r["SqlExecuted"].ToString(), @"\s+", " ");
            string standardisedUsersSql = Regex.Replace(currentSql,@"\s+"," ");

            return standardisedDatabaseSql.ToLower().Trim().Equals(standardisedUsersSql.ToLower().Trim());
        }

        public void CommitResults(CacheCommitArguments arguments)
        {
            var configuration = arguments.Configuration;
            var operation = arguments.Operation;

            DeleteCacheEntryIfAny(configuration, operation);

            using (var con = _server.GetConnection())
            {
                con.Open();
                var transaction = con.BeginTransaction();

                string nameWeWillGiveTableInCache = operation + "_AggregateConfiguration" + configuration.ID;

                //it already has a name
                if(!string.IsNullOrWhiteSpace(arguments.Results.TableName))
                    //if it isn't the one we want then user has named his table something so complain
                    if (!arguments.Results.TableName.Equals(nameWeWillGiveTableInCache))
                        throw new Exception("Cannot commit data table because you have given it a name (" + arguments.Results.TableName +") - we want to rename it but I'm guessing you'd be surprised if it came back with a different name on it");
                    
                //either it has no name or it already has name we want so its ok
                arguments.Results.TableName = nameWeWillGiveTableInCache;

                DataTableHelper helper = new DataTableHelper(arguments.Results);
                string tableName;

                //add explicit types
                if(arguments.ExplicitTypesDictionary != null)
                    foreach (KeyValuePair<string, string> kvp in arguments.ExplicitTypesDictionary)
                        helper.ExplicitWriteTypes.Add(kvp.Key, kvp.Value);
                
                helper.UploadFileToConnection(_server, con, out tableName, transaction,true,false);


                var cmdCreateNew =
                    DatabaseCommandHelper.GetCommand(
                        "INSERT INTO [CachedAggregateConfigurationResults] (Committer,AggregateConfiguration_ID,SqlExecuted,Operation,TableName) Values (@Committer,@AggregateConfiguration_ID,@SqlExecuted,@Operation,@TableName)", con,transaction);


                cmdCreateNew.Parameters.Add(DatabaseCommandHelper.GetParameter("@Committer", cmdCreateNew));
                cmdCreateNew.Parameters["@Committer"].Value = Environment.UserName;

                cmdCreateNew.Parameters.Add(DatabaseCommandHelper.GetParameter("@AggregateConfiguration_ID", cmdCreateNew));
                cmdCreateNew.Parameters["@AggregateConfiguration_ID"].Value = configuration.ID;

                cmdCreateNew.Parameters.Add(DatabaseCommandHelper.GetParameter("@SqlExecuted", cmdCreateNew));
                cmdCreateNew.Parameters["@SqlExecuted"].Value = arguments.SQL.Trim();

                cmdCreateNew.Parameters.Add(DatabaseCommandHelper.GetParameter("@Operation", cmdCreateNew));
                cmdCreateNew.Parameters["@Operation"].Value = operation.ToString();

                cmdCreateNew.Parameters.Add(DatabaseCommandHelper.GetParameter("@TableName", cmdCreateNew));
                cmdCreateNew.Parameters["@TableName"].Value = tableName;

                cmdCreateNew.ExecuteNonQuery();

                arguments.CommitTableDataCompleted(tableName,helper,con,transaction);
             
                transaction.Commit();
            }
        }

        public void DeleteCacheEntryIfAny(AggregateConfiguration configuration, AggregateOperation operation)
        {
            var table = GetLatestResultsTableUnsafe(configuration, operation);

            if(table != null)

            using (var con = _server.GetConnection())
            {
                con.Open();

                //drop the data
                _database.ExpectTable(table.GetRuntimeName()).Drop();
                
                //delete the record!
                int deletedRows = DatabaseCommandHelper.GetCommand("DELETE FROM [CachedAggregateConfigurationResults] WHERE AggregateConfiguration_ID = "+configuration.ID+" AND Operation = '"+operation+"'", con).ExecuteNonQuery();

                if(deletedRows != 1)
                    throw new Exception("Expected exactly 1 record in CachedAggregateConfigurationResults to be deleted when erasing its record of operation " + operation + " but there were " + deletedRows +" affected records");
            }
        }
    }
}