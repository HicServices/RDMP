// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using YamlDotNet.Serialization;

namespace Rdmp.Core.Repositories;

/// <summary>
/// Implementation of <see cref="IRepository"/> which creates objects on the file system instead of a database.
/// </summary>
public class YamlRepository : MemoryDataExportRepository
{
    private ISerializer _serializer;

    /// <summary>
    /// All objects that are known about by this repository
    /// </summary>
    public IReadOnlyCollection<IMapsDirectlyToDatabaseTable> AllObjects => Objects.Keys.ToList().AsReadOnly();

    public DirectoryInfo Directory { get; }

    private readonly Lock lockFs = new();

    public YamlRepository(DirectoryInfo dir)
    {
        Directory = dir;

        if (!Directory.Exists)
            Directory.Create();

        // Build the serializer
        _serializer = CreateSerializer(GetCompatibleTypes());
        
        if (File.Exists(GetEncryptionKeyPathFile()))
        {
            EncryptionKeyPath = File.ReadLines(GetEncryptionKeyPathFile()).First();
            // Check if the file does exist but we were confused by stray whitespace:
            if (!File.Exists(EncryptionKeyPath) && char.IsWhiteSpace(EncryptionKeyPath[^1]))
            {
                var trimmed = EncryptionKeyPath.TrimEnd();
                if (File.Exists(trimmed)) EncryptionKeyPath = trimmed;
            }
        }
        
        LoadObjects();      

        // Don't create new objects with the ID of existing objects
        NextObjectId = Objects.IsEmpty ? 0 : Objects.Max(o => o.Key.ID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static ISerializer CreateSerializer(IEnumerable<Type> supportedTypes)
    {
        var builder = new SerializerBuilder();
        builder.WithTypeConverter(new VersionYamlTypeConverter());

        foreach (var type in supportedTypes)
        {
            var respect = TableRepository.GetPropertyInfos(type);

            builder = type.GetProperties().Where(prop => !respect.Contains(prop)).Aggregate(builder,
                (current, prop) => current.WithAttributeOverride(type, prop.Name, new YamlIgnoreAttribute()));
        }

        return builder.Build();
    }

    private void LoadObjects()
    {
        var subdirs = Directory.GetDirectories();
        var builder = new DeserializerBuilder();
        builder.WithTypeConverter(new VersionYamlTypeConverter());

        var deserializer = builder.Build();

        foreach (var t in GetCompatibleTypes().OrderBy(ObjectDependencyOrder))
        {
            // find the directory that contains all the YAML files e.g. MyDir/Catalogue/
            var typeDir = subdirs.FirstOrDefault(d => d.Name.Equals(t.Name));

            if (typeDir == null)
            {
                Directory.CreateSubdirectory(t.Name);
                continue;
            }

            lock (lockFs)
            {
                foreach (var yaml in typeDir.EnumerateFiles("*.yaml"))
                    try
                    {
                        var obj = (IMapsDirectlyToDatabaseTable)deserializer.Deserialize(
                            File.ReadAllText(yaml.FullName), t);
                        SetRepositoryOnObject(obj);
                        Objects.TryAdd(obj, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error loading object in file {yaml.FullName}", ex);
                    }
            }
        }

        LoadDefaults();

        LoadDataExportProperties();

        LoadCredentialsDictionary();

        PackageDictionary = Load<IExtractableDataSetPackage, IExtractableDataSet>(nameof(PackageDictionary)) ??
                            PackageDictionary;

        GovernanceCoverage = Load<GovernancePeriod, ICatalogue>(nameof(GovernanceCoverage)) ?? GovernanceCoverage;

        ForcedJoins = Load<AggregateConfiguration, ITableInfo>(nameof(ForcedJoins)) ?? ForcedJoins;

        LoadCohortContainerContents();

        LoadWhereSubContainers();
    }

    private int ObjectDependencyOrder(Type arg)
    {
        // Load Plugin objects before dependent children
        if (arg == typeof(Plugin))
            return 1;

        return arg == typeof(LoadModuleAssembly) ? 2 : 3;
    }


    /// <summary>
    /// Sets <see cref="IMapsDirectlyToDatabaseTable.Repository"/> on <paramref name="obj"/>.
    /// Override to also set other destination repo specific fields
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void SetRepositoryOnObject(IMapsDirectlyToDatabaseTable obj)
    {
        obj.Repository = this;

        switch (obj)
        {
            case DataAccessCredentials creds:
                creds.SetRepository(this);
                break;
            case ExternalDatabaseServer eds:
                eds.SetRepository(this);
                break;
            case ExternalCohortTable ect:
                ect.SetRepository(this);
                break;
            case RemoteRDMP remote:
                remote.SetRepository(this);
                break;
            case ConcreteContainer container:
                container.SetManager(this);
                break;
        }
    }

    public override void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters)
    {
        base.InsertAndHydrate(toCreate, constructorParameters);

        // put it on disk
        lock (lockFs)
        {
            SaveToDatabase(toCreate);
        }
    }

    public override void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        lock (lockFs)
        {
            base.DeleteFromDatabase(oTableWrapperObject);
            File.Delete(GetPath(oTableWrapperObject));
        }
    }

    public override void SaveToDatabase(IMapsDirectlyToDatabaseTable o)
    {
        base.SaveToDatabase(o);

        SetRepositoryOnObject(o);

        var yaml = _serializer.Serialize(o);

        lock (lockFs)
        {
            File.WriteAllText(GetPath(o), yaml);
        }
    }

    /// <summary>
    /// Returns the path on disk in which the yaml file for <paramref name="o"/> is stored
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private string GetPath(IMapsDirectlyToDatabaseTable o) =>
        Path.Combine(Directory.FullName, o.GetType().Name, $"{o.ID}.yaml");

    public override void DeleteEncryptionKeyPath()
    {
        base.DeleteEncryptionKeyPath();

        if (File.Exists(GetEncryptionKeyPathFile()))
            File.Delete(GetEncryptionKeyPathFile());
    }

    public override void SetEncryptionKeyPath(string fullName)
    {
        base.SetEncryptionKeyPath(fullName);

        // if setting it to null
        if (string.IsNullOrWhiteSpace(fullName))
        {
            // delete the file on disk
            if (File.Exists(GetEncryptionKeyPathFile()))
                File.Delete(GetEncryptionKeyPathFile());
        }
        else
        {
            File.WriteAllText(GetEncryptionKeyPathFile(), fullName);
        }
    }

    private string GetEncryptionKeyPathFile() => Path.Combine(Directory.FullName, "EncryptionKeyPath");

    #region Server Defaults Persistence

    private string GetDefaultsFile() => Path.Combine(Directory.FullName, "Defaults.yaml");

    public override void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
    {
        base.SetDefault(toChange, externalDatabaseServer);

        SaveDefaults();
    }

    private void SaveDefaults()
    {
        var serializer = new Serializer();

        // save the default and the ID
        File.WriteAllText(GetDefaultsFile(),
            serializer.Serialize(Defaults.ToDictionary(k => k.Key, v => v.Value?.ID ?? 0)));
    }

    public void LoadDefaults()
    {
        var deserializer = new Deserializer();

        var defaultsFile = GetDefaultsFile();

        if (File.Exists(defaultsFile))
        {
            var yaml = File.ReadAllText(defaultsFile);
            var objectIds = deserializer.Deserialize<Dictionary<PermissableDefaults, int>>(yaml);

            // file exists but is empty
            if (objectIds == null)
                return;

            Defaults = objectIds.ToDictionary(
                k => k.Key,
                v => v.Value == 0
                    ? null
                    : (IExternalDatabaseServer)GetObjectByIDIfExists<ExternalDatabaseServer>(v.Value));
        }
    }

    /// <summary>
    /// Returns the object referenced or null if it has been deleted on the sly (e.g. by user deleting .yaml files on disk)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    private T GetObjectByIDIfExists<T>(int id) where T : DatabaseEntity
    {
        try
        {
            return GetObjectByID<T>(id);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    #endregion

    #region DataExportProperties Persistence

    private string GetDataExportPropertiesFile() => Path.Combine(Directory.FullName, "DataExportProperties.yaml");

    public void LoadDataExportProperties()
    {
        var deserializer = new Deserializer();

        var defaultsFile = GetDataExportPropertiesFile();

        if (File.Exists(defaultsFile))
        {
            var yaml = File.ReadAllText(defaultsFile);
            var props = deserializer.Deserialize<Dictionary<DataExportProperty, string>>(yaml);

            if (props != null)
                PropertiesDictionary = props;
        }
    }

    private void SaveDataExportProperties()
    {
        var serializer = new Serializer();

        // save the default and the ID
        File.WriteAllText(GetDataExportPropertiesFile(), serializer.Serialize(PropertiesDictionary));
    }

    public override void SetValue(DataExportProperty property, string value)
    {
        base.SetValue(property, value);
        SaveDataExportProperties();
    }

    #endregion

    public override void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        base.AddDataSetToPackage(package, dataSet);
        Save(PackageDictionary, nameof(PackageDictionary));
    }

    public override void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        base.RemoveDataSetFromPackage(package, dataSet);
        Save(PackageDictionary, nameof(PackageDictionary));
    }


    public override void Link(GovernancePeriod governancePeriod, ICatalogue catalogue)
    {
        base.Link(governancePeriod, catalogue);
        Save(GovernanceCoverage, nameof(GovernanceCoverage));
    }

    public override void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue)
    {
        base.Unlink(governancePeriod, catalogue);
        Save(GovernanceCoverage, nameof(GovernanceCoverage));
    }

