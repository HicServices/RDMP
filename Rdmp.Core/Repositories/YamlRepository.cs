// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapsDirectlyToDatabaseTable;
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


}
