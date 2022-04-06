// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.DataExport;
using Rdmp.Core.Logging;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.Sharing.Dependency;
using Rdmp.Core.Validation.Dependency;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.DataAccess;
using IContainer = Rdmp.Core.Curation.Data.IContainer;

namespace Rdmp.Core.Repositories
{
    /// <summary>
    /// Memory only implementation of <see cref="ICatalogueRepository"/> in which all objects are created in 
    /// dictionaries and arrays in memory instead of the database.
    /// </summary>
    public class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository, IServerDefaults,ITableInfoCredentialsManager, IAggregateForcedJoinManager, ICohortContainerManager, IFilterManager, IGovernanceManager, IEncryptionManager
    {
        public IAggregateForcedJoinManager AggregateForcedJoinManager { get { return this; } }
        public IGovernanceManager GovernanceManager { get { return this; }}
        public ITableInfoCredentialsManager TableInfoCredentialsManager { get { return this; }}
        public ICohortContainerManager CohortContainerManager { get { return this; }}
        public IEncryptionManager EncryptionManager { get { return this; }}

        public IFilterManager FilterManager { get { return this; }}
        public IPluginManager PluginManager { get; private set; }

        public IJoinManager JoinManager { get; private set; }

        public MEF MEF
        {
            get => _mef;
            set
            {
                _mef = value;
                var odf =  ObscureDependencyFinder as CatalogueObscureDependencyFinder;
                var dxm = this as IDataExportRepository;

                if(odf != null && dxm != null)
                    odf.AddOtherDependencyFinderIfNotExists<ValidationXMLObscureDependencyFinder>(new RepositoryProvider(dxm));
            }
        }

        public CommentStore CommentStore { get; set; }

        public IObscureDependencyFinder ObscureDependencyFinder { get; set; }
        public string ConnectionString { get { return null; } }
        public DbConnectionStringBuilder ConnectionStringBuilder { get { return null; } }
        public DiscoveredServer DiscoveredServer { get { return null; }}

        readonly Dictionary<PermissableDefaults, IExternalDatabaseServer> _defaults = new Dictionary<PermissableDefaults, IExternalDatabaseServer>();

        public MemoryCatalogueRepository(IServerDefaults currentDefaults = null)
        {
            JoinManager = new JoinManager(this);
            PluginManager = new PluginManager(this);
            CommentStore = new CommentStoreWithKeywords();

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
                        Objects.TryAdd(defaultServer,0);

                    _defaults.Add(value,defaultServer);
                }

            //start IDs with the maximum id of any default to avoid collisions
            if (Objects.Any())
                NextObjectId = Objects.Keys.Max(o => o.ID);


            var dependencyFinder = new CatalogueObscureDependencyFinder(this);
            
            var dxm = this as IDataExportRepository;

            if(dxm !=  null)
            {
                dependencyFinder.AddOtherDependencyFinderIfNotExists<ObjectSharingObscureDependencyFinder>(new RepositoryProvider(dxm));
                dependencyFinder.AddOtherDependencyFinderIfNotExists<BetweenCatalogueAndDataExportObscureDependencyFinder>(new RepositoryProvider(dxm));
            }

            ObscureDependencyFinder = dependencyFinder;
        }
        

        public LogManager GetDefaultLogManager()
        {
            var server = GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
            if (server == null)
                return null;
            
            return new LogManager(server);
        }