    public override void CreateLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
    {
        base.CreateLinkBetween(configuration, tableInfo);
        Save(ForcedJoins, nameof(ForcedJoins));
    }

    public override void BreakLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
    {
        base.BreakLinkBetween(configuration, tableInfo);
        Save(ForcedJoins, nameof(ForcedJoins));
    }

    #region Persist CredentialsDictionary

    private string GetCredentialsDictionaryFile() => Path.Combine(Directory.FullName, "CredentialsDictionary.yaml");

    public void LoadCredentialsDictionary()
    {
        var deserializer = new Deserializer();

        var file = GetCredentialsDictionaryFile();

        if (File.Exists(file))
        {
            var yaml = File.ReadAllText(file);

            var ids = deserializer.Deserialize<Dictionary<int, Dictionary<DataAccessContext, int>>>(yaml);

            // file exists but is empty
            if (ids == null)
                return;

            CredentialsDictionary = new Dictionary<ITableInfo, Dictionary<DataAccessContext, DataAccessCredentials>>();

            foreach (var tableToCredentialUsage in ids)
            {
                var table = GetObjectByIDIfExists<TableInfo>(tableToCredentialUsage.Key);

                // TableInfo was deleted on the sly
                if (table == null)
                    continue;

                var valDictionary = new Dictionary<DataAccessContext, DataAccessCredentials>();
                foreach (var (usage, value) in tableToCredentialUsage.Value)
                {
                    var credential = GetObjectByIDIfExists<DataAccessCredentials>(value);

                    // Credentials can be deleted on the sly
                    if (credential != null) valDictionary.Add(usage, credential);
                }

                CredentialsDictionary.Add(table, valDictionary);
            }
        }
    }

