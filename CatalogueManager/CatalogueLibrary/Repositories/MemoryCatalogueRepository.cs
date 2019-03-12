using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.Data.Referencing;
using CatalogueLibrary.Nodes;
using FAnsi.Connections;
using FAnsi.Discovery;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Repositories
{
    class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository, IServerDefaults,ITableInfoToCredentialsLinker
    {
        public AggregateForcedJoin AggregateForcedJoiner { get; set; }
        public ITableInfoToCredentialsLinker TableInfoToCredentialsLinker { get { return this; }}
        public IEncryptStrings GetEncrypter()
        {
            return new SimpleStringValueEncryption(null);
        }

        public JoinInfoFinder JoinInfoFinder { get; set; }
        public MEF MEF { get; set; }
        public CommentStore CommentStore { get; private set; }

        public IObscureDependencyFinder ObscureDependencyFinder { get; set; }
        public string ConnectionString { get { return null; } }
        public DbConnectionStringBuilder ConnectionStringBuilder { get { return null; } }
        public DiscoveredServer DiscoveredServer { get { return null; }}

        readonly Dictionary<PermissableDefaults, IExternalDatabaseServer> _defaults = new Dictionary<PermissableDefaults, IExternalDatabaseServer>();

        public MemoryCatalogueRepository(IServerDefaults currentDefaults = null) :
            base(typeof(MemoryCatalogueRepository).Assembly.GetTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
        {
            //we need to know what the default servers for stuff are
            foreach (PermissableDefaults value in Enum.GetValues(typeof (PermissableDefaults)))
                if(currentDefaults == null)
                    _defaults.Add(value, null); //we have no defaults to import
                else
                {
                    //we have defaults to import so get the default
                    var defaultServer = currentDefaults.GetDefaultFor(value);

                    //if it's not null we must be able to return it with GetObjectByID
                    if (defaultServer != null)
                        Objects[typeof (ExternalDatabaseServer)].Add(defaultServer);

                    _defaults.Add(value,defaultServer);
                }

            //start IDs with the maximum id of any default to avoid collisions
            var allObjs = Objects.SelectMany(kvp => kvp.Value).ToList();
            if (allObjs.Any())
                NextObjectId = allObjs.Max(o => o.ID);
        }
        

        public IManagedConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        public IManagedConnection BeginNewTransactedConnection()
        {
            throw new NotImplementedException();
        }

        public void EndTransactedConnection(bool commit)
        {
            throw new NotImplementedException();
        }

        public void ClearUpdateCommandCache()
        {
            throw new NotImplementedException();
        }

        public int? ObjectToNullableInt(object o)
        {
            throw new NotImplementedException();
        }

        public DateTime? ObjectToNullableDateTime(object o)
        {
            throw new NotImplementedException();
        }

        public void TestConnection()
        {
            throw new NotImplementedException();
        }

        public bool SupportsObjectType(Type type)
        {
            throw new NotImplementedException();
        }

        public int? GetOrderIfExistsFor(AggregateConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public LogManager GetDefaultLogManager()
        {
            throw new NotImplementedException();
        }

        public Catalogue[] GetAllCatalogues(bool includeDeprecatedCatalogues = false)
        {
            throw new NotImplementedException();
        }

        public Catalogue[] GetAllCataloguesWithAtLeastOneExtractableItem()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent)
        {
            return GetAllObjects<AnyTableSqlParameter>().Where(o => o.IsReferenceTo(parent));
        }

        public TicketingSystemConfiguration GetTicketingSystem()
        {
            throw new NotImplementedException();
        }

        public void PopulateInsertCommandValuesWithCurrentState(DbCommand insertCommand,
            IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            throw new NotImplementedException();
        }

        public T CloneObjectInTable<T>(T oToClone, TableRepository destinationRepository) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public T[] GetAllObjectsWhere<T>(string whereSQL, Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity
        {
            throw new NotImplementedException();
        }

        public IServerDefaults GetServerDefaults()
        {
            return this;
        }
        
        public IExternalDatabaseServer GetDefaultFor(PermissableDefaults field)
        {
            return _defaults[field];
        }

        public void ClearDefault(PermissableDefaults toDelete)
        {
            _defaults[toDelete] = null;
        }

        public void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
        {
            _defaults[toChange] = externalDatabaseServer;
        }

        public override void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters)
        {
            base.InsertAndHydrate(toCreate, constructorParameters);

            var v = toCreate as VersionedDatabaseEntity;
            if(v != null)
                v.SoftwareVersion = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion;
        }

        #region ITableInfoToCredentialsLinker
        
        /// <summary>
        /// records which credentials can be used to access the table under which contexts
        /// </summary>
        Dictionary<TableInfo,Dictionary<DataAccessContext, DataAccessCredentials>> _credentialsDictionary = new Dictionary<TableInfo, Dictionary<DataAccessContext, DataAccessCredentials>>();

        public void CreateLinkBetween(DataAccessCredentials credentials, TableInfo tableInfo, DataAccessContext context)
        {
            if(!_credentialsDictionary.ContainsKey(tableInfo))
                _credentialsDictionary.Add(tableInfo,new Dictionary<DataAccessContext, DataAccessCredentials>());

            _credentialsDictionary[tableInfo].Add(context,credentials);
        }

        public void BreakLinkBetween(DataAccessCredentials credentials, TableInfo tableInfo, DataAccessContext context)
        {
            if (!_credentialsDictionary.ContainsKey(tableInfo))
                return;

            _credentialsDictionary[tableInfo].Remove(context);
        }

        public void BreakAllLinksBetween(DataAccessCredentials credentials, TableInfo tableInfo)
        {
            if(!_credentialsDictionary.ContainsKey(tableInfo))
                return;

            var toRemove = _credentialsDictionary[tableInfo].Where(v=>Equals(v.Value ,credentials)).Select(k=>k.Key).ToArray();

            foreach (DataAccessContext context in toRemove)
                _credentialsDictionary[tableInfo].Remove(context);
        }

        public DataAccessCredentials GetCredentialsIfExistsFor(TableInfo tableInfo, DataAccessContext context)
        {
            if(_credentialsDictionary.ContainsKey(tableInfo))
                if (_credentialsDictionary[tableInfo].ContainsKey(context))
                    return _credentialsDictionary[tableInfo][context];

            return null;
        }

        public Dictionary<DataAccessContext, DataAccessCredentials> GetCredentialsIfExistsFor(TableInfo tableInfo)
        {
            if (_credentialsDictionary.ContainsKey(tableInfo))
                return _credentialsDictionary[tableInfo];

            return null;
        }

        public Dictionary<TableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(DataAccessCredentials[] allCredentials, TableInfo[] allTableInfos)
        {
            throw new NotImplementedException();
        }

        public Dictionary<DataAccessContext, List<TableInfo>> GetAllTablesUsingCredentials(DataAccessCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password)
        {
            return GetAllObjects<DataAccessCredentials>().FirstOrDefault(c=>Equals(c.Name,username) && Equals(c.GetDecryptedPassword(),password));
        }

        public void SetContextFor(DataAccessCredentialUsageNode node, DataAccessContext destinationContext)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}