// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Rdmp.Core.CommandLine.Runners;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Command line arguments for uploading plugins
/// </summary>
[Verb("pack", HelpText = "Uploads a new RDMP plugin into the database")]
public class PackOptions : RDMPCommandLineOptions
{
    [Option('f', "file", Required = true, HelpText = $"The {PackPluginRunner.PluginPackageSuffix} plugin file to add")]
    public string File { get; set; }

    [Option('p', "prune", HelpText =
        $"Modifies the {PackPluginRunner.PluginPackageSuffix} plugin file by removing duplicate dlls or those already contained in RDMP core before uploading to database")]
    public bool Prune { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Commit Plugin to Rdmp", new PackOptions
            {
                File = $@"MyPlugin{PackPluginRunner.PluginPackageSuffix}",
                CatalogueConnectionString = ExampleCatalogueConnectionString,
                DataExportConnectionString = ExampleDataExportConnectionString
            });
        }
    }
}