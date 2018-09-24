using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Allows you to create and destroy usage relationships between TableInfos and DataAccessCredentials (under context X).  For example you might have a DataAccessCredentials 
    /// called 'RoutineLoaderAccount' and give tables A,B and C permission to use it under DataAccessContext.DataLoad then have a seperate DataAccessCredentials called 
    /// 'ReadonlyUserAccount' and give tables A,B,C and D permission to use it under DataAccessContext.Any
    /// 
    /// <para></para>
    /// </summary>
    public class TableInfoToCredentialsLinker
    {
        private readonly ICatalogueRepository _repository;

        //returns of querying these links are either 
        //          Dictionary<DataAccessContext,DataAccessCredentials> for all links where there is only one access point 1 - M (1 point many credentials)
        //OR        Dictionary<DataAccessContext, List<TableInfo>>      for all links where the query originates with a credentials M-M (credential is used by many users under many different contexts including potentially used by the same user under two+ different contexts)
        
        //Cannot query Find all links between [collection of access points] and [collection of credentials] yet (probably never need to do this)

        /// <summary>
        /// Creates a new helper class instance for writting/deleting credential usages for <see cref="TableInfo"/> objects in the <paramref name="repository"/>
        /// </summary>
        /// <param name="repository"></param>
        public TableInfoToCredentialsLinker(ICatalogueRepository repository)
        {
            _repository = repository;
        }


        /// <summary>
        /// Declares that the given <paramref name="tableInfo"/> can be accessed using the <paramref name="credentials"/> (username / encrypted password) under the 
        /// usage <paramref name="context"/> 
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="tableInfo"></param>
        /// <param name="context"></param>
        public void CreateLinkBetween(DataAccessCredentials credentials, TableInfo tableInfo,DataAccessContext context)
        {
            using (var con = _repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("INSERT INTO DataAccessCredentials_TableInfo(DataAccessCredentials_ID,TableInfo_ID,Context) VALUES (@cid,@tid,@context)", con.Connection,con.Transaction);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@cid", cmd));
                cmd.Parameters["@cid"].Value = credentials.ID;

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
                cmd.Parameters["@tid"].Value = tableInfo.ID;

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@context", cmd));
                cmd.Parameters["@context"].Value = context;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Removes the right to use passed <paramref name="credentials"/> to access the <paramref name="tableInfo"/> under the <paramref name="context"/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="tableInfo"></param>
        /// <param name="context"></param>
        public void BreakLinkBetween(DataAccessCredentials credentials, TableInfo tableInfo, DataAccessContext context)
        {
            _repository.Delete(
                "DELETE FROM DataAccessCredentials_TableInfo WHERE DataAccessCredentials_ID = @cid AND TableInfo_ID = @tid and Context =@context",
                new Dictionary<string, object>()
                {
                    {"cid", credentials.ID},
                    {"tid", tableInfo.ID},
                    {"context", context},
                });
        }

        /// <summary>
        /// Removes all rights to use the passed <paramref name="credentials"/> to access the <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="tableInfo"></param>
        public void BreakAllLinksBetween(DataAccessCredentials credentials, TableInfo tableInfo)
        {
                 _repository.Delete("DELETE FROM DataAccessCredentials_TableInfo WHERE DataAccessCredentials_ID = @cid AND TableInfo_ID = @tid",
                new Dictionary<string, object>()
                {
                    {"cid", credentials.ID},
                    {"tid", tableInfo.ID}
                },false);
        }

        /// <summary>
        ///  Answers the question, "what is the best credential (if any) to use under the given context"
        /// 
        /// <para>Tries to find a DataAccessCredentials for the supplied TableInfo.  For example you are trying to find a username/pasword to use with the TableInfo when performing
        /// a DataLoad, this method will first return any explicit usage allowances (if there is a credential liscenced for use during DataLoad) if no such credentials exist 
        /// it will then check for a credential which is liscenced for Any usage (can be used for data load, data export etc) and return that else it will return null</para>
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public DataAccessCredentials GetCredentialsIfExistsFor(TableInfo tableInfo, DataAccessContext context)
        {
            int toReturn = -1;

            using (var con = _repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT DataAccessCredentials_ID,Context FROM DataAccessCredentials_TableInfo WHERE TableInfo_ID = @tid and (Context =@context OR Context="+((int)DataAccessContext.Any)+") ", con.Connection,con.Transaction);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
                cmd.Parameters["@tid"].Value = tableInfo.ID;
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@context", cmd));
                cmd.Parameters["@context"].Value = context;

                var r = cmd.ExecuteReader();
                
                //gets the first liscenced usage
                if(r.Read())
                {
                    //there is one 
                    //get it by it's id
                    toReturn = Convert.ToInt32(r["DataAccessCredentials_ID"]);

                    //if the first record is liscenced for Any
                    if (Convert.ToInt32(r["Context"]) == ((int) DataAccessContext.Any))
                    {
                        //see if there is a more specific second record (e.g. DataLoad)
                        if(r.Read())
                            toReturn = Convert.ToInt32(r["DataAccessCredentials_ID"]);
                    }           
                }
            }

            return toReturn != -1 ?_repository.GetObjectByID<DataAccessCredentials>(toReturn):null;
        }


        /// <summary>
        /// Fetches all <see cref="DataAccessContext"/> under which the given <paramref name="credential"/> (username and encrypted password) can be used to access the <see cref="TableInfo"/>.
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public Dictionary<DataAccessContext, DataAccessCredentials> GetAllLinksBetween(TableInfo tableInfo, DataAccessCredentials credential)
        {
            var toReturn = new Dictionary<DataAccessContext, int>();

            using (var con = _repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT DataAccessCredentials_ID,Context FROM DataAccessCredentials_TableInfo WHERE TableInfo_ID = @tid and DataAccessCredentials_ID = @cid", con.Connection,con.Transaction);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
                cmd.Parameters["@tid"].Value = tableInfo.ID;
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@cid", cmd));
                cmd.Parameters["@cid"].Value = credential.ID;

                var r = cmd.ExecuteReader();
                toReturn = GetLinksFromReader(r);
            }

            return toReturn.ToDictionary(k => k.Key, v => _repository.GetObjectByID<DataAccessCredentials>(v.Value));
        }

        /// <summary>
        /// Fetches all <see cref="DataAccessCredentials"/> (username and encrypted password) that can be used to access the <see cref="TableInfo"/> under any
        /// <see cref="DataAccessContext"/>)
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public Dictionary<DataAccessContext,DataAccessCredentials> GetCredentialsIfExistsFor(TableInfo tableInfo)
        {
            var toReturn = new Dictionary<DataAccessContext, int>();

            using (var con = _repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT DataAccessCredentials_ID,Context FROM DataAccessCredentials_TableInfo WHERE TableInfo_ID = @tid", con.Connection,con.Transaction);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
                cmd.Parameters["@tid"].Value = tableInfo.ID;

                var r = cmd.ExecuteReader();
                toReturn = GetLinksFromReader(r);
            }
            return toReturn.ToDictionary(k => k.Key, v => _repository.GetObjectByID<DataAccessCredentials>(v.Value));
        }

        /// <summary>
        /// Returns all credential usage permissions for the given set of <paramref name="allTableInfos"/> and <paramref name="allCredentials"/>
        /// </summary>
        /// <param name="allCredentials"></param>
        /// <param name="allTableInfos"></param>
        /// <returns></returns>
        public Dictionary<TableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(DataAccessCredentials[] allCredentials, TableInfo[] allTableInfos)
        {
            var allCredentialsDictionary = allCredentials.ToDictionary(k => k.ID, v => v);
            var allTablesDictionary = allTableInfos.ToDictionary(k => k.ID, v => v);

            var toReturn = new Dictionary<TableInfo,List<DataAccessCredentialUsageNode>>();

            using (var con = _repository.GetConnection())
            {
                var cmd =
                    DatabaseCommandHelper.GetCommand(
                        "SELECT DataAccessCredentials_ID,TableInfo_ID,Context FROM DataAccessCredentials_TableInfo",
                        con.Connection, con.Transaction);
                var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    //get the context
                    DataAccessContext context = GetContext(r);

                    var tid = Convert.ToInt32(r["TableInfo_ID"]);
                    var cid = Convert.ToInt32(r["DataAccessCredentials_ID"]);
                    
                    //async error? someone created a new credential usage between the allCredentials array being fetched and us reaching this methods execution?
                    if (!allTablesDictionary.ContainsKey(tid) || !allCredentialsDictionary.ContainsKey(cid))
                        continue;//should be super rare never gonna happen

                    var t = allTablesDictionary[tid];
                    var c = allCredentialsDictionary[cid];

                    if(!toReturn.ContainsKey(t))
                        toReturn.Add(t,new List<DataAccessCredentialUsageNode>());

                    toReturn[t].Add(new DataAccessCredentialUsageNode(c,t,context));

                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns all the <see cref="TableInfo"/> that are allowed to use the given <paramref name="credentials"/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public Dictionary<DataAccessContext, List<TableInfo>> GetAllTablesUsingCredentials(DataAccessCredentials credentials)
        {
            Dictionary<DataAccessContext,List<int>> toReturn = new Dictionary<DataAccessContext, List<int>>();

            toReturn.Add(DataAccessContext.Any, new List<int>());
            toReturn.Add(DataAccessContext.DataExport, new List<int>());
            toReturn.Add(DataAccessContext.DataLoad, new List<int>());
            toReturn.Add(DataAccessContext.InternalDataProcessing, new List<int>());
            toReturn.Add(DataAccessContext.Logging, new List<int>());
            
            using (var con = _repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT TableInfo_ID,Context FROM DataAccessCredentials_TableInfo WHERE DataAccessCredentials_ID = @cid", con.Connection,con.Transaction);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@cid", cmd));
                cmd.Parameters["@cid"].Value = credentials.ID;

                var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    //get the context
                    DataAccessContext context = GetContext(r);
                    
                    //add the TableInfo under that context
                    toReturn[context].Add((int)r["TableInfo_ID"]);
                }
            }

            return toReturn.ToDictionary(k => k.Key, v => _repository.GetAllObjectsInIDList<TableInfo>(v.Value).ToList());
        }

        /// <summary>
        /// Helper that returns 1-M results (where there is only one originating TableInfo, if there are more than 1 table info in your SQL query you will get key collisions)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private Dictionary<DataAccessContext, int> GetLinksFromReader(DbDataReader r)
        {
            var toReturn = new Dictionary<DataAccessContext, int>();
            //gets the first liscenced usage
            while (r.Read())
            {
                //get the context
                DataAccessContext context;

                //if it's not a valid context something has gone very wrong
                if (!DataAccessContext.TryParse((string)r["Context"], out context))
                    throw new Exception("Invalid DataAccessContext " + r["Context"]);

                //there is only one credentials per context per table info so dont worry about key collisions they should be impossible
                toReturn.Add(context, Convert.ToInt32(r["DataAccessCredentials_ID"]));
            }

            return toReturn;
        }

        /// <summary>
        /// Returns the existing <see cref="DataAccessCredentials"/> if any which match the unencrypted <paramref name="username"/> and <paramref name="password"/> combination.  Throws
        /// if there are more than 1 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password)
        {
            //see if we already have a record of this user
            DataAccessCredentials[] existingCredentials = _repository.GetAllObjects<DataAccessCredentials>().Where(c => c.Username.Equals(username)).ToArray();

            //found an existing credential that matched on username
            if (existingCredentials.Any())
            {
                //there is one or more existing credential with this username
                var matchingOnPassword = existingCredentials.Where(c => c.GetDecryptedPassword().Equals(password)).ToArray();

                if (matchingOnPassword.Length == 1)
                    return matchingOnPassword.Single();

                if (matchingOnPassword.Length > 1)
                    throw new Exception("Found " + matchingOnPassword.Length + " DataAccessCredentials that matched the supplied username/password - does your database have massive duplication in it?");

                //there are 0 that match on password
                return null;
            }
            else
                //did not find an existing credential that matched on username
                return null;
        }
        private DataAccessContext GetContext(DbDataReader r)
        {
            //if it's not a valid context something has gone very wrong
            DataAccessContext context;
            if (!DataAccessContext.TryParse((string) r["Context"], out context))
                throw new Exception("Invalid DataAccessContext " + r["Context"]);

            return context;
        }

        /// <summary>
        /// Changes the <see cref="DataAccessContext"/> under which the <paramref name="node"/> credentials are usable to access the <paramref name="node"/> table 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="destinationContext"></param>
        public void SetContextFor(DataAccessCredentialUsageNode node, DataAccessContext destinationContext)
        {
            //don't bother if it is already at that context
            if(node.Context == destinationContext)
                return;

            BreakLinkBetween(node.Credentials,node.TableInfo,node.Context);
            CreateLinkBetween(node.Credentials,node.TableInfo,destinationContext);
        }
    }
}
