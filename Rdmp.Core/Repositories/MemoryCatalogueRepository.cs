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
using ReusableLibraryCode.Settings;
using YamlDotNet.Serialization;
using IContainer = Rdmp.Core.Curation.Data.IContainer;

namespace Rdmp.Core.Repositories
{
    /// <summary>
    /// Memory only implementation of <see cref="ICatalogueRepository"/> in which all objects are created in 
    /// dictionaries and arrays in memory instead of the database.
    /// </summary>
    public class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository, IServerDefaults,ITableInfoCredentialsManager, IAggregateForcedJoinManager, ICohortContainerManager, IFilterManager, IGovernanceManager
    {
        public IAggregateForcedJoinManager AggregateForcedJoinManager { get { return this; } }
        public IGovernanceManager GovernanceManager { get { return this; }}
        public ITableInfoCredentialsManager TableInfoCredentialsManager { get { return this; }}
        public ICohortContainerManager CohortContainerManager { get { return this; }}
        public IEncryptionManager EncryptionManager { get; private set; }

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

        /// <summary>
        /// Path to RSA private key encryption certificate for decrypting encrypted credentials.
        /// </summary>
        public string EncryptionKeyPath { get; protected set; }

        protected virtual Dictionary<PermissableDefaults, IExternalDatabaseServer> Defaults { get; set; } = new ();

        public MemoryCatalogueRepository(IServerDefaults currentDefaults = null)
        {
            JoinManager = new JoinManager(this);
            PluginManager = new PluginManager(this);
            CommentStore = new CommentStoreWithKeywords();
            EncryptionManager = new PasswordEncryptionKeyLocation(this);

            //we need to know what the default servers for stuff are
            foreach (PermissableDefaults value in Enum.GetValues(typeof (PermissableDefaults)))
                if(currentDefaults == null)
                    Defaults.Add(value, null); //we have no defaults to import
                else
                {
                    //we have defaults to import so get the default
                    var defaultServer = currentDefaults.GetDefaultFor(value);

                    //if it's not null we must be able to return it with GetObjectByID
                    if (defaultServer != null)
                        Objects.TryAdd(defaultServer,0);

                    Defaults.Add(value,defaultServer);
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


            base.DeleteFromDatabase(oTableWrapperObject);
            ObscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);
        }

        public DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity
        {
            return Objects.Keys.OfType<T>().Where(r => r.IsReferenceTo(o)).ToArray();
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

        public ExternalDatabaseServer[] GetAllDatabases<T>() where T:IPatcher,new()
        {
            IPatcher p = new T();
            return GetAllObjects<ExternalDatabaseServer>().Where(s=>s.WasCreatedBy(p)).ToArray();
        }

        public IExternalDatabaseServer GetDefaultFor(PermissableDefaults field)
        {
            return Defaults.TryGetValue(field, out var result) ? result : null;
        }

        public void ClearDefault(PermissableDefaults toDelete)
        {
            SetDefault(toDelete, null);
        }

        public virtual void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
        {
            if (Defaults.ContainsKey(toChange))
                Defaults[toChange] = externalDatabaseServer;
            else
                Defaults.Add(toChange, externalDatabaseServer);
        }
        
        public override void Clear()
        {
            base.Clear();

            CohortContainerContents.Clear();
            CredentialsDictionary.Clear();
            ForcedJoins.Clear();
            WhereSubContainers.Clear();
            Defaults.Clear();
        }

        #region ITableInfoToCredentialsLinker
        
        /// <summary>
        /// records which credentials can be used to access the table under which contexts
        /// </summary>
        protected Dictionary<ITableInfo,Dictionary<DataAccessContext, DataAccessCredentials>> CredentialsDictionary { get; set; } = new ();

        public virtual void CreateLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context)
        {
            if(!CredentialsDictionary.ContainsKey(tableInfo))
                CredentialsDictionary.Add(tableInfo,new Dictionary<DataAccessContext, DataAccessCredentials>());

            CredentialsDictionary[tableInfo].Add(context,credentials);

            tableInfo.ClearAllInjections();
        }

        public virtual void BreakLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context)
        {
            if (!CredentialsDictionary.ContainsKey(tableInfo))
                return;

            CredentialsDictionary[tableInfo].Remove(context);

            tableInfo.ClearAllInjections();
        }

        public virtual void BreakAllLinksBetween(DataAccessCredentials credentials, ITableInfo tableInfo)
        {
            if(!CredentialsDictionary.ContainsKey(tableInfo))
                return;

            var toRemove = CredentialsDictionary[tableInfo].Where(v=>Equals(v.Value ,credentials)).Select(k=>k.Key).ToArray();

            foreach (DataAccessContext context in toRemove)
                CredentialsDictionary[tableInfo].Remove(context);
        }

