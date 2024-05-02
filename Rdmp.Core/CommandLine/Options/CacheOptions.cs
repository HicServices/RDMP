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
///     Command line options for the caching engine
/// </summary>
[Verb("cache",
    HelpText =
        "Run the Caching engine which fetches data by date from a remote endpoint in batches of a given size (independently from loading it to any relational databases)")]
public class CacheOptions : RDMPCommandLineOptions
{
    [Option('c', "CacheProgress", HelpText = "The ID of the CacheProgress you want to run", Required = true,
        Default = "0")]
    public string CacheProgress { get; set; }

    [Option('r', "RetryMode",
        HelpText = "True to attempt to process archival CacheFetchFailure dates instead of new (uncached) dates.")]
    public bool RetryMode { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Check the cache is runnable",
                new CacheOptions { Command = CommandLineActivity.check, CacheProgress = "2" });
            yield return new Example("Check the cache is runnable and returns error code " +
                                     "instead of success if there are warnings",
                new CacheOptions { Command = CommandLineActivity.check, CacheProgress = "2", FailOnWarnings = true });
            yield return new Example("Run cache progress overriding RDMP platform databases (specified in .config)",
                new CacheOptions
                {
                    Command = CommandLineActivity.run,
                    CacheProgress = "2",
                    ServerName = @"localhost\sqlexpress",
                    CatalogueDatabaseName = "RDMP_Catalogue",
                    DataExportDatabaseName = "RDMP_DataExport"
                });
        }
    }
}