        public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent)
        {
            return GetAllObjects<AnyTableSqlParameter>().Where(o => o.IsReferenceTo(parent));
        }

        public TicketingSystemConfiguration GetTicketingSystem()
        {
            return null;
        }


        public override void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            ObscureDependencyFinder.ThrowIfDeleteDisallowed(oTableWrapperObject);

            if(oTableWrapperObject is DataAccessCredentials creds)
            {
                var users = GetAllTablesUsingCredentials(creds);

                // if there are any contexts where there are any associated tables using this credentials
                if (users.Any(k=>k.Value.Any()))
                    throw new CredentialsInUseException($"Cannot delete credentials {creds} because it is in use by one or more TableInfo objects({string.Join(",",users.SelectMany(u=>u.Value).Distinct().Select(t =>t.Name))})");
            }
            

            base.DeleteFromDatabase(oTableWrapperObject);
            ObscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);
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
            return Objects.OfType<T>().Where(r => r.IsReferenceTo(o)).ToArray();
        }

        public bool IsLookupTable(ITableInfo tableInfo)
        {
            return GetAllObjects<Lookup>().Any(l => l.Description.TableInfo.Equals(tableInfo));
        }

        public Catalogue[] GetAllCataloguesUsing(TableInfo tableInfo)
        {
            return
                GetAllObjects<Catalogue>()
                    .Where(
                        c =>
                            c.CatalogueItems.Any(
                                ci => ci.ColumnInfo_ID != null && ci.ColumnInfo.TableInfo_ID == tableInfo.ID)).ToArray();
        }

        public void UpsertAndHydrate<T>(T toCreate, ShareManager shareManager, ShareDefinition shareDefinition) where T : class, IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public void SetValue(PropertyInfo prop, object value, IMapsDirectlyToDatabaseTable onObject)
        {
            prop.SetValue(onObject,value);
        }

        public ExternalDatabaseServer[] GetAllDatabases<T>() where T:IPatcher,new()
        {
            IPatcher p = new T();
            return GetAllObjects<ExternalDatabaseServer>().Where(s=>s.WasCreatedBy(p)).ToArray();
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
        
        public override void Clear()
        {
            base.Clear();

            _cohortContainerContents.Clear();
            _credentialsDictionary.Clear();
            _forcedJoins.Clear();
            _whereSubContainers.Clear();
            _defaults.Clear();
        }

        #region ITableInfoToCredentialsLinker
        
        /// <summary>
        /// records which credentials can be used to access the table under which contexts
        /// </summary>
        readonly Dictionary<ITableInfo,Dictionary<DataAccessContext, DataAccessCredentials>> _credentialsDictionary = new Dictionary<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>>();

        public void CreateLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context)
        {
            if(!_credentialsDictionary.ContainsKey(tableInfo))
                _credentialsDictionary.Add(tableInfo,new Dictionary<DataAccessContext, DataAccessCredentials>());

            _credentialsDictionary[tableInfo].Add(context,credentials);
        }

        public void BreakLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context)
        {
            if (!_credentialsDictionary.ContainsKey(tableInfo))
                return;

            _credentialsDictionary[tableInfo].Remove(context);
        }

        public void BreakAllLinksBetween(DataAccessCredentials credentials, ITableInfo tableInfo)
        {
            if(!_credentialsDictionary.ContainsKey(tableInfo))
                return;

            var toRemove = _credentialsDictionary[tableInfo].Where(v=>Equals(v.Value ,credentials)).Select(k=>k.Key).ToArray();

            foreach (DataAccessContext context in toRemove)
                _credentialsDictionary[tableInfo].Remove(context);
        }

        public DataAccessCredentials GetCredentialsIfExistsFor(ITableInfo tableInfo, DataAccessContext context)
        {
            if(_credentialsDictionary.ContainsKey(tableInfo))
                if (_credentialsDictionary[tableInfo].ContainsKey(context))
                    return _credentialsDictionary[tableInfo][context];

            return null;
        }

        public Dictionary<DataAccessContext, DataAccessCredentials> GetCredentialsIfExistsFor(ITableInfo tableInfo)
        {
            if (_credentialsDictionary.ContainsKey(tableInfo))
                return _credentialsDictionary[tableInfo];

            return null;
        }

        public Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(DataAccessCredentials[] allCredentials, ITableInfo[] allTableInfos)
        {
            var toreturn = new Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>>();

            foreach (KeyValuePair<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> kvp in _credentialsDictionary)
            {
                toreturn.Add(kvp.Key, new List<DataAccessCredentialUsageNode>());

                foreach (KeyValuePair<DataAccessContext, DataAccessCredentials> forNode in kvp.Value)
                    toreturn[kvp.Key].Add(new DataAccessCredentialUsageNode(forNode.Value, kvp.Key, forNode.Key));
            }

            return toreturn;
        }

        public Dictionary<DataAccessContext, List<ITableInfo>> GetAllTablesUsingCredentials(DataAccessCredentials credentials)
        {
            var toreturn = new Dictionary<DataAccessContext, List<ITableInfo>>();
            
            //add the keys
            foreach (DataAccessContext context in Enum.GetValues(typeof (DataAccessContext)))
                toreturn.Add(context, new List<ITableInfo>());

            foreach (KeyValuePair<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> kvp in _credentialsDictionary)
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
        readonly Dictionary<AggregateConfiguration,List<ITableInfo>> _forcedJoins = new Dictionary<AggregateConfiguration, List<ITableInfo>>();

        public ITableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
        {
            if (!_forcedJoins.ContainsKey(configuration))
                return new TableInfo[0];

            return _forcedJoins[configuration].ToArray();
        }

        public void BreakLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
        {
            if (!_forcedJoins.ContainsKey(configuration))
                return;

            _forcedJoins[configuration].Remove(tableInfo);
        }

        public void CreateLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
        {
            if (!_forcedJoins.ContainsKey(configuration))
                _forcedJoins.Add(configuration,new List<ITableInfo>());

            _forcedJoins[configuration].Add(tableInfo);
        }
        #endregion

        #region ICohortContainerLinker
        readonly Dictionary<CohortAggregateContainer, HashSet<CohortContainerContent>> _cohortContainerContents = new Dictionary<CohortAggregateContainer, HashSet<CohortContainerContent>>(); 

        public CohortAggregateContainer GetParent(AggregateConfiguration child)
        {
            //if it is in the contents of a container
            if (_cohortContainerContents.Any(kvp => kvp.Value.Select(c=>c.Orderable).Contains(child)))
                return _cohortContainerContents.Single(kvp => kvp.Value.Select(c => c.Orderable).Contains(child)).Key;

            return null;
        }

        public void Add(CohortAggregateContainer parent,AggregateConfiguration child,int order)
        {
            //make sure we know about the container
            if(!_cohortContainerContents.ContainsKey(parent))
                _cohortContainerContents.Add(parent, new HashSet<CohortContainerContent>());

            _cohortContainerContents[parent].Add(new CohortContainerContent(child, order));
        }

        public void Remove(CohortAggregateContainer parent, AggregateConfiguration child)
        {
            var toRemove = _cohortContainerContents[parent].Single(c => c.Orderable.Equals(child));
            _cohortContainerContents[parent].Remove(toRemove);
        }

        private class CohortContainerContent
        {
            public IOrderable Orderable { get; private set; }
            public int Order { get; set; }

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

        public IOrderable[] GetChildren(CohortAggregateContainer parent)
        {
            if (!_cohortContainerContents.ContainsKey(parent))
                return new IOrderable[0];

            return _cohortContainerContents[parent].OrderBy(o => o.Order).Select(o => o.Orderable).ToArray();
        }
        
        public CohortAggregateContainer GetParent(CohortAggregateContainer child)
        {
            var match = _cohortContainerContents.Where(k => k.Value.Any(hs => Equals(hs.Orderable, child))).Select(kvp=>kvp.Key).ToArray();
            if (match.Length > 0)
                return match.Single();
            
            return null;
        }

        public void Remove(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            _cohortContainerContents[parent].RemoveWhere(c => Equals(c.Orderable, child));
        }

        public void SetOrder(AggregateConfiguration child, int newOrder)
        {
            var parent = GetParent(child);

            if (parent != null && _cohortContainerContents.ContainsKey(parent))
            {
                var record = _cohortContainerContents[parent].SingleOrDefault(o => o.Orderable.Equals(child));
                if (record != null)
                    record.Order = newOrder;
            }
        }

        public void Add(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            if(!_cohortContainerContents.ContainsKey(parent))
                _cohortContainerContents.Add(parent,new HashSet<CohortContainerContent>());

            _cohortContainerContents[parent].Add(new CohortContainerContent(child, child.Order));
        }


        #endregion


        #region IFilterContainerManager

        readonly Dictionary<IContainer, HashSet<IContainer>> _whereSubContainers = new Dictionary<IContainer, HashSet<IContainer>>();
        
        public IContainer[] GetSubContainers(IContainer container)
        {
            if(!_whereSubContainers.ContainsKey(container))
                return new IContainer[0];

            return _whereSubContainers[container].ToArray();
        }

        public void MakeIntoAnOrphan(IContainer container)
        {
            foreach (var contents in _whereSubContainers)
                if (contents.Value.Contains(container))
                    contents.Value.Remove(container);
        }

        public IContainer GetParentContainerIfAny(IContainer container)
        {
            var match = _whereSubContainers.Where(k => k.Value.Contains(container)).ToArray();
            if (match.Length != 0)
                return match[0].Key;

            return null;
        }

        public IFilter[] GetFilters(IContainer container)
        {
            return GetAllObjects<IFilter>().Where(f =>!(f is ExtractionFilter) && f.FilterContainer_ID == container.ID).ToArray();
        }

        public void AddChild(IContainer container, IFilter filter)
        {
            filter.FilterContainer_ID = container.ID;
        }

        public void AddSubContainer(IContainer parent, IContainer child)
        {
            if (!_whereSubContainers.ContainsKey(parent))
                _whereSubContainers.Add(parent, new HashSet<IContainer>());
            
            _whereSubContainers[parent].Add(child);
        }

        #endregion

        #region IGovernanceManager

        readonly Dictionary<GovernancePeriod,HashSet<ICatalogue>> _governanceCoverage = new Dictionary<GovernancePeriod, HashSet<ICatalogue>>();
        private MEF _mef;

        public void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue)
        {
            if(!_governanceCoverage.ContainsKey(governancePeriod))
                _governanceCoverage.Add(governancePeriod,new HashSet<ICatalogue>());

            _governanceCoverage[governancePeriod].Remove(catalogue);
        }

        public void Link(GovernancePeriod governancePeriod, ICatalogue catalogue)
        {
            if (!_governanceCoverage.ContainsKey(governancePeriod))
                _governanceCoverage.Add(governancePeriod, new HashSet<ICatalogue>());

            _governanceCoverage[governancePeriod].Add(catalogue);
        }

        public Dictionary<int, HashSet<int>> GetAllGovernedCataloguesForAllGovernancePeriods()
        {
            return  _governanceCoverage.ToDictionary(k => k.Key.ID, v => new HashSet<int>(v.Value.Select(c => c.ID)));
        }

        public IEnumerable<ICatalogue> GetAllGovernedCatalogues(GovernancePeriod governancePeriod)
        {
            if (!_governanceCoverage.ContainsKey(governancePeriod))
                return Enumerable.Empty<ICatalogue>();

            return _governanceCoverage[governancePeriod];
        }

        #endregion

        #region IEncryptionManager
        public IEncryptStrings GetEncrypter()
        {
            return new SimpleStringValueEncryption(null);
        }
        public void ClearAllInjections()
        {

        }

        public void SetEncryptionKeyPath(string fullName)
        {
            throw new NotImplementedException();
        }

        public string GetEncryptionKeyPath()
        {
            throw new NotImplementedException();
        }

        public void DeleteEncryptionKeyPath()
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginNewTransaction()
        {
            return new EmptyDisposeable();
        }

        public void EndTransaction(bool commit)
        {

        }
        #endregion
        protected override void CascadeDeletes(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            base.CascadeDeletes(oTableWrapperObject);

            if (oTableWrapperObject is Catalogue catalogue)
            {
                foreach (var ci in catalogue.CatalogueItems)
                {
                    ci.DeleteInDatabase();
                }
            }

            // when deleting a TableInfo
            if(oTableWrapperObject is TableInfo t)
            {
                // forget about its credentials usages
                _credentialsDictionary.Remove(t);
            }

            if (oTableWrapperObject is CatalogueItem catalogueItem)
            {
                catalogueItem.ExtractionInformation?.DeleteInDatabase();
            }
        }
    }
}