        public DataAccessCredentials GetCredentialsIfExistsFor(ITableInfo tableInfo, DataAccessContext context)
        {
            if(CredentialsDictionary.ContainsKey(tableInfo))
            {
                if (CredentialsDictionary[tableInfo].ContainsKey(context))
                    return CredentialsDictionary[tableInfo][context];

                if (CredentialsDictionary[tableInfo].ContainsKey(DataAccessContext.Any))
                    return CredentialsDictionary[tableInfo][DataAccessContext.Any];
            }
                

            return null;
        }

        public Dictionary<DataAccessContext, DataAccessCredentials> GetCredentialsIfExistsFor(ITableInfo tableInfo)
        {
            if (CredentialsDictionary.ContainsKey(tableInfo))
                return CredentialsDictionary[tableInfo];

            return null;
        }

        public Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(DataAccessCredentials[] allCredentials, ITableInfo[] allTableInfos)
        {
            var toreturn = new Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>>();

            foreach (KeyValuePair<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> kvp in CredentialsDictionary)
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

            foreach (KeyValuePair<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> kvp in CredentialsDictionary)
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
        protected Dictionary<AggregateConfiguration,HashSet<ITableInfo>> ForcedJoins { get; set; } = new ();

        public ITableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
        {
            var everyone = Enumerable.Empty<ITableInfo>();

            // join with everyone? .... what do you mean everyone? EVERYONE!!!!
            if (UserSettings.AlwaysJoinEverything)
                everyone = configuration.Catalogue.GetTableInfosIdeallyJustFromMainTables();

            if (!ForcedJoins.ContainsKey(configuration))
                return everyone.ToArray();

            return ForcedJoins[configuration].Union(everyone).ToArray();
        }

        public virtual void BreakLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
        {
            if (!ForcedJoins.ContainsKey(configuration))
                return;

            ForcedJoins[configuration].Remove(tableInfo);
        }

        public virtual void CreateLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
        {
            if (!ForcedJoins.ContainsKey(configuration))
                ForcedJoins.Add(configuration,new HashSet<ITableInfo>());

            ForcedJoins[configuration].Add(tableInfo);
        }
        #endregion

        #region ICohortContainerLinker
        protected Dictionary<CohortAggregateContainer, HashSet<CohortContainerContent>> CohortContainerContents = new (); 

        public CohortAggregateContainer GetParent(AggregateConfiguration child)
        {
            //if it is in the contents of a container
            if (CohortContainerContents.Any(kvp => kvp.Value.Select(c=>c.Orderable).Contains(child)))
                return CohortContainerContents.Single(kvp => kvp.Value.Select(c => c.Orderable).Contains(child)).Key;

            return null;
        }

        public virtual void Add(CohortAggregateContainer parent,AggregateConfiguration child,int order)
        {
            //make sure we know about the container
            if(!CohortContainerContents.ContainsKey(parent))
                CohortContainerContents.Add(parent, new HashSet<CohortContainerContent>());

            CohortContainerContents[parent].Add(new CohortContainerContent(child, order));
        }

        public virtual void Remove(CohortAggregateContainer parent, AggregateConfiguration child)
        {
            var toRemove = CohortContainerContents[parent].Single(c => c.Orderable.Equals(child));
            CohortContainerContents[parent].Remove(toRemove);
        }

        public class CohortContainerContent
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
            var o = CohortContainerContents.SelectMany(kvp => kvp.Value).SingleOrDefault(c => c.Orderable.Equals(configuration));
            if (o == null)
                return null;

            return o.Order;
        }

        public IOrderable[] GetChildren(CohortAggregateContainer parent)
        {
            if (!CohortContainerContents.ContainsKey(parent))
                return new IOrderable[0];

            return CohortContainerContents[parent].OrderBy(o => o.Order).Select(o => o.Orderable).ToArray();
        }
        
        public CohortAggregateContainer GetParent(CohortAggregateContainer child)
        {
            var match = CohortContainerContents.Where(k => k.Value.Any(hs => Equals(hs.Orderable, child))).Select(kvp=>kvp.Key).ToArray();
            if (match.Length > 0)
                return match.Single();
            
            return null;
        }

        public virtual void Remove(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            CohortContainerContents[parent].RemoveWhere(c => Equals(c.Orderable, child));
        }

        public virtual void SetOrder(AggregateConfiguration child, int newOrder)
        {
            var parent = GetParent(child);

            if (parent != null && CohortContainerContents.ContainsKey(parent))
            {
                var record = CohortContainerContents[parent].SingleOrDefault(o => o.Orderable.Equals(child));
                if (record != null)
                    record.Order = newOrder;
            }
        }

