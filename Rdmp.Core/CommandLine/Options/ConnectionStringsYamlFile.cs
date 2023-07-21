// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Holds connection strings to an RDMP database along with name and description of what those databases
///     are in the organisation (human readable descriptions).  This class assists with persistence and is designed
///     to be a helper for <see cref="RDMPCommandLineOptions.ConnectionStringsFile" />
/// </summary>
public class ConnectionStringsYamlFile
{
    /// <summary>
    ///     Connection string to the RDMP main catalogue metadata database
    /// </summary>
    public string CatalogueConnectionString { get; set; }

    /// <summary>
    ///     Connection string to the RDMP secondary (Data export) metadata database
    /// </summary>
    public string DataExportConnectionString { get; set; }

    /// <summary>
    ///     Optional user readable name for these settings e.g. 'Testing Instance'
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Optional user readable description of the role of the instance
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     The file on disk that was read to populate the properties in this class
    /// </summary>
    [YamlIgnore]
    public FileInfo FileLoaded { get; set; }

    /// <summary>
    ///     Reads the yaml in <paramref name="f" /> and returns a deserialized instance of
    ///     <see cref="ConnectionStringsYamlFile" />
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static ConnectionStringsYamlFile LoadFrom(FileInfo f)
    {
        var deserializer = new Deserializer();
        var toReturn = deserializer.Deserialize<ConnectionStringsYamlFile>(File.ReadAllText(f.FullName));

        if (toReturn == null || string.IsNullOrWhiteSpace(toReturn.CatalogueConnectionString))
            throw new Exception(
                $"{nameof(CatalogueConnectionString)} is missing from the RDMP connection strings file: '{f.FullName}'");

        toReturn.FileLoaded = f;
        return toReturn;
    }

    public static bool TryLoadFrom(FileInfo f, out ConnectionStringsYamlFile result)
    {
        try
        {
            result = LoadFrom(f);
            return true;
        }
        catch (Exception)
        {
            result = null;
            return false;
        }
    }
}