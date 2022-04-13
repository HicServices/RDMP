// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories.Managers;
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

    object lockFs = new object();

    public YamlRepository(DirectoryInfo dir)
    {
        Directory = dir;

        if (!Directory.Exists)
            Directory.Create();

        // Build the serializer
        var builder = new SerializerBuilder();

        foreach(var type in GetCompatibleTypes())
        {
            var respect = TableRepository.GetPropertyInfos(type);

            foreach(var prop in type.GetProperties())
            {
                if(!respect.Contains(prop))
                {
                    builder = builder.WithAttributeOverride(type, prop.Name, new YamlIgnoreAttribute());
                }
            }
        }

        _serializer = builder.Build();

        LoadObjects();

        if (File.Exists(GetEncryptionKeyPathFile()))
            EncryptionKeyPath = File.ReadAllText(GetEncryptionKeyPathFile());

        MEF = new MEF();

        // Don't create new objects with the ID of existing objects
        NextObjectId = Objects.Count == 0 ? 0 : Objects.Max(o => o.Key.ID);
    }

    private void LoadObjects()
    {
        var subdirs = Directory.GetDirectories();
        var deserializer = new Deserializer();

        foreach (var t in GetCompatibleTypes())
        {
            // find the directory that contains all the YAML files e.g. MyDir/Catalogue/
            var typeDir = subdirs.FirstOrDefault(d => d.Name.Equals(t.Name));

            if (typeDir == null)
            {
                Directory.CreateSubdirectory(t.Name);
                continue;
            }
            
            lock(lockFs)
            {
                foreach (var yaml in typeDir.EnumerateFiles("*.yaml"))
                {
                    var obj = (IMapsDirectlyToDatabaseTable)deserializer.Deserialize(File.ReadAllText(yaml.FullName), t);
                    SetRepositoryOnObject(obj);
                    Objects.TryAdd(obj, 0);
                }
            }
        }

        LoadDefaults();

        LoadDataExportProperties();

        LoadPackageContents();
    }

    /// <summary>
    /// Sets <see cref="IMapsDirectlyToDatabaseTable.Repository"/> on <paramref name="obj"/>.
    /// Override to also set other destination repo specific fields
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void SetRepositoryOnObject(IMapsDirectlyToDatabaseTable obj)
    {
        obj.Repository = this;
    }

    public override void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters)
    {
        base.InsertAndHydrate(toCreate, constructorParameters);

        // put it on disk
        lock(lockFs)
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
    private string GetPath(IMapsDirectlyToDatabaseTable o)
    {
        return Path.Combine(Directory.FullName, o.GetType().Name, o.ID + ".yaml");
    }

    public override void DeleteEncryptionKeyPath()
    {
        base.DeleteEncryptionKeyPath();

        if(File.Exists(GetEncryptionKeyPathFile()))
            File.Delete(GetEncryptionKeyPathFile());
    }
    public override void SetEncryptionKeyPath(string fullName)
    {
        base.SetEncryptionKeyPath(fullName);

        // if setting it to null
        if (string.IsNullOrWhiteSpace(fullName)) {

            // delete the file on disk
            if (File.Exists(GetEncryptionKeyPathFile()))
                File.Delete(GetEncryptionKeyPathFile());
        }
        else
            File.WriteAllText(GetEncryptionKeyPathFile(), fullName);
    }
    private string GetEncryptionKeyPathFile()
    {
        return Path.Combine(Directory.FullName, "EncryptionKeyPath");
    }

    #region Server Defaults Persistence
    private string GetDefaultsFile()
    {
        return Path.Combine(Directory.FullName, "Defaults.yaml");
    }

    public override void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
    {
        base.SetDefault(toChange, externalDatabaseServer);

        SaveDefaults();
    }

    private void SaveDefaults()
    {
        var serializer = new Serializer();

        // save the default and the ID
        File.WriteAllText(GetDefaultsFile(),serializer.Serialize(Defaults.ToDictionary(k=>k.Key,v=>v.Value?.ID ?? 0)));
    }

    public void LoadDefaults()
    {
        var deserializer = new Deserializer();

        var defaultsFile = GetDefaultsFile();

        if(File.Exists(defaultsFile))
        {
            var yaml = File.ReadAllText(defaultsFile);
            var objectIds = deserializer.Deserialize<Dictionary<PermissableDefaults, int>>(yaml);
            Defaults = objectIds.ToDictionary(
                k=>k.Key,
                v=>v.Value == 0 ? null : (IExternalDatabaseServer)GetObjectByID<ExternalDatabaseServer>(v.Value));
        }
    }
    #endregion

    #region DataExportProperties Persistence
    private string GetDataExportPropertiesFile()
    {
        return Path.Combine(Directory.FullName, "DataExportProperties.yaml");
    }
    public void LoadDataExportProperties()
    {
        var deserializer = new Deserializer();

        var defaultsFile = GetDataExportPropertiesFile();

        if (File.Exists(defaultsFile))
        {
            var yaml = File.ReadAllText(defaultsFile);
            PropertiesDictionary = deserializer.Deserialize<Dictionary<DataExportProperty, string>>(yaml);
        }
    }
    private void SaveDataExportProperties()
    {
        var serializer = new Serializer();

        // save the default and the ID
        File.WriteAllText(GetDataExportPropertiesFile(), serializer.Serialize(PropertiesDictionary));
    }
    public override string GetValue(DataExportProperty property)
    {
        return base.GetValue(property);
    }
    public override void SetValue(DataExportProperty property, string value)
    {
        base.SetValue(property, value);
        SaveDataExportProperties();
    }
    #endregion

    #region Persist Package Contents

    private string GetPackageContentsFile()
    {
        return Path.Combine(Directory.FullName, "PackageContents.yaml");
    }
    public void LoadPackageContents()
    {
        var deserializer = new Deserializer();

        var packageContentFile = GetPackageContentsFile();

        if (File.Exists(packageContentFile))
        {
            var yaml = File.ReadAllText(packageContentFile);
            PackageDictionary = deserializer.Deserialize<Dictionary<int, List<int>>>(yaml)
                .ToDictionary(
                    k => GetObjectByID<IExtractableDataSetPackage>(k.Key),
                    v => new HashSet<IExtractableDataSet>(v.Value.Select(v => GetObjectByID<ExtractableDataSet>(v))));
        }
    }
    private void SavePackageContents()
    {
        var serializer = new Serializer();

        // save the default and the ID
        File.WriteAllText(GetPackageContentsFile(), serializer.Serialize(GetPackageContentsDictionary()));
    }

    public override void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        base.AddDataSetToPackage(package, dataSet);

        SavePackageContents();
    }

    public override void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        base.RemoveDataSetFromPackage(package, dataSet);

        SavePackageContents();
    }

    #endregion

}