        public virtual void Add(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            if(!CohortContainerContents.ContainsKey(parent))
                CohortContainerContents.Add(parent,new HashSet<CohortContainerContent>());

            CohortContainerContents[parent].Add(new CohortContainerContent(child, child.Order));
        }


        #endregion


        #region IFilterContainerManager

        protected Dictionary<IContainer, HashSet<IContainer>> WhereSubContainers { get; set; } = new ();
        
        public IContainer[] GetSubContainers(IContainer container)
        {
            if(!WhereSubContainers.ContainsKey(container))
                return new IContainer[0];

            return WhereSubContainers[container].ToArray();
        }

        public virtual void MakeIntoAnOrphan(IContainer container)
        {
            foreach (var contents in WhereSubContainers)
                if (contents.Value.Contains(container))
                    contents.Value.Remove(container);
        }

        public IContainer GetParentContainerIfAny(IContainer container)
        {
            var match = WhereSubContainers.Where(k => k.Value.Contains(container)).ToArray();
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
            filter.SaveToDatabase();
        }

        public virtual void AddSubContainer(IContainer parent, IContainer child)
        {
            if (!WhereSubContainers.ContainsKey(parent))
                WhereSubContainers.Add(parent, new HashSet<IContainer>());
            
            WhereSubContainers[parent].Add(child);
        }

        #endregion

        #region IGovernanceManager

        protected Dictionary<GovernancePeriod,HashSet<ICatalogue>> GovernanceCoverage { get; set; } = new ();
        private MEF _mef;

        public virtual void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue)
        {
            if(!GovernanceCoverage.ContainsKey(governancePeriod))
                GovernanceCoverage.Add(governancePeriod,new HashSet<ICatalogue>());

            GovernanceCoverage[governancePeriod].Remove(catalogue);
        }

        public virtual void Link(GovernancePeriod governancePeriod, ICatalogue catalogue)
        {
            if (!GovernanceCoverage.ContainsKey(governancePeriod))
                GovernanceCoverage.Add(governancePeriod, new HashSet<ICatalogue>());

            GovernanceCoverage[governancePeriod].Add(catalogue);
        }

        public Dictionary<int, HashSet<int>> GetAllGovernedCataloguesForAllGovernancePeriods()
        {
            return  GovernanceCoverage.ToDictionary(k => k.Key.ID, v => new HashSet<int>(v.Value.Select(c => c.ID)));
        }

        public IEnumerable<ICatalogue> GetAllGovernedCatalogues(GovernancePeriod governancePeriod)
        {
            if (!GovernanceCoverage.ContainsKey(governancePeriod))
                return Enumerable.Empty<ICatalogue>();

            return GovernanceCoverage[governancePeriod];
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

        public virtual void SetEncryptionKeyPath(string fullName)
        {
            EncryptionKeyPath = fullName;
        }

        public string GetEncryptionKeyPath()
        {
            return EncryptionKeyPath;
        }

        public virtual void DeleteEncryptionKeyPath()
        {
            EncryptionKeyPath = null;
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

            if (oTableWrapperObject is ExtractionInformation extractionInformation)
            {
                extractionInformation.CatalogueItem.ClearAllInjections();
            }

            // when deleting a TableInfo
            if (oTableWrapperObject is TableInfo t)
            {
                // forget about its credentials usages
                CredentialsDictionary.Remove(t);

                foreach(var c in t.ColumnInfos)
                {
                    c.DeleteInDatabase();
                }
            }

            // when deleting a ColumnInfo
            if (oTableWrapperObject is ColumnInfo columnInfo)
            {
                foreach(var ci in Objects.Keys.OfType<CatalogueItem>().Where(ci=>ci.ColumnInfo_ID == columnInfo.ID))
                {
                    ci.ColumnInfo_ID = null;
                    ci.ClearAllInjections();
                    ci.SaveToDatabase();
                }
            }

            if (oTableWrapperObject is CatalogueItem catalogueItem)
            {
                catalogueItem.ExtractionInformation?.DeleteInDatabase();
            }
        }
        /// <inheritdoc/>
        public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName, IMapsDirectlyToDatabaseTable obj)
        {
            return GetAllObjectsWhere<ExtendedProperty>("Name", propertyName)
            .Where(r => r.IsReferenceTo(obj));
        }
        /// <inheritdoc/>
        public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName)
        {
            return GetAllObjectsWhere<ExtendedProperty>("Name", propertyName);
        }
        /// <inheritdoc/>
        public IEnumerable<ExtendedProperty> GetExtendedProperties(IMapsDirectlyToDatabaseTable obj)
        {
            // First pass use SQL to pull only those with ReferencedObjectID ID that match our object
            return GetAllObjectsWhere<ExtendedProperty>("ReferencedObjectID", obj.ID)
                // Second pass make sure the object/repo match
                .Where(r => r.IsReferenceTo(obj));
        }
    }
}