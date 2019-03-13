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
    public class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository, IServerDefaults,ITableInfoToCredentialsLinker, IAggregateForcedJoin, ICohortContainerLinker
    {
        public IAggregateForcedJoin AggregateForcedJoiner { get { return this; } }
        public ITableInfoToCredentialsLinker TableInfoToCredentialsLinker { get { return this; }}
        public ICohortContainerLinker CohortContainerLinker { get { return this; }}

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

        public bool IsLookupTable(ITableInfo tableInfo)
        {
            return GetAllObjects<Lookup>().Any(l => l.Description.TableInfo.Equals(tableInfo));
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
                v.SoftwareVersion = GetVersion().ToString();
        }

        #region ITableInfoToCredentialsLinker
        
        /// <summary>
        /// records which credentials can be used to access the table under which contexts
        /// </summary>
        readonly Dictionary<TableInfo,Dictionary<DataAccessContext, DataAccessCredentials>> _credentialsDictionary = new Dictionary<TableInfo, Dictionary<DataAccessContext, DataAccessCredentials>>();

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
            var toreturn = new Dictionary<TableInfo, List<DataAccessCredentialUsageNode>>();

            foreach (KeyValuePair<TableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> kvp in _credentialsDictionary)
            {
                toreturn.Add(kvp.Key, new List<DataAccessCredentialUsageNode>());

                foreach (KeyValuePair<DataAccessContext, DataAccessCredentials> forNode in kvp.Value)
                    toreturn[kvp.Key].Add(new DataAccessCredentialUsageNode(forNode.Value, kvp.Key, forNode.Key));
            }

            return toreturn;
        }

        public Dictionary<DataAccessContext, List<TableInfo>> GetAllTablesUsingCredentials(DataAccessCredentials credentials)
        {
            var toreturn = new Dictionary<DataAccessContext, List<TableInfo>>();
            
            //add the keys
            foreach (DataAccessContext context in Enum.GetValues(typeof (DataAccessContext)))
                toreturn.Add(context, new List<TableInfo>());

            foreach (KeyValuePair<TableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> kvp in _credentialsDictionary)
                foreach (KeyValuePair<DataAccessContext, DataAccessCredentials> forNode in kvp.Value)
                    toreturn[forNode.Key].Add(kvp.Key);
            
            return toreturn;
        }

        public DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password)
        {
            return GetAllObjects<DataAccessCredentials>().FirstOrDefault(c=>Equals(c.Name,username) && Equals(c.GetDecryptedPassword(),password));
        }

        #endregion

        #region IAggregateForcedJoin
        readonly Dictionary<AggregateConfiguration,List<TableInfo>> _forcedJoins = new Dictionary<AggregateConfiguration, List<TableInfo>>();

        public TableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
        {
            if (!_forcedJoins.ContainsKey(configuration))
                return new TableInfo[0];

            return _forcedJoins[configuration].ToArray();
        }

        public void BreakLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo)
        {
            if (!_forcedJoins.ContainsKey(configuration))
                return;

            _forcedJoins[configuration].Remove(tableInfo);
        }

        public void CreateLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo)
        {
            if (!_forcedJoins.ContainsKey(configuration))
                _forcedJoins.Add(configuration,new List<TableInfo>());

            _forcedJoins[configuration].Add(tableInfo);
        }
        #endregion

        #region ICohortContainerLinker
        readonly Dictionary<CohortAggregateContainer, List<CohortContainerContent>> _cohortContainerContents = new Dictionary<CohortAggregateContainer, List<CohortContainerContent>>(); 

        public CohortAggregateContainer GetCohortAggregateContainerIfAny(AggregateConfiguration aggregateConfiguration)
        {
            //if it is in the contents of a container
            if (_cohortContainerContents.Any(kvp => kvp.Value.Select(c=>c.Orderable).Contains(aggregateConfiguration)))
                return _cohortContainerContents.Single(kvp => kvp.Value.Select(c => c.Orderable).Contains(aggregateConfiguration)).Key;

            return null;
        }

        public void AddConfigurationToContainer(AggregateConfiguration configuration, CohortAggregateContainer cohortAggregateContainer,int order)
        {
            //make sure we know about the container
            if(!_cohortContainerContents.ContainsKey(cohortAggregateContainer))
                _cohortContainerContents.Add(cohortAggregateContainer,new List<CohortContainerContent>());

            _cohortContainerContents[cohortAggregateContainer].Add(new CohortContainerContent(configuration,order));
        }

        public void RemoveConfigurationFromContainer(AggregateConfiguration configuration,CohortAggregateContainer cohortAggregateContainer)
        {
            var toRemove = _cohortContainerContents[cohortAggregateContainer].Single(c => c.Orderable.Equals(configuration));
            _cohortContainerContents[cohortAggregateContainer].Remove(toRemove);
        }

        private class CohortContainerContent
        {
            public IOrderable Orderable { get; private set; }
            public int Order { get; private set; }

            public CohortContainerContent(IOrderable orderable, int order)
            {
                Orderable = orderable;
                Order = order;
            }
        }
        
        public int? GetOrderIfExistsFor(AggregateConfiguration configuration)
        {
            var o = _cohortContainerContents.SelectMany(kvp => kvp.Value).SingleOrDefault(c => c.Orderable.Equals(configuration));
            if (o == null)
                return null;

            return o.Order;
        }

        public CohortAggregateContainer[] GetSubcontainers(CohortAggregateContainer cohortAggregateContainer)
        {
            if (!_cohortContainerContents.ContainsKey(cohortAggregateContainer))
                return null;
                    
            return _cohortContainerContents[cohortAggregateContainer].Select(c=>c.Orderable).OfType<CohortAggregateContainer>().ToArray();
        }

        public AggregateConfiguration[] GetAggregateConfigurations(CohortAggregateContainer cohortAggregateContainer)
        {
            if (!_cohortContainerContents.ContainsKey(cohortAggregateContainer))
                return null;

            return _cohortContainerContents[cohortAggregateContainer].Select(c => c.Orderable).OfType<AggregateConfiguration>().ToArray();
        }

        #endregion
    }
}