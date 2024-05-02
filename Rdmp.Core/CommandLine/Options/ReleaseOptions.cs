// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CommandLine;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Options for the Release Engine which is responsible for gathering all the artifacts produced by the Extraction
///     Engine (anonymised project extracts, bundled lookups and documents etc)
///     and transmitting them somewhere as a final released package.
/// </summary>
[Verb("release",
    HelpText =
        "Releases one or more ExtractionConfigurations (e.g. Cases & Controls) for an extraction Project that has been successfully extracted via the Extraction Engine (see extract command)")]
public class ReleaseOptions : ConcurrentRDMPCommandLineOptions
{
    [Option('c', "Configurations",
        HelpText = "List of ExtractionConfiguration IDs to release, they must all belong to the same Project")]
    public string Configurations { get; set; }

    [Option('p', "Pipeline", HelpText = "The ID of the release Pipeline to use")]
    public string Pipeline { get; set; }

    [Option('s', "SelectedDatasets",
        HelpText =
            "List of SelectedDatasets IDs to release, they must all belong to ExtractionConfigurations within the same Project")]
    public string SelectedDataSets { get; set; }

    [Option('g', "Globals", HelpText = "True to release extracted globals (default) or false to skip them",
        Default = true)]
    public bool? ReleaseGlobals { get; set; }

    [Option(HelpText = "True to skip any ExtractionConfiguration that are already released")]
    public bool SkipReleased { get; internal set; }
}