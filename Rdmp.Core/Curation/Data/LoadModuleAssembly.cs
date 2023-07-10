// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;
using YamlDotNet.Serialization;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// This entity stores the large binary blob for the <see cref="Plugin"/> class.  The <see cref="Bin"/> can be written to disk
/// as a nuget file which can be loaded at runtime to add plugin functionality for rdmp.
/// </summary>
public class LoadModuleAssembly : DatabaseEntity, IInjectKnown<Plugin>
{
    #region Database Properties
    private byte[] _bin;
    private string _committer;
    private DateTime _uploadDate;
    private int _plugin_ID;
    private Lazy<Plugin> _knownPlugin;


    /// <summary>
    /// The assembly (dll) file as a Byte[], use File.WriteAllBytes to write it to disk
    /// </summary>
    [YamlIgnore]
    public byte[] Bin
    {
        get => _bin;
        set => SetField(ref _bin,value);
    }

    /// <summary>
    /// The user who uploaded the dll
    /// </summary>
    public string Committer
    {
        get => _committer;
        set => SetField(ref _committer,value);
    }

    /// <summary>
    /// The date the dll was uploaded
    /// </summary>
    public DateTime UploadDate
    {
        get => _uploadDate;
        set => SetField(ref _uploadDate,value);
    }

    /// <summary>
    /// The plugin this file forms a part of (each <see cref="Plugin"/> will usually have multiple dlls as part of its dependencies)
    /// </summary>
    [Relationship(typeof(Plugin), RelationshipType.SharedObject)]
    public int Plugin_ID
    {
        get => _plugin_ID;
        set => SetField(ref _plugin_ID,value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="Plugin_ID"/>
    [NoMappingToDatabase]
    public Plugin Plugin => _knownPlugin.Value;

    #endregion

    public LoadModuleAssembly()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Uploads the given dll file to the catalogue database ready for use as a plugin within RDMP (also uploads any pdb file in the same dir)
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="f"></param>
    /// <param name="plugin"></param>
    public LoadModuleAssembly(ICatalogueRepository repository, FileInfo f, Plugin plugin)
    {
        var dictionaryParameters = GetDictionaryParameters(f, plugin);

        //so we can reference it in fetch requests to check for duplication (normally Repository is set during hydration by the repo)
        Repository = repository;

        Repository.InsertAndHydrate(this,dictionaryParameters);
        ClearAllInjections();
    }

    internal LoadModuleAssembly(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Bin = r["Bin"] as byte[];
        Committer = r["Committer"] as string;
        UploadDate = Convert.ToDateTime(r["UploadDate"]);
        Plugin_ID = Convert.ToInt32(r["Plugin_ID"]);
        ClearAllInjections();
    }

    internal LoadModuleAssembly(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
        ClearAllInjections();
    }

    /// <summary>
    /// Unpack the plugin DLL files, excluding any Windows specific dlls when not running on Windows
    /// </summary>
    public IEnumerable<ValueTuple<string,MemoryStream>> GetContents()
    {
        var isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
        using var ms = new MemoryStream(Bin);
        using var zip = new ZipFile(ms);
        foreach (ZipEntry e in zip)
        {
            if (!e.IsFile || !e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;
            if (isWin && e.Name.Contains("/windows/")) continue;
            using var s = zip.GetInputStream(e);
            using var ms2 = new MemoryStream();
            s.CopyTo(ms2);
            ms2.Position = 0;
            yield return (e.Name,ms2);
        }
    }

    /// <summary>
    /// Downloads the plugin nupkg to the given directory
    /// </summary>
    /// <param name="downloadDirectory"></param>
    public string DownloadAssembly(DirectoryInfo downloadDirectory)
    {
        var targetDirectory = downloadDirectory.FullName;
        if (!downloadDirectory.Exists)
            downloadDirectory.Create();

        var targetFile = Path.Combine(targetDirectory, Plugin.Name);
            
        //file already exists
        if (File.Exists(targetFile))
            if(AreEqual(File.ReadAllBytes(targetFile), Bin))
                return targetFile;

        var timeout = 5000;

        TryAgain:
        try
        {
            //if it has changed length or does not exist, write it out to the disk
            File.WriteAllBytes(targetFile, Bin);
        }
        catch (Exception)
        {
            timeout -= 100;
            Thread.Sleep(100);

            if (timeout <= 0)
                throw;

            goto TryAgain;
        }

        return targetFile;
    }

    private Dictionary<string, object> GetDictionaryParameters(FileInfo f, Plugin plugin)
    {
        if(f.Extension != PackPluginRunner.PluginPackageSuffix)
            throw new Exception($"Expected LoadModuleAssembly file to be a {PackPluginRunner.PluginPackageSuffix}");

        var allBytes = File.ReadAllBytes(f.FullName);

        var dictionaryParameters = new Dictionary<string, object>
        {
            {"Bin",allBytes},
            {"Committer",Environment.UserName},
            {"Plugin_ID",plugin.ID}
        };

        return dictionaryParameters;
    }

    private static bool AreEqual(byte[] readAllBytes, byte[] dll)
    {
        return readAllBytes.Length == dll.Length && !dll.Where((t, i) => !readAllBytes[i].Equals(t)).Any();
    }

    public void InjectKnown(Plugin instance)
    {
        _knownPlugin = new Lazy<Plugin>(instance);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"LoadModuleAssembly_{ID}";
    }

    public void ClearAllInjections()
    {
        _knownPlugin = new Lazy<Plugin>(() => Repository.GetObjectByID<Plugin>(Plugin_ID));
    }
}