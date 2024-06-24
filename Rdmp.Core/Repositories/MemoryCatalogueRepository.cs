// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.DataExport;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Sharing.Dependency;
using Rdmp.Core.Validation.Dependency;
using IContainer = Rdmp.Core.Curation.Data.IContainer;

namespace Rdmp.Core.Repositories;

/// <summary>
/// Memory only implementation of <see cref="ICatalogueRepository"/> in which all objects are created in
/// dictionaries and arrays in memory instead of the database.
/// </summary>
public class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository, ITableInfoCredentialsManager,
    IAggregateForcedJoinManager, ICohortContainerManager, IFilterManager, IGovernanceManager
{
    public IAggregateForcedJoinManager AggregateForcedJoinManager => this;
    public IGovernanceManager GovernanceManager => this;
    public ITableInfoCredentialsManager TableInfoCredentialsManager => this;
    public ICohortContainerManager CohortContainerManager => this;
    public IEncryptionManager EncryptionManager { get; private set; }

    public IFilterManager FilterManager => this;

    public IJoinManager JoinManager { get; private set; }

    public CommentStore CommentStore { get; set; }

    private IObscureDependencyFinder _odf;

    public IObscureDependencyFinder ObscureDependencyFinder
    {
        get => _odf;
        set
        {
            _odf = value;
            if (_odf is CatalogueObscureDependencyFinder catFinder &&
                this is IDataExportRepository dataExportRepository)
                catFinder.AddOtherDependencyFinderIfNotExists<ValidationXMLObscureDependencyFinder>(
                    new RepositoryProvider(dataExportRepository));
        }
    }

    /// <summary>
    /// Path to RSA private key encryption certificate for decrypting encrypted credentials.
    /// </summary>
    public string EncryptionKeyPath { get; protected set; }

    protected virtual Dictionary<PermissableDefaults, IExternalDatabaseServer> Defaults { get; set; } = new();

    public MemoryCatalogueRepository(IServerDefaults currentDefaults = null)
    {
        JoinManager = new JoinManager(this);
        CommentStore = new CommentStoreWithKeywords();
        EncryptionManager = new PasswordEncryptionKeyLocation(this);

        //we need to know what the default servers for stuff are
        foreach (PermissableDefaults value in Enum.GetValues(typeof(PermissableDefaults)))
            if (currentDefaults == null)
            {
                Defaults.Add(value, null); //we have no defaults to import
            }
            else
            {
                //we have defaults to import so get the default
                var defaultServer = currentDefaults.GetDefaultFor(value);

                //if it's not null we must be able to return it with GetObjectByID
                if (defaultServer != null)
                    Objects.TryAdd(defaultServer, 0);

                Defaults.Add(value, defaultServer);
            }

        //start IDs with the maximum id of any default to avoid collisions
        if (Objects.Any())
            NextObjectId = Objects.Keys.Max(o => o.ID);


        var dependencyFinder = new CatalogueObscureDependencyFinder(this);

        if (this is IDataExportRepository dxm)
        {
            dependencyFinder.AddOtherDependencyFinderIfNotExists<ObjectSharingObscureDependencyFinder>(
                new RepositoryProvider(dxm));
            dependencyFinder.AddOtherDependencyFinderIfNotExists<BetweenCatalogueAndDataExportObscureDependencyFinder>(
                new RepositoryProvider(dxm));
        }

        ObscureDependencyFinder = dependencyFinder;
    }


    public LogManager GetDefaultLogManager()
    {
        var server = GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        return server == null ? null : new LogManager(server);
    }

    public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent)
    {
        return GetAllObjects<AnyTableSqlParameter>().Where(o => o.IsReferenceTo(parent));
    }

    public TicketingSystemConfiguration GetTicketingSystem() => null;


    public override void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        ObscureDependencyFinder.ThrowIfDeleteDisallowed(oTableWrapperObject);


        base.DeleteFromDatabase(oTableWrapperObject);
        ObscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);
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

    public ExternalDatabaseServer[] GetAllDatabases<T>() where T : IPatcher, new()
    {
        IPatcher p = new T();
        return GetAllObjects<ExternalDatabaseServer>().Where(s => s.WasCreatedBy(p)).ToArray();
    }

    public IExternalDatabaseServer GetDefaultFor(PermissableDefaults field) =>
        Defaults.GetValueOrDefault(field);

    public void ClearDefault(PermissableDefaults toDelete)
    {
        SetDefault(toDelete, null);
    }

    public virtual void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
    {
        Defaults[toChange] = externalDatabaseServer;
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
    protected Dictionary<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>> CredentialsDictionary
    {
        get;
        set;
    } =
        new();

    public virtual void CreateLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo,
        DataAccessContext context)
    {
        if (!CredentialsDictionary.ContainsKey(tableInfo))
            CredentialsDictionary.Add(tableInfo, new Dictionary<DataAccessContext, DataAccessCredentials>());

        CredentialsDictionary[tableInfo].Add(context, credentials);

        tableInfo.ClearAllInjections();
    }

    public virtual void BreakLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo,
        DataAccessContext context)
    {
        if (!CredentialsDictionary.TryGetValue(tableInfo, out var credentialsMap))
            return;

        credentialsMap.Remove(context);

        tableInfo.ClearAllInjections();
    }

    public virtual void BreakAllLinksBetween(DataAccessCredentials credentials, ITableInfo tableInfo)
    {
        if (!CredentialsDictionary.TryGetValue(tableInfo, out var credentialsMap))
            return;

        var toRemove = credentialsMap.Where(v => Equals(v.Value, credentials)).Select(k => k.Key)
            .ToArray();

        foreach (var context in toRemove)
            credentialsMap.Remove(context);
    }

    public DataAccessCredentials GetCredentialsIfExistsFor(ITableInfo tableInfo, DataAccessContext context)
    {
        if (!CredentialsDictionary.TryGetValue(tableInfo, out var credentialsList)) return null;
        if (credentialsList.TryGetValue(context, out var credentials))
            return credentials;
        return credentialsList.TryGetValue(DataAccessContext.Any, out credentials) ? credentials : null;
    }

    public Dictionary<DataAccessContext, DataAccessCredentials> GetCredentialsIfExistsFor(ITableInfo tableInfo) =>
        CredentialsDictionary.GetValueOrDefault(tableInfo);

    public Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(
        DataAccessCredentials[] allCredentials, ITableInfo[] allTableInfos)
    {
        var toreturn = new Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>>();

        foreach (var kvp in CredentialsDictionary)
        {
            toreturn.Add(kvp.Key, new List<DataAccessCredentialUsageNode>());

            foreach (var forNode in kvp.Value)
                toreturn[kvp.Key].Add(new DataAccessCredentialUsageNode(forNode.Value, kvp.Key, forNode.Key));
        }

        return toreturn;
    }

    public Dictionary<DataAccessContext, List<ITableInfo>> GetAllTablesUsingCredentials(
        DataAccessCredentials credentials)
    {
        var toreturn = Enum.GetValues(typeof(DataAccessContext)).Cast<DataAccessContext>()
            .ToDictionary(context => context, _ => new List<ITableInfo>());

        //add the keys

        foreach (var kvp in CredentialsDictionary)
            foreach (var forNode in kvp.Value)
                toreturn[forNode.Key].Add(kvp.Key);

        return toreturn;
    }

    public DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password)
    {
        return GetAllObjects<DataAccessCredentials>().FirstOrDefault(c =>
            Equals(c.Username, username) && Equals(c.GetDecryptedPassword(), password));
    }

    #endregion

    #region IAggregateForcedJoin

    protected Dictionary<AggregateConfiguration, HashSet<ITableInfo>> ForcedJoins { get; set; } = new();

    public ITableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
    {
        var everyone =
            // join with everyone? .... what do you mean everyone? EVERYONE!!!!
            UserSettings.AlwaysJoinEverything
                ? configuration.Catalogue.GetTableInfosIdeallyJustFromMainTables()
                : Enumerable.Empty<ITableInfo>();

        return !ForcedJoins.TryGetValue(configuration, out var forced) ? everyone.ToArray()
            : forced.Union(everyone).ToArray();
    }

    public virtual void BreakLinkBetween([NotNull] AggregateConfiguration configuration, ITableInfo tableInfo)
    {
        if (ForcedJoins.TryGetValue(configuration, out var value)) value.Remove(tableInfo);
    }

    public virtual void CreateLinkBetween([NotNull] AggregateConfiguration configuration, ITableInfo tableInfo)
    {
        if (!ForcedJoins.TryGetValue(configuration,out var forced))
            ForcedJoins.Add(configuration, forced=new HashSet<ITableInfo>());

        forced.Add(tableInfo);
    }

    #endregion

    #region ICohortContainerLinker

    protected Dictionary<CohortAggregateContainer, HashSet<CohortContainerContent>> CohortContainerContents =
        new();

    public CohortAggregateContainer GetParent(AggregateConfiguration child)
    {
        //if it is in the contents of a container
        return CohortContainerContents.Any(kvp => kvp.Value.Select(c => c.Orderable).Contains(child))
            ? CohortContainerContents.Single(kvp => kvp.Value.Select(c => c.Orderable).Contains(child)).Key
            : null;
    }

    public virtual void Add(CohortAggregateContainer parent, AggregateConfiguration child, int order)
    {
        //make sure we know about the container
        if (!CohortContainerContents.ContainsKey(parent))
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
        var o = CohortContainerContents.SelectMany(kvp => kvp.Value)
            .SingleOrDefault(c => c.Orderable.Equals(configuration));

        return o?.Order;
    }

    public IOrderable[] GetChildren(CohortAggregateContainer parent)
    {
        return !CohortContainerContents.TryGetValue(parent, out var cohortContainerContents) ?
            Array.Empty<IOrderable>()
            : cohortContainerContents.OrderBy(static o => o.Order).Select(static o => o.Orderable).ToArray();
    }

    public CohortAggregateContainer GetParent(CohortAggregateContainer child)
    {
        var match = CohortContainerContents.Where(k => k.Value.Any(hs => Equals(hs.Orderable, child)))
            .Select(static kvp => kvp.Key).ToArray();
        return match.Length > 0 ? match.Single() : null;
    }

    public virtual void Remove(CohortAggregateContainer parent, CohortAggregateContainer child)
    {
        CohortContainerContents[parent].RemoveWhere(c => Equals(c.Orderable, child));
    }

    public virtual void SetOrder(AggregateConfiguration child, int newOrder)
    {
        var parent = GetParent(child);

        if (parent != null && CohortContainerContents.TryGetValue(parent, out var cohortContainerContent))
        {
            var record = cohortContainerContent.SingleOrDefault(o => o.Orderable.Equals(child));
            if (record != null)
                record.Order = newOrder;
        }
    }

    public virtual void Add(CohortAggregateContainer parent, CohortAggregateContainer child)
    {
        if (!CohortContainerContents.ContainsKey(parent))
            CohortContainerContents.Add(parent, new HashSet<CohortContainerContent>());

        CohortContainerContents[parent].Add(new CohortContainerContent(child, child.Order));
    }

    #endregion


    #region IFilterContainerManager

    protected Dictionary<IContainer, HashSet<IContainer>> WhereSubContainers { get; set; } = new();

    [NotNull]
    public IContainer[] GetSubContainers(IContainer container) =>
        !WhereSubContainers.TryGetValue(container, out var containers)
            ? Array.Empty<IContainer>()
        : containers.ToArray();

    public virtual void MakeIntoAnOrphan(IContainer container)
    {
        foreach (var contents in WhereSubContainers)
            contents.Value.Remove(container);
    }

    [CanBeNull]
    public IContainer GetParentContainerIfAny(IContainer container)
    {
        return WhereSubContainers.FirstOrDefault(k => k.Value.Contains(container)).Key;
    }

    public IFilter[] GetFilters(IContainer container)
    {
        return GetAllObjects<IFilter>().Where(f => f is not ExtractionFilter && f.FilterContainer_ID == container.ID)
            .ToArray();
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

    protected Dictionary<GovernancePeriod, HashSet<ICatalogue>> GovernanceCoverage { get; set; } = new();

    public virtual void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue)
    {
        if (!GovernanceCoverage.ContainsKey(governancePeriod))
            GovernanceCoverage.Add(governancePeriod, new HashSet<ICatalogue>());

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
        return GovernanceCoverage.ToDictionary(static k => k.Key.ID,
            static v => new HashSet<int>(v.Value.Select(static c => c.ID)));
    }

    public IEnumerable<ICatalogue> GetAllGovernedCatalogues(GovernancePeriod governancePeriod) =>
        !GovernanceCoverage.TryGetValue(governancePeriod, out var governedCatalogues) ? Enumerable.Empty<ICatalogue>()
            : governedCatalogues;

    #endregion

    #region IEncryptionManager

    public virtual void SetEncryptionKeyPath(string fullName)
    {
        EncryptionKeyPath = fullName;
    }

    public string GetEncryptionKeyPath() => EncryptionKeyPath;

    public virtual void DeleteEncryptionKeyPath()
    {
        EncryptionKeyPath = null;
    }

    #endregion

    protected override void CascadeDeletes(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        base.CascadeDeletes(oTableWrapperObject);

        switch (oTableWrapperObject)
        {
            case Catalogue catalogue:
                {
                    foreach (var ci in catalogue.CatalogueItems) ci.DeleteInDatabase();

                    break;
                }
            case ExtractionInformation extractionInformation:
                extractionInformation.CatalogueItem.ClearAllInjections();
                break;
            // when deleting a TableInfo
            case TableInfo t:
                {
                    // forget about its credentials usages
                    CredentialsDictionary.Remove(t);

                    foreach (var c in t.ColumnInfos) c.DeleteInDatabase();

                    break;
                }
            // when deleting a ColumnInfo
            case ColumnInfo columnInfo:
                {
                    foreach (var ci in Objects.Keys.OfType<CatalogueItem>().Where(ci => ci.ColumnInfo_ID == columnInfo.ID))
                    {
                        ci.ColumnInfo_ID = null;
                        ci.ClearAllInjections();
                        ci.SaveToDatabase();
                    }

                    break;
                }
            case CatalogueItem catalogueItem:
                catalogueItem.ExtractionInformation?.DeleteInDatabase();
                break;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName, IMapsDirectlyToDatabaseTable obj)
    {
        return GetAllObjectsWhere<ExtendedProperty>("Name", propertyName)
            .Where(r => r.IsReferenceTo(obj));
    }

    /// <inheritdoc/>
    public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName) =>
        GetAllObjectsWhere<ExtendedProperty>("Name", propertyName);

    /// <inheritdoc/>
    public IEnumerable<ExtendedProperty> GetExtendedProperties(IMapsDirectlyToDatabaseTable obj)
    {
        // First pass use SQL to pull only those with ReferencedObjectID ID that match our object
        return GetAllObjectsWhere<ExtendedProperty>("ReferencedObjectID", obj.ID)
            // Second pass make sure the object/repo match
            .Where(r => r.IsReferenceTo(obj));
    }
}