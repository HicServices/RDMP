// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Command line options for the Data Load Engine
/// </summary>
[Verb("dle", HelpText = "Runs the Data Load Engine")]
public class DleOptions : RDMPCommandLineOptions
{
    [Option('l', "LoadMetadata",
        HelpText = "The ID of the LoadMetadata you want to run or pattern e.g. LoadMetadata:Load*bob", Required = false,
        Default = "0")]
    public string LoadMetadata { get; set; }

    [Option('p', "LoadProgress",
        HelpText =
            "If your LoadMetadata has multiple LoadProgresses, you can run only one of them by specifying the ID of the LoadProgress to run here",
        Required = false, Default = "0")]
    public string LoadProgress { get; set; }

    [Option('i', "Iterative",
        HelpText =
            "If the LoadMetadata has LoadProgress(es) then they will be run until available data is exhausted (if false then only one batch will be loaded e.g. 5 days)",
        Required = false, Default = false)]
    public bool Iterative { get; set; }

    [Option('d', "DaysToLoad", HelpText = "Only applies if using a LoadProgress, overrides how much is loaded at once")]
    public int? DaysToLoad { get; set; }

    [Option(HelpText = "Do not copy files from ForLoading into the file Archive")]
    public bool DoNotArchiveData { get; set; }

    [Option(HelpText = "Abort the data load after populating the RAW environment only")]
    public bool StopAfterRAW { get; set; }

    [Option(HelpText =
        "Abort the data load after populating the STAGING environment (no data will be merged with LIVE)")]
    public bool StopAfterSTAGING { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Run load LoadMetadata with ID 30",
                new DleOptions { Command = CommandLineActivity.run, LoadMetadata = "30" });
            yield return new Example("Override for RDMP platform databases (specified in .config)",
                new DleOptions
                {
                    Command = CommandLineActivity.run,
                    LoadMetadata = "LoadMetadata:Loading*Biochem*",
                    ServerName = @"localhost\sqlexpress",
                    CatalogueDatabaseName = "RDMP_Catalogue",
                    DataExportDatabaseName = "RDMP_DataExport"
                });
            yield return new Example("Use explicit connection strings to RDMP databases",
                new DleOptions
                {
                    Command = CommandLineActivity.run,
                    LoadMetadata = "30",
                    CatalogueConnectionString = ExampleCatalogueConnectionString,
                    DataExportConnectionString = ExampleDataExportConnectionString
                });
        }
    }
}