// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
/// How are files cached within the cache (e.g. within a zip? tar? just uncompressed in a directory).
/// </summary>
public enum CacheArchiveType
{
    /// <summary>
    /// Cached files are in a directory uncompressed
    /// </summary>
    None = 0,

    /// <summary>
    /// Cached files are contained in a zip file
    /// </summary>
    Zip = 1
}

/// <inheritdoc cref="ILoadMetadata"/>
public class LoadMetadata : DatabaseEntity, ILoadMetadata, IHasDependencies, IHasQuerySyntaxHelper,
    ILoggedActivityRootObject, IHasFolder
{
    #region Database Properties

    private string _locationOfFlatFiles;
    private string _anonymisationEngineClass;
    private string _name;
    private string _description;
    private CacheArchiveType _cacheArchiveType;
    private int? _overrideRawServerID;
    private bool _ignoreTrigger;
    private string _folder;
    private DateTime? _lastLoadTime;

    /// <inheritdoc/>
    [AdjustableLocation]
    public string LocationOfFlatFiles
    {
        get => _locationOfFlatFiles;
        set => SetField(ref _locationOfFlatFiles, value);
    }

    /// <summary>
    /// Not used
    /// </summary>
    public string AnonymisationEngineClass
    {
        get => _anonymisationEngineClass;
        set => SetField(ref _anonymisationEngineClass, value);
    }

    /// <inheritdoc/>
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Human readable description of the load, what it does etc
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    /// The format for storing files in when reading/writing to a cache with a <see cref="CacheProgress"/>.  This may not be respected
    /// depending on the implementation of the sepecific ICacheLayout
    /// </summary>
    public CacheArchiveType CacheArchiveType
    {
        get => _cacheArchiveType;
        set => SetField(ref _cacheArchiveType, value);
    }

    /// <summary>
    /// Optional.  Indicates that when running the Data Load Engine, the specific <see cref="ExternalDatabaseServer"/> should be used for the RAW server (instead of
    /// the system default - see <see cref="ServerDefaults"/>).
    /// </summary>
    public int? OverrideRAWServer_ID
    {
        get => _overrideRawServerID;
        set => SetField(ref _overrideRawServerID, value);
    }


    /// <iheritdoc/>
    public bool IgnoreTrigger
    {
        get => _ignoreTrigger;
        set => SetField(ref _ignoreTrigger, value);
    }

    /// <inheritdoc/>
    [UsefulProperty]
    public string Folder
    {
        get => _folder;
        set => SetField(ref _folder, FolderHelper.Adjust(value));
    }


    /// <summary>
    /// Stores the last time the load was ran.
    /// </summary>
    public DateTime? LastLoadTime
    {
        get => _lastLoadTime;
        set => SetField(ref _lastLoadTime, value);
    }

    #endregion


    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ExternalDatabaseServer OverrideRAWServer => OverrideRAWServer_ID.HasValue
        ? Repository.GetObjectByID<ExternalDatabaseServer>(OverrideRAWServer_ID.Value)
        : null;

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ILoadProgress[] LoadProgresses => Repository.GetAllObjectsWithParent<LoadProgress>(this);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IOrderedEnumerable<IProcessTask> ProcessTasks
    {
        get
        {
            return
                Repository.GetAllObjectsWithParent<ProcessTask>(this).Cast<IProcessTask>().OrderBy(pt => pt.Order);
        }
    }

    #endregion

    public LoadMetadata()
    {
    }

    /// <summary>
    /// Create a new DLE load.  This load will not have any <see cref="ProcessTask"/> and will not load any <see cref="TableInfo"/> yet.
    /// 
    /// <para>To set the loaded tables, set <see cref="Catalogue.LoadMetadata_ID"/> on some of your datasets</para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public LoadMetadata(ICatalogueRepository repository, string name = null)
    {
        name ??= $"NewLoadMetadata{Guid.NewGuid()}";

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "IgnoreTrigger", false /*todo could be system global default here*/ },
            { "Folder", FolderHelper.Root },
            {"LastLoadTime", null }
        });
    }

    internal LoadMetadata(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        LocationOfFlatFiles = r["LocationOfFlatFiles"].ToString();
        Name = r["Name"] as string;
        AnonymisationEngineClass = r["AnonymisationEngineClass"].ToString();
        Name = r["Name"].ToString();
        Description = r["Description"] as string; //allows for nulls
        CacheArchiveType = (CacheArchiveType)r["CacheArchiveType"];
        OverrideRAWServer_ID = ObjectToNullableInt(r["OverrideRAWServer_ID"]);
        IgnoreTrigger = ObjectToNullableBool(r["IgnoreTrigger"]) ?? false;
        Folder = r["Folder"] as string ?? FolderHelper.Root;
        LastLoadTime = string.IsNullOrWhiteSpace(r["LastLoadTime"].ToString()) ?null: DateTime.Parse(r["LastLoadTime"].ToString());
    }

    internal LoadMetadata(ShareManager shareManager, ShareDefinition shareDefinition) : base()
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
    }

    /// <inheritdoc/>
    public override void DeleteInDatabase()
    {
        var firstOrDefault = GetAllCatalogues().FirstOrDefault();

        if (firstOrDefault != null)
            throw new Exception(
                $"This load is used by {firstOrDefault.Name} so cannot be deleted (Disassociate it first)");

        base.DeleteInDatabase();
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <inheritdoc/>
    public IEnumerable<ICatalogue> GetAllCatalogues() => Repository.GetAllObjectsWithParent<Catalogue>(this);

    /// <inheritdoc cref="GetDistinctLoggingDatabase()"/>
    public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
    {
        var loggingServers = GetLoggingServers();

        var loggingServer = loggingServers.FirstOrDefault();

        //get distinct connection
        var toReturn = DataAccessPortal.ExpectDistinctServer(loggingServers, DataAccessContext.Logging, true);

        serverChosen = (IExternalDatabaseServer)loggingServer;
        return toReturn;
    }

    /// <summary>
    /// The unique logging server for auditing the load (found by querying <see cref="Catalogue.LiveLoggingServer"/>)
    /// </summary>
    /// <returns></returns>
    public DiscoveredServer GetDistinctLoggingDatabase() => GetDistinctLoggingDatabase(out _);

    private IDataAccessPoint[] GetLoggingServers()
    {
        var catalogue = GetAllCatalogues().ToArray();

        return !catalogue.Any()
            ? throw new NotSupportedException(
                $"LoadMetaData '{ToString()} (ID={ID}) does not have any Catalogues associated with it so it is not possible to fetch its LoggingDatabaseSettings")
            : (IDataAccessPoint[])catalogue.Select(c => c.LiveLoggingServer).ToArray();
    }

    /// <summary>
    /// Returns the unique value of <see cref="Catalogue.LoggingDataTask"/> amongst all catalogues loaded by the <see cref="LoadMetadata"/>
    /// </summary>
    /// <returns></returns>
    public string GetDistinctLoggingTask()
    {
        var catalogueMetadatas = GetAllCatalogues().ToArray();

        if (!catalogueMetadatas.Any())
            throw new Exception($"There are no Catalogues associated with load metadata (ID={ID})");

        var cataloguesWithoutLoggingTasks =
            catalogueMetadatas.Where(c => string.IsNullOrWhiteSpace(c.LoggingDataTask)).ToArray();

        if (cataloguesWithoutLoggingTasks.Any())
            throw new Exception(
                $"The following Catalogues do not have a LoggingDataTask specified:{cataloguesWithoutLoggingTasks.Aggregate("", (s, n) => $"{s}{n}(ID={n.ID}),")}");

        var distinctLoggingTasks = catalogueMetadatas.Select(c => c.LoggingDataTask).Distinct().ToArray();
        return distinctLoggingTasks.Length >= 2
            ? throw new Exception(
                $"There are {distinctLoggingTasks.Length} logging tasks in Catalogues belonging to this metadata (ID={ID})")
            : distinctLoggingTasks[0];
    }

    /// <summary>
    /// Return all <see cref="TableInfo"/> underlying the <see cref="Catalogue"/>(s) which use this load (what tables will be loaded by the DLE).
    /// </summary>
    /// <param name="includeLookups">true to include lookup tables (e.g. z_sex etc) configured in the <see cref="Catalogue"/>(s)</param>
    /// <returns></returns>
    public List<TableInfo> GetDistinctTableInfoList(bool includeLookups)
    {
        var toReturn = new List<TableInfo>();

        foreach (var catalogueMetadata in GetAllCatalogues())
            foreach (TableInfo tableInfo in catalogueMetadata.GetTableInfoList(includeLookups))
                if (!toReturn.Contains(tableInfo))
                    toReturn.Add(tableInfo);

        return toReturn;
    }

    /// <inheritdoc/>
    public DiscoveredServer GetDistinctLiveDatabaseServer()
    {
        var normalTables = new HashSet<ITableInfo>();
        var lookupTables = new HashSet<ITableInfo>();

        foreach (var catalogue in GetAllCatalogues())
        {
            catalogue.GetTableInfos(out var normal, out var lookup);

            foreach (var n in normal)
                normalTables.Add(n);
            foreach (var l in lookup)
                lookupTables.Add(l);
        }

        if (normalTables.Any())
            return DataAccessPortal.ExpectDistinctServer(normalTables.ToArray(), DataAccessContext.DataLoad, true);

        return lookupTables.Any()
            ? DataAccessPortal.ExpectDistinctServer(lookupTables.ToArray(), DataAccessContext.DataLoad, true)
            : throw new Exception(
                $"LoadMetadata {this} has no TableInfos configured (or possibly the tables have been deleted resulting in MISSING ColumnInfos?)");
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn() => null;

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => GetAllCatalogues().ToArray();

    /// <summary>
    /// Tests that the logging database for the load is reachable and that it has an appropriate logging task for the load (if not a new task will be created 'Loading X')
    /// </summary>
    /// <param name="catalogue"></param>
    public void EnsureLoggingWorksFor(ICatalogue catalogue)
    {
        //if there's no logging task / logging server set them up with the same name as the lmd
        IExternalDatabaseServer loggingServer;

        if (catalogue.LiveLoggingServer_ID == null)
        {
            loggingServer = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer != null)
                catalogue.LiveLoggingServer_ID = loggingServer.ID;
            else
                throw new NotSupportedException(
                    "You do not yet have any logging servers configured so cannot create data loads");
        }
        else
        {
            loggingServer = Repository.GetObjectByID<ExternalDatabaseServer>(catalogue.LiveLoggingServer_ID.Value);
        }

        //if there's no logging task yet and there's a logging server
        if (string.IsNullOrWhiteSpace(catalogue.LoggingDataTask))
        {
            var lm = new LogManager(loggingServer);
            var loggingTaskName = Name;

            lm.CreateNewLoggingTaskIfNotExists(loggingTaskName);
            catalogue.LoggingDataTask = loggingTaskName;
            catalogue.SaveToDatabase();
        }
    }

    /// <inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        var syntax = GetAllCatalogues().Select(c => c.GetQuerySyntaxHelper()).Distinct().ToArray();
        return syntax.Length > 1
            ? throw new Exception(
                $"LoadMetadata '{this}' has multiple underlying Catalogue Live Database Type(s) - not allowed")
            : syntax.SingleOrDefault();
    }

    /// <summary>
    /// Returns all runs since each LoadMetadata has its own task and all runs apply to that task and hence this object
    /// </summary>
    /// <param name="runs"></param>
    /// <returns></returns>
    public IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs) => runs;

    public static bool UsesPersistentRaw(ILoadMetadata loadMetadata)
    {
        return loadMetadata.CatalogueRepository.GetExtendedProperties(ExtendedProperty.PersistentRaw,
            loadMetadata).Any(p => p.Value == "true");
    }
}