    private void SaveCredentialsDictionary()
    {
        var serializer = new Serializer();

        var ids =
            CredentialsDictionary.ToDictionary(
                k => k.Key.ID,
                v => v.Value.ToDictionary(k => k.Key, v => v.Value.ID));

        // save the default and the ID
        File.WriteAllText(GetCredentialsDictionaryFile(), serializer.Serialize(ids));
    }

    public override void BreakAllLinksBetween(DataAccessCredentials credentials, ITableInfo tableInfo)
    {
        base.BreakAllLinksBetween(credentials, tableInfo);

        SaveCredentialsDictionary();
    }

    public override void BreakLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo,
        DataAccessContext context)
    {
        base.BreakLinkBetween(credentials, tableInfo, context);
        SaveCredentialsDictionary();
    }

    public override void CreateLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo,
        DataAccessContext context)
    {
        base.CreateLinkBetween(credentials, tableInfo, context);
        SaveCredentialsDictionary();
    }

    #endregion


    #region Persist Cohort Containers

    private void SaveCohortContainerContents(CohortAggregateContainer toSave)
    {
        var dir = Path.Combine(Directory.FullName, nameof(PersistCohortContainerContent));
        var file = Path.Combine(dir, $"{toSave.ID}.yaml");

        var serializer = new Serializer();
        var yaml = serializer.Serialize(CohortContainerContents[toSave]
            .Select(c => new PersistCohortContainerContent(c)).ToList());
        File.WriteAllText(file, yaml);
    }

    private void LoadCohortContainerContents()
    {
        var dir = new DirectoryInfo(Path.Combine(Directory.FullName, nameof(PersistCohortContainerContent)));

        if (!dir.Exists)
            dir.Create();

        var deserializer = new Deserializer();

        foreach (var f in dir.GetFiles("*.yaml").ToArray())
        {
            var id = int.Parse(Path.GetFileNameWithoutExtension(f.Name));

            var content = deserializer.Deserialize<List<PersistCohortContainerContent>>(File.ReadAllText(f.FullName));

            // file exists but is empty
            if (content == null)
                continue;

            try
            {
                CohortAggregateContainer container;

                try
                {
                    container = GetObjectByID<CohortAggregateContainer>(id);
                }
                catch (KeyNotFoundException)
                {
                    // The container doesn't exist anymore
                    f.Delete();
                    continue;
                }

                CohortContainerContents.Add(container,
                    new HashSet<CohortContainerContent>(content.Select(c => c.GetContent(this))));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file {f.FullName}", ex);
            }
        }
    }

    public override void Add(CohortAggregateContainer parent, AggregateConfiguration child, int order)
    {
        base.Add(parent, child, order);
        SaveCohortContainerContents(parent);
    }

    public override void Add(CohortAggregateContainer parent, CohortAggregateContainer child)
    {
        base.Add(parent, child);
        SaveCohortContainerContents(parent);
    }

    public override void Remove(CohortAggregateContainer parent, AggregateConfiguration child)
    {
        base.Remove(parent, child);
        SaveCohortContainerContents(parent);
    }

    public override void SetOrder(AggregateConfiguration child, int newOrder)
    {
        base.SetOrder(child, newOrder);
        SaveCohortContainerContents(child.GetCohortAggregateContainerIfAny());
    }

    public override void Remove(CohortAggregateContainer parent, CohortAggregateContainer child)
    {
        base.Remove(parent, child);
        SaveCohortContainerContents(parent);
    }

    private class PersistCohortContainerContent
    {
        public string Type { get; set; }
        public int ID { get; set; }
        public int Order { get; set; }

        public PersistCohortContainerContent()
        {
        }

        public PersistCohortContainerContent(CohortContainerContent c)
        {
            Type = c.Orderable.GetType().Name;
            ID = ((IMapsDirectlyToDatabaseTable)c.Orderable).ID;
            Order = c.Order;
        }


        public CohortContainerContent GetContent(YamlRepository repository)
        {
            if (Type.Equals(nameof(AggregateConfiguration)))
                return new CohortContainerContent(repository.GetObjectByID<AggregateConfiguration>(ID), Order);

            return Type.Equals(nameof(CohortAggregateContainer))
                ? new CohortContainerContent(repository.GetObjectByID<CohortAggregateContainer>(ID), Order)
                : throw new Exception($"Unexpected IOrderable Type name '{Type}'");
        }
    }

    #endregion

    public override void MakeIntoAnOrphan(IContainer container)
    {
        base.MakeIntoAnOrphan(container);
        SaveWhereSubContainers();
    }

    public override void AddSubContainer(IContainer parent, IContainer child)
    {
        base.AddSubContainer(parent, child);
        SaveWhereSubContainers();
    }

    private void SaveWhereSubContainers()
    {
        Save(WhereSubContainers.Where(kvp => kvp.Key is FilterContainer)
            .ToDictionary(
                k => k.Key,
                v => v.Value), "ExtractionFilters");

        Save(WhereSubContainers.Where(kvp => kvp.Key is AggregateFilterContainer)
            .ToDictionary(
                k => k.Key,
                v => v.Value), "AggregateFilters");
    }

    public override string ToString() => $"{{YamlRepository {Directory.FullName}}}";

    private void LoadWhereSubContainers()
    {
        foreach (var c in Load<FilterContainer, FilterContainer>("ExtractionFilters") ??
                          new Dictionary<FilterContainer, HashSet<FilterContainer>>())
            WhereSubContainers.Add(c.Key, new HashSet<IContainer>(c.Value));
        foreach (var c in Load<AggregateFilterContainer, AggregateFilterContainer>("AggregateFilters") ??
                          new Dictionary<AggregateFilterContainer, HashSet<AggregateFilterContainer>>())
            WhereSubContainers.Add(c.Key, new HashSet<IContainer>(c.Value));
    }

    private Dictionary<T, HashSet<T2>> Load<T, T2>(string filenameWithoutSuffix)
        where T : IMapsDirectlyToDatabaseTable
        where T2 : IMapsDirectlyToDatabaseTable
    {
        var deserializer = new Deserializer();

        var file = Path.Combine(Directory.FullName, $"{filenameWithoutSuffix}.yaml");

        if (File.Exists(file))
        {
            var yaml = File.ReadAllText(file);

            var dictionary = new Dictionary<T, HashSet<T2>>();

            var dict = deserializer.Deserialize<Dictionary<int, List<int>>>(yaml);

            //file exists but is empty
            if (dict == null)
                return null;

            foreach (var ids in dict)
                try
                {
                    var key = GetObjectByID<T>(ids.Key);

                    var set = new HashSet<T2>();

                    foreach (var val in ids.Value)
                        try
                        {
                            set.Add(GetObjectByID<T2>(val));
                        }
                        catch (KeyNotFoundException)
                        {
                            // skip missing objects (they will disappear next save anyway)
                        }

                    dictionary.Add(key, set);
                }
                catch (KeyNotFoundException)
                {
                    // skip missing container objects (they will disappear next save anyway)
                }

            return dictionary;
        }

        return new Dictionary<T, HashSet<T2>>();
    }

    private void Save<T, T2>(Dictionary<T, HashSet<T2>> collection, string filenameWithoutSuffix)
        where T : IMapsDirectlyToDatabaseTable
        where T2 : IMapsDirectlyToDatabaseTable
    {
        var file = Path.Combine(Directory.FullName, $"{filenameWithoutSuffix}.yaml");
        var serializer = new Serializer();

        // save the default and the ID
        File.WriteAllText(file, serializer.Serialize(
            collection.ToDictionary(
                k => k.Key.ID,
                v => v.Value.Select(c => c.ID).ToList()
            )));
    }
}