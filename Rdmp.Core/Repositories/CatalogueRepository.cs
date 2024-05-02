// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Comments;

namespace Rdmp.Core.Repositories;

/// <inheritdoc cref="ICatalogueRepository" />
public class CatalogueRepository : TableRepository, ICatalogueRepository
{
    /// <inheritdoc />
    public IAggregateForcedJoinManager AggregateForcedJoinManager { get; }

    /// <inheritdoc />
    public IGovernanceManager GovernanceManager { get; }

    /// <inheritdoc />
    public ITableInfoCredentialsManager TableInfoCredentialsManager { get; }

    /// <inheritdoc />
    public IJoinManager JoinManager { get; set; }

    /// <inheritdoc />
    public CommentStore CommentStore { get; set; }

    /// <inheritdoc />
    public ICohortContainerManager CohortContainerManager { get; }

    public IEncryptionManager EncryptionManager { get; }

    /// <summary>
    ///     Flag used by Startup processes to determine whether the <see cref="CommentStore" /> should be loaded with
    ///     documentation from the xmldoc files.
    /// </summary>
    public static bool SuppressHelpLoading;

    /// <inheritdoc />
    public IFilterManager FilterManager { get; }

    /// <summary>
    ///     Sets up an <see cref="IRepository" /> which connects to the database <paramref name="catalogueConnectionString" />
    ///     to fetch/create <see cref="DatabaseEntity" /> objects.
    /// </summary>
    /// <param name="catalogueConnectionString"></param>
    public CatalogueRepository(DbConnectionStringBuilder catalogueConnectionString) : base(null,
        catalogueConnectionString)
    {
        AggregateForcedJoinManager = new AggregateForcedJoin(this);
        GovernanceManager = new GovernanceManager(this);
        TableInfoCredentialsManager = new TableInfoCredentialsManager(this);
        JoinManager = new JoinManager(this);
        CohortContainerManager = new CohortContainerManager(this);
        FilterManager = new AggregateFilterManager(this);
        EncryptionManager = new PasswordEncryptionKeyLocation(this);

        CommentStore = new CommentStoreWithKeywords();

        ObscureDependencyFinder = new CatalogueObscureDependencyFinder(this);

        //Shortcuts to improve performance of ConstructEntity (avoids reflection)
        Constructors.Add(typeof(Catalogue), (rep, r) => new Catalogue((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(CohortAggregateContainer),
            (rep, r) => new CohortAggregateContainer((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(CohortIdentificationConfiguration),
            (rep, r) => new CohortIdentificationConfiguration((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(GovernanceDocument), (rep, r) => new GovernanceDocument((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(GovernancePeriod), (rep, r) => new GovernancePeriod((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(StandardRegex), (rep, r) => new StandardRegex((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AnyTableSqlParameter),
            (rep, r) => new AnyTableSqlParameter((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ANOTable), (rep, r) => new ANOTable((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateConfiguration),
            (rep, r) => new AggregateConfiguration((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateContinuousDateAxis),
            (rep, r) => new AggregateContinuousDateAxis((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateDimension), (rep, r) => new AggregateDimension((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateFilter), (rep, r) => new AggregateFilter((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateFilterContainer),
            (rep, r) => new AggregateFilterContainer((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateFilterParameter),
            (rep, r) => new AggregateFilterParameter((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(CatalogueItem), (rep, r) => new CatalogueItem((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ColumnInfo), (rep, r) => new ColumnInfo((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(JoinableCohortAggregateConfiguration),
            (rep, r) => new JoinableCohortAggregateConfiguration((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(JoinableCohortAggregateConfigurationUse),
            (rep, r) => new JoinableCohortAggregateConfigurationUse((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ExternalDatabaseServer),
            (rep, r) => new ExternalDatabaseServer((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ExtractionFilter), (rep, r) => new ExtractionFilter((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ExtractionFilterParameter),
            (rep, r) => new ExtractionFilterParameter((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ExtractionInformation),
            (rep, r) => new ExtractionInformation((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ExtractionFilterParameterSet),
            (rep, r) => new ExtractionFilterParameterSet((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(LoadMetadata), (rep, r) => new LoadMetadata((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ExtractionFilterParameterSetValue),
            (rep, r) => new ExtractionFilterParameterSetValue((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(LoadProgress), (rep, r) => new LoadProgress((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(Favourite), (rep, r) => new Favourite((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(Pipeline), (rep, r) => new Pipeline((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(Lookup), (rep, r) => new Lookup((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(AggregateTopX), (rep, r) => new AggregateTopX((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(PipelineComponent), (rep, r) => new PipelineComponent((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(LookupCompositeJoinInfo),
            (rep, r) => new LookupCompositeJoinInfo((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(PipelineComponentArgument),
            (rep, r) => new PipelineComponentArgument((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(PreLoadDiscardedColumn),
            (rep, r) => new PreLoadDiscardedColumn((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ProcessTask), (rep, r) => new ProcessTask((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(DashboardLayout), (rep, r) => new DashboardLayout((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ProcessTaskArgument),
            (rep, r) => new ProcessTaskArgument((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(DashboardControl), (rep, r) => new DashboardControl((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(DataAccessCredentials),
            (rep, r) => new DataAccessCredentials((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(SupportingDocument), (rep, r) => new SupportingDocument((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(DashboardObjectUse), (rep, r) => new DashboardObjectUse((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(SupportingSQLTable), (rep, r) => new SupportingSQLTable((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(TableInfo), (rep, r) => new TableInfo((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(RemoteRDMP), (rep, r) => new RemoteRDMP((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ObjectImport), (rep, r) => new ObjectImport((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ObjectExport), (rep, r) => new ObjectExport((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(CacheProgress), (rep, r) => new CacheProgress((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(ConnectionStringKeyword),
            (rep, r) => new ConnectionStringKeyword((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(WindowLayout), (rep, r) => new WindowLayout((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(PermissionWindow), (rep, r) => new PermissionWindow((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(TicketingSystemConfiguration),
            (rep, r) => new TicketingSystemConfiguration((ICatalogueRepository)rep, r));
        Constructors.Add(typeof(CacheFetchFailure), (rep, r) => new CacheFetchFailure((ICatalogueRepository)rep, r));
    }

    /// <inheritdoc />
    public LogManager GetDefaultLogManager()
    {
        return new LogManager(GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
    }

    /// <inheritdoc />
    public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent)
    {
        var type = parent.GetType();

        return !AnyTableSqlParameter.IsSupportedType(type)
            ? throw new NotSupportedException($"This table does not support parents of type {type.Name}")
            : (IEnumerable<AnyTableSqlParameter>)GetReferencesTo<AnyTableSqlParameter>(parent);
    }

    /// <inheritdoc />
    public TicketingSystemConfiguration GetTicketingSystem()
    {
        var configuration = GetAllObjects<TicketingSystemConfiguration>().Where(t => t.IsActive).ToArray();

        return configuration.Length switch
        {
            0 => null,
            1 => configuration[0],
            _ => throw new NotSupportedException(
                $"There should only ever be one active ticketing system, something has gone very wrong, there are currently {configuration.Length}")
        };
    }

    protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
    {
        return Constructors.TryGetValue(t, out var constructor)
            ? constructor(this, reader)
            : ObjectConstructor.ConstructIMapsDirectlyToDatabaseObject<ICatalogueRepository>(t, this, reader);
    }

    private readonly ConcurrentDictionary<Type, IRowVerCache> _caches = new();

    public override T[] GetAllObjects<T>()
    {
        return _caches.GetOrAdd(typeof(T), t => new RowVerCache<T>(this))
            .GetAllObjects<T>();
    }

    public override T[] GetAllObjectsNoCache<T>()
    {
        return base.GetAllObjects<T>();
    }


    public ExternalDatabaseServer[] GetAllDatabases<T>() where T : IPatcher, new()
    {
        IPatcher p = new T();
        return GetAllObjects<ExternalDatabaseServer>().Where(s => s.WasCreatedBy(p)).ToArray();
    }


    /// <summary>
    ///     Returns all objects of Type T which reference the supplied object <paramref name="o" />
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity
    {
        return GetAllObjects<T>(
            $"WHERE ReferencedObjectID = {o.ID} AND ReferencedObjectType = '{o.GetType().Name}' AND ReferencedObjectRepositoryType = '{o.Repository.GetType().Name}'");
    }

    public bool IsLookupTable(ITableInfo tableInfo)
    {
        using var con = GetConnection();
        using var cmd = DatabaseCommandHelper.GetCommand(
            @"if exists (select 1 from Lookup join ColumnInfo on Lookup.Description_ID = ColumnInfo.ID where TableInfo_ID = @tableInfoID)
select 1
else
select 0", con.Connection, con.Transaction);
        DatabaseCommandHelper.AddParameterWithValueToCommand("@tableInfoID", cmd, tableInfo.ID);
        return Convert.ToBoolean(cmd.ExecuteScalar());
    }

    public Catalogue[] GetAllCataloguesUsing(TableInfo tableInfo)
    {
        return GetAllObjects<Catalogue>(
            $@"Where
  Catalogue.ID in (Select CatalogueItem.Catalogue_ID from
  CatalogueItem join
  ColumnInfo on ColumnInfo_ID = ColumnInfo.ID
  where
  TableInfo_ID = {tableInfo.ID} )").ToArray();
    }

    public IExternalDatabaseServer GetDefaultFor(PermissableDefaults field)
    {
        if (field == PermissableDefaults.None)
            return null;

        using var con = GetConnection();
        using var cmd = DatabaseCommandHelper.GetCommand(
            "SELECT ExternalDatabaseServer_ID FROM ServerDefaults WHERE DefaultType = @type", con.Connection,
            con.Transaction);
        var p = cmd.CreateParameter();

        p.ParameterName = "@type";
        p.Value = ServerDefaults.StringExpansionDictionary[field];
        cmd.Parameters.Add(p);

        var executeScalar = cmd.ExecuteScalar();

        return executeScalar == DBNull.Value
            ? null
            : GetObjectByID<ExternalDatabaseServer>(Convert.ToInt32(executeScalar));
    }

    public void ClearDefault(PermissableDefaults toDelete)
    {
        if (toDelete == PermissableDefaults.None)
            return;

        Delete("DELETE FROM ServerDefaults WHERE DefaultType=@DefaultType",
            new Dictionary<string, object>
            {
                { "DefaultType", ServerDefaults.StringExpansionDictionary[toDelete] }
            }, false);
    }


    /// <inheritdoc />
    public void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
    {
        if (toChange == PermissableDefaults.None)
            throw new ArgumentException("toChange cannot be None", nameof(toChange));

        if (externalDatabaseServer == null)
        {
            ClearDefault(toChange);
            return;
        }


        var oldValue = GetDefaultFor(toChange);

        if (oldValue == null)
            InsertNewValue(toChange, externalDatabaseServer);
        else
            UpdateExistingValue(toChange, externalDatabaseServer);
    }

    private void UpdateExistingValue(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
    {
        if (toChange == PermissableDefaults.None)
            throw new ArgumentException("toChange cannot be None", nameof(toChange));

        const string sql =
            "UPDATE ServerDefaults set ExternalDatabaseServer_ID  = @ExternalDatabaseServer_ID where DefaultType=@DefaultType";

        var affectedRows = Update(sql, new Dictionary<string, object>
        {
            { "DefaultType", ServerDefaults.StringExpansionDictionary[toChange] },
            { "ExternalDatabaseServer_ID", externalDatabaseServer.ID }
        });

        if (affectedRows != 1)
            throw new Exception(
                $"We were asked to update default for {toChange} but the query '{sql}' did not result in 1 affected rows (it resulted in {affectedRows})");
    }

    private void InsertNewValue(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
    {
        if (toChange == PermissableDefaults.None)
            throw new ArgumentException("toChange cannot be None", nameof(toChange));

        Insert(
            "INSERT INTO ServerDefaults(DefaultType,ExternalDatabaseServer_ID) VALUES (@DefaultType,@ExternalDatabaseServer_ID)",
            new Dictionary<string, object>
            {
                { "DefaultType", ServerDefaults.StringExpansionDictionary[toChange] },
                { "ExternalDatabaseServer_ID", externalDatabaseServer.ID }
            });
    }

    public void SetEncryptionKeyPath(string path)
    {
        using var con = GetConnection();
        //Table can only ever have 1 record
        using var cmd = DatabaseCommandHelper.GetCommand(
            @"if exists (select 1 from PasswordEncryptionKeyLocation)
    UPDATE PasswordEncryptionKeyLocation SET Path = @Path
  else
  INSERT INTO PasswordEncryptionKeyLocation(Path,Lock) VALUES (@Path,'X')
  ", con.Connection, con.Transaction);
        DatabaseCommandHelper.AddParameterWithValueToCommand("@Path", cmd, path);
        cmd.ExecuteNonQuery();
    }

    public string GetEncryptionKeyPath()
    {
        //otherwise use the database
        using var con = DiscoveredServer.GetConnection();
        con.Open();
        //Table can only ever have 1 record
        using var cmd = DatabaseCommandHelper.GetCommand("SELECT Path from PasswordEncryptionKeyLocation", con);
        return cmd.ExecuteScalar() as string;
    }

    public void DeleteEncryptionKeyPath()
    {
        using var con = GetConnection();
        //Table can only ever have 1 record
        using var cmd = DatabaseCommandHelper.GetCommand("DELETE FROM PasswordEncryptionKeyLocation",
            con.Connection, con.Transaction);
        var affectedRows = cmd.ExecuteNonQuery();
        if (affectedRows != 1)
            throw new Exception($"Delete from PasswordEncryptionKeyLocation resulted in {affectedRows}, expected 1");
    }

    /// <inheritdoc />
    public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName, IMapsDirectlyToDatabaseTable obj)
    {
        return GetAllObjectsWhere<ExtendedProperty>("Name", propertyName)
            .Where(r => r.IsReferenceTo(obj));
    }

    /// <inheritdoc />
    public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName)
    {
        return GetAllObjectsWhere<ExtendedProperty>("Name", propertyName);
    }

    /// <inheritdoc />
    public IEnumerable<ExtendedProperty> GetExtendedProperties(IMapsDirectlyToDatabaseTable obj)
    {
        // First pass use SQL to pull only those with ReferencedObjectID ID that match our object
        return GetAllObjectsWhere<ExtendedProperty>("ReferencedObjectID", obj.ID)
            // Second pass make sure the object/repo match
            .Where(r => r.IsReferenceTo(obj));
    }
}