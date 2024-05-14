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
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Describes an sql table (or table valued function) on a given Server from which you intend to either extract and/or load / curate data.
/// These can be created most easily by using TableInfoImporter.  This entity is the hanging off point for PreLoadDiscardedColumn, ColumnInfo etc
/// 
/// <para>TDescribes an sql table (or table valued function) on a given [DBMS] Server from which you intend to either extract and/or load / curate data.
/// A TableInfo represents a cached state of the live database table schema.  You can synchronize a TableInfo at any time to handle schema changes
/// (e.g. dropping columns) (see <see cref="TableInfoSynchronizer"/>).</para>
/// </summary>
public class TableInfo : DatabaseEntity, ITableInfo, INamed, IHasFullyQualifiedNameToo, IInjectKnown<ColumnInfo[]>,
    ICheckable
{
    /// <summary>
    /// Cached results of <see cref="GetQuerySyntaxHelper"/>
    /// </summary>
    private static ConcurrentDictionary<DatabaseType, IQuerySyntaxHelper> _cachedSyntaxHelpers = new();

    #region Database Properties

    private string _name;
    private DatabaseType _databaseType;
    private string _server;
    private string _database;
    private string _state;
    private string _validationXml;
    private bool _isPrimaryExtractionTable;
    private int? _identifierDumpServer_ID;
    private bool _isTableValuedFunction;
    private string _schema;
    private bool _isView;

    /// <summary>
    /// Fully specified table name
    /// </summary>
    [Sql]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    public DatabaseType DatabaseType
    {
        get => _databaseType;
        set => SetField(ref _databaseType, value);
    }

    /// <inheritdoc/>
    public string Server
    {
        get => _server;
        set => SetField(ref _server, value);
    }

    /// <inheritdoc/>
    [Sql]
    public string Database
    {
        get => _database;
        set => SetField(ref _database, value);
    }

    /// <summary>
    /// Obsolete
    /// </summary>
    [Obsolete("Not used for anything")]
    public string State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    /// <summary>
    /// Obsolete
    /// </summary>
    [Obsolete("Not used for anything")]
    [DoNotExtractProperty]
    public string ValidationXml
    {
        get => _validationXml;
        set => SetField(ref _validationXml, value);
    }

    /// <inheritdoc/>
    public bool IsPrimaryExtractionTable
    {
        get => _isPrimaryExtractionTable;
        set => SetField(ref _isPrimaryExtractionTable, value);
    }

    /// <inheritdoc/>
    public int? IdentifierDumpServer_ID
    {
        get => _identifierDumpServer_ID;
        set => SetField(ref _identifierDumpServer_ID, value);
    }

    /// <inheritdoc/>
    public bool IsTableValuedFunction
    {
        get => _isTableValuedFunction;
        set => SetField(ref _isTableValuedFunction, value);
    }

    /// <inheritdoc/>
    public string Schema
    {
        get => _schema;
        set => SetField(ref _schema, value);
    }

    public bool IsView
    {
        get => _isView;
        set => SetField(ref _isView, value);
    }

    #endregion

    private Lazy<ColumnInfo[]> _knownColumnInfos;
    private Lazy<bool> _knownIsLookup;
    private Dictionary<DataAccessContext, Lazy<IDataAccessCredentials>> _knownCredentials = new();


    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ColumnInfo[] ColumnInfos => _knownColumnInfos.Value;

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public PreLoadDiscardedColumn[] PreLoadDiscardedColumns =>
        Repository.GetAllObjectsWithParent<PreLoadDiscardedColumn>(this);

    /// <inheritdoc cref="IdentifierDumpServer_ID"/>
    [NoMappingToDatabase]
    public ExternalDatabaseServer IdentifierDumpServer =>
        IdentifierDumpServer_ID == null
            ? null
            : Repository.GetObjectByID<ExternalDatabaseServer>((int)IdentifierDumpServer_ID);

    #endregion

    public TableInfo()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Defines a new table reference in the platform database <paramref name="repository"/>.
    /// <para>Usually you should use <see cref="TableInfoImporter"/> instead</para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public TableInfo(ICatalogueRepository repository, string name)
    {
        var dumpServer = repository.GetDefaultFor(PermissableDefaults.IdentifierDumpServer_ID);

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "IdentifierDumpServer_ID", dumpServer?.ID ?? (object)DBNull.Value }
        });

        ClearAllInjections();
    }

    internal TableInfo(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"].ToString();
        DatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), r["DatabaseType"].ToString());
        Server = r["Server"].ToString();
        Database = r["Database"].ToString();
        _state = r["State"].ToString();
        Schema = r["Schema"].ToString();
        _validationXml = r["ValidationXml"].ToString();

        IsTableValuedFunction =
            r["IsTableValuedFunction"] != DBNull.Value && Convert.ToBoolean(r["IsTableValuedFunction"]);

        IsPrimaryExtractionTable = r["IsPrimaryExtractionTable"] != DBNull.Value &&
                                   Convert.ToBoolean(r["IsPrimaryExtractionTable"]);

        IdentifierDumpServer_ID =
            r["IdentifierDumpServer_ID"] == DBNull.Value ? null : (int)r["IdentifierDumpServer_ID"];

        IsView = r["IsView"] != DBNull.Value && Convert.ToBoolean(r["IsView"]);

        ClearAllInjections();
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <inheritdoc/>
    public ISqlParameter[] GetAllParameters() => CatalogueRepository.GetAllParametersForParentTable(this).ToArray();

    /// <summary>
    /// Sorts two <see cref="TableInfo"/> alphabetically
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
        if (obj is TableInfo)
            return -string.Compare(obj.ToString(), ToString(),
                StringComparison.CurrentCulture); //sort alphabetically (reverse)

        throw new Exception($"Cannot compare {GetType().Name} to {obj.GetType().Name}");
    }


    /// <inheritdoc/>
    public string GetRuntimeName() => GetQuerySyntaxHelper().GetRuntimeName(Name);

    /// <inheritdoc cref="ITableInfo.GetFullyQualifiedName"/>
    public string GetFullyQualifiedName() =>
        GetQuerySyntaxHelper().EnsureFullyQualified(Database, Schema, GetRuntimeName());

    /// <inheritdoc cref="ITableInfo.GetDatabaseRuntimeName()"/>
    public string GetDatabaseRuntimeName() => Database.Trim(QuerySyntaxHelper.TableNameQualifiers);


    /// <inheritdoc/>
    public string GetDatabaseRuntimeName(LoadStage loadStage, INameDatabasesAndTablesDuringLoads namer = null)
    {
        var baseName = GetDatabaseRuntimeName();

        namer ??= new FixedStagingDatabaseNamer(baseName);

        return namer.GetDatabaseName(baseName, loadStage.ToLoadBubble());
    }

    /// <inheritdoc/>
    public string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
    {
        // If no naming scheme is specified, the default 'FixedStaging...' prepends the database name and appends '_STAGING'
        tableNamingScheme ??= new FixedStagingDatabaseNamer(Database);

        var baseName = GetQuerySyntaxHelper().GetRuntimeName(Name);

        return tableNamingScheme.GetName(baseName, bubble);
    }

    /// <inheritdoc/>
    public string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null) =>
        GetRuntimeName(stage.ToLoadBubble(), tableNamingScheme);

    /// <inheritdoc/>
    public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context) =>
        context == DataAccessContext.Any
            ? throw new Exception("You cannot ask for any credentials, you must supply a usage context.")
            : _knownCredentials[context].Value;

    /// <summary>
    /// Declares that the given <paramref name="credentials"/> should be used to access the data table referenced by this
    /// <see cref="TableInfo"/> under the given <see cref="DataAccessContext"/> (loading data etc).
    /// </summary>
    /// <param name="credentials">Credentials to use (username / encrypted password)</param>
    /// <param name="context">When the credentials can be used (Use Any for any case)</param>
    /// <param name="allowOverwriting">False will throw if there is already credentials declared for the table/context</param>
    public void SetCredentials(DataAccessCredentials credentials, DataAccessContext context,
        bool allowOverwriting = false)
    {
        var existingCredentials =
            CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(this, context);

        //if user told us to set credentials to null complain
        if (credentials == null)
            throw new Exception(
                "Credentials was null, to remove a credential use TableInfoToCredentialsLinker.BreakLinkBetween instead");

        //if there are existing credentials already
        if (existingCredentials != null)
        {
            //user is trying to set the same credentials again
            if (existingCredentials.Equals(credentials))
                return; //don't bother

            if (!allowOverwriting)
                throw new Exception(
                    $"Cannot overwrite existing credentials {existingCredentials.Name} with new credentials {credentials.Name} with context {context} because allowOverwriting was false");

            //allow overwriting is on
            //remove the existing link
            CatalogueRepository.TableInfoCredentialsManager.BreakLinkBetween(existingCredentials, this, context);
        }

        //create a new one to the new credentials
        CatalogueRepository.TableInfoCredentialsManager.CreateLinkBetween(credentials, this, context);
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return
            CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(this)
                .Select(kvp => kvp.Value)
                .Cast<IHasDependencies>()
                .ToArray();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => ColumnInfos.ToArray();


    /// <summary>
    /// Checks that the table referenced exists on the database server and that its properties and <see cref="ColumnInfo"/> etc are synchronized with the live
    /// table as it exists on the server.
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (IsLookupTable())
            if (IsPrimaryExtractionTable)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Table is both a Lookup table AND is marked IsPrimaryExtractionTable", CheckResult.Fail));

        try
        {
            var synchronizer = new TableInfoSynchronizer(this);
            synchronizer.Synchronize(notifier);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Synchronization failed on TableInfo {this}",
                CheckResult.Fail, e));
        }
    }

    /// <summary>
    /// Higher performance version of <see cref="IsLookupTable()"/> when you have
    /// a <see cref="ICoreChildProvider"/> around for rapid in memory answers
    /// </summary>
    /// <param name="childProvider"></param>
    /// <returns></returns>
    public bool IsLookupTable(ICoreChildProvider childProvider)
    {
        // we are a lookup if
        var lookupDescriptionColumnInfoIds = new HashSet<int>(childProvider.AllLookups.Select(l => l.Description_ID));
        return ColumnInfos.Any(c => lookupDescriptionColumnInfoIds.Contains(c.ID));
    }

    /// <inheritdoc/>
    public bool IsLookupTable() => _knownIsLookup.Value;

    private bool FetchIsLookup() => CatalogueRepository.IsLookupTable(this);

    /// <inheritdoc/>
    public Catalogue[] GetAllRelatedCatalogues() => CatalogueRepository.GetAllCataloguesUsing(this);

    /// <inheritdoc/>
    public IEnumerable<IHasStageSpecificRuntimeName> GetColumnsAtStage(LoadStage loadStage)
    {
        //if it is AdjustRaw then it will also have the pre load discarded columns
        if (loadStage <= LoadStage.AdjustRaw)
            foreach (var discardedColumn in PreLoadDiscardedColumns.Where(c =>
                         c.Destination != DiscardedColumnDestination.Dilute))
                yield return discardedColumn;

        //also add column infos
        foreach (var c in ColumnInfos)
            if (loadStage <= LoadStage.AdjustRaw && SpecialFieldNames.IsHicPrefixed(c))
                continue;
            else if (loadStage <= LoadStage.AdjustStaging &&
                     c.IsAutoIncrement) //auto increment columns do not get created in RAW/STAGING
                continue;
            else if (loadStage == LoadStage.AdjustStaging &&
                     //these two do not appear in staging
                     (c.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID) ||
                      c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom))
                    )
                continue;
            else
                yield return c;
    }

    /// <inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() =>
        _cachedSyntaxHelpers.GetOrAdd(DatabaseType, new QuerySyntaxHelperFactory().Create(DatabaseType));

    /// <inheritdoc/>
    public void InjectKnown(ColumnInfo[] instance)
    {
        _knownColumnInfos = new Lazy<ColumnInfo[]>(instance);
    }

    /// <inheritdoc/>
    public void ClearAllInjections()
    {
        _knownColumnInfos = new Lazy<ColumnInfo[]>(FetchColumnInfos);
        _knownIsLookup = new Lazy<bool>(FetchIsLookup);
        _knownCredentials.Clear();

        foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
        {
            if (context == DataAccessContext.Any)
                continue;

            //avoid access to
            var context1 = context;
            _knownCredentials.Add(context1,
                new Lazy<IDataAccessCredentials>(() =>
                    CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(this,
                        context1)));
        }
    }

    private ColumnInfo[] FetchColumnInfos() => Repository.GetAllObjectsWithParent<ColumnInfo, TableInfo>(this);

    /// <inheritdoc/>
    public DiscoveredTable Discover(DataAccessContext context)
    {
        var db = DataAccessPortal.ExpectDatabase(this, context);

        return IsTableValuedFunction
            ? db.ExpectTableValuedFunction(GetRuntimeName(), Schema)
            : db.ExpectTable(GetRuntimeName(), Schema, IsView ? TableType.View : TableType.Table);
    }

    /// <inheritdoc/>
    public bool DiscoverExistence(DataAccessContext context, out string reason)
    {
        DiscoveredTable tbl;

        try
        {
            tbl = Discover(context);
        }
        catch (Exception ex)
        {
            reason = ex.Message;
            return false;
        }

        if (!tbl.Database.Server.Exists())
        {
            reason = $"Server {tbl.Database.Server} did not exist";
            return false;
        }

        if (!tbl.Database.Exists())
        {
            reason = $"Database {tbl.Database} did not exist";
            return false;
        }

        if (!tbl.Exists())
        {
            reason = $"Table {tbl.GetFullyQualifiedName()} did not exist";
            return false;
        }

        reason = null;
        return true;
    }


    /// <summary>
    /// Returns true if the TableInfo is a reference to the discovered live table (same database, same table name, same server)
    /// <para>By default servername is not checked since you can have server aliases e.g. localhost\sqlexpress could be the same as 127.0.0.1\sqlexpress</para>
    /// </summary>
    /// <param name="discoveredTable">Pass true to also check the servername is EXACTLY the same (dangerous due to the fact that servers can be accessed by hostname or IP etc)</param>
    /// <param name="alsoCheckServer"></param>
    /// <returns></returns>
    public bool Is(DiscoveredTable discoveredTable, bool alsoCheckServer = false) =>
        GetRuntimeName().Equals(discoveredTable.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase) &&
        GetDatabaseRuntimeName()
            .Equals(discoveredTable.Database.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase) &&
        DatabaseType == discoveredTable.Database.Server.DatabaseType &&
        (!alsoCheckServer ||
         discoveredTable.Database.Server.Name.Equals(Server, StringComparison.CurrentCultureIgnoreCase));
}