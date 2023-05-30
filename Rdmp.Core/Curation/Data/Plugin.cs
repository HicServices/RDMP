// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// A nupkg file which contains compiled code to add additional capabilities to RDMP (e.g. to handle Dicom images).  Plugins are loaded and
/// stored in the RDMP platform databases and written to disk/loaded when executed by the RDMP client - this ensures that all users run the same
/// version of the Plugin(s).
/// </summary>
public class Plugin : DatabaseEntity,INamed
{
    #region Database Properties

    private string _name;
    private string _uploadedFromDirectory;
    private Version _pluginVersion;
    private Version _rdmpVersion;

    /// <inheritdoc/>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref  _name, value);
    }

    /// <summary>
    /// Where the plugin files were uploaded from
    /// </summary>
    public string UploadedFromDirectory
    {
        get => _uploadedFromDirectory;
        set => SetField(ref  _uploadedFromDirectory, value);
    }

        
        
    /// <summary>
    /// Returns <see cref="Name"/> without the verison e.g. "Rdmp.Dicom" from an ambigious name:
    ///  Rdmp.Dicom.0.0.1.nupkg 
    ///  Rdmp.Dicom.nupkg 
    ///  Rdmp.Dicom
    /// </summary>
    /// <returns></returns>
    public string GetShortName()
    {
        var regexSuffix = new Regex(@"(\.\d*)*(\.nupkg)?$");
        return regexSuffix.Replace(Name,"");
    }

    /// <summary>
    /// The master version of the <see cref="Plugin"/>
    /// <para>Not currently used</para>
    /// </summary>
    public Version PluginVersion
    {
        get => _pluginVersion;
        set => SetField(ref  _pluginVersion, value);
    }

    /// <summary>
    /// The version of RDMP which the plugin is compatible with.  This is determined by looking at the dependencies tag in
    /// the nuspec file of the nupkg being uploaded.
    /// </summary>
    public Version RdmpVersion
    {
        get => _rdmpVersion;
        set => SetField(ref  _rdmpVersion, value);
    }

    #endregion

    public Plugin()
    {

    }

    /// <summary>
    /// Defines a new collection of dlls that provide plugin functionality for RDMP
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="pluginZipFile"></param>
    /// <param name="pluginVersion"></param>
    /// <param name="rdmpVersion"></param>
    public Plugin(ICatalogueRepository repository, FileInfo pluginZipFile, Version pluginVersion, Version rdmpVersion)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>()
        {
            {"Name", pluginZipFile.Name},
            {"UploadedFromDirectory", pluginZipFile.DirectoryName},
            {"PluginVersion", (pluginVersion ?? new Version(0,0,0,0))},
            {"RdmpVersion", (rdmpVersion ?? new Version(0,0,0,0))}
        });
            
    }

    internal Plugin(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Name = r["Name"].ToString();
        UploadedFromDirectory = r["UploadedFromDirectory"].ToString();

        try
        {
            PluginVersion = new Version((string)r["PluginVersion"]);
        }
        catch (ArgumentException)
        {
            PluginVersion = new Version("0.0.0.0");//user hacked database and typed in 'I've got a lovely bunch of coconuts' into the version field?
        }
        try
        {
            RdmpVersion = new Version((string)r["RdmpVersion"]);
        }
        catch (ArgumentException)
        {
            RdmpVersion = new Version("0.0.0.0");//user hacked database and typed in 'I've got a lovely bunch of coconuts' into the version field?
        }
    }

    internal Plugin(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
    }

    #region Relationships

    /// <summary>
    /// Gets all the dlls and source code(if available) stored as <see cref="LoadModuleAssembly"/> in the catalogue database
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<LoadModuleAssembly> LoadModuleAssemblies => Repository.GetAllObjectsWithParent<LoadModuleAssembly>(this);

    #endregion

    /// <summary>
    /// Returns a folder name suitable for storing the dlls for the plugin in as a subdirectory of 
    /// <paramref name="downloadDirectoryRoot"/>
    /// </summary>
    /// <param name="downloadDirectoryRoot"></param>
    /// <returns></returns>
    public string GetPluginDirectoryName(DirectoryInfo downloadDirectoryRoot)
    {
        var pluginName = Path.GetFileNameWithoutExtension(Name);

        if(string.IsNullOrWhiteSpace(pluginName))
            throw new Exception("Plugin doens't have a valid name");

        return Path.Combine(downloadDirectoryRoot.FullName ,pluginName);
    }
}