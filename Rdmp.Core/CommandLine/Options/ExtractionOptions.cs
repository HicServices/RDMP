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
///     Options for the Extraction Engine which performs cohort linkage against datasets and extracts anonymous datasets
/// </summary>
[Verb("extract", HelpText = "Runs the Data Extraction Engine")]
public class ExtractionOptions : ConcurrentRDMPCommandLineOptions
{
    [Option('g', "Globals", HelpText = "Include extraction of globals (global SupportingDocuments etc")]
    public bool ExtractGlobals { get; set; }

    [Option('e', "ExtractionConfiguration", HelpText = "The ExtractionConfiguration ID to extract", Required = true)]
    public string ExtractionConfiguration { get; set; }

    [Option('p', "Pipeline", HelpText = "The ID of the extraction Pipeline to use", Required = true)]
    public string Pipeline { get; set; }

    [Option('s', "Datasets",
        HelpText =
            "Restrict extraction to only those ExtractableDatasets that have the provided list of IDs (must be part of the ExtractionConfiguration)")]
    public string Datasets { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Check dataset 123 and 124 in configuration 32 for extraction using pipeline 2",
                new ExtractionOptions
                {
                    Command = CommandLineActivity.check,
                    ExtractionConfiguration = "32",
                    Pipeline = "2",
                    Datasets = "123,124"
                }
            );
        }
    }
}