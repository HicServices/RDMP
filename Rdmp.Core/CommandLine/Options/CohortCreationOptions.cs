// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CommandLine;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Command line options for the Cohort Creation Pipelines
/// </summary>
[Verb("cohort", HelpText = "Runs the Cohort Creation")]
public class CohortCreationOptions : RDMPCommandLineOptions
{
    // Used for refreshes:
    [Option('e', "ExtractionConfiguration", HelpText = "The ExtractionConfiguration ID to extract", Required = true)]
    public string ExtractionConfiguration { get; set; }

    // Other Options:
    /*
     * External Cohort Table
     *
     * Project (only existing?)
     *
     * New Cohort / Revision?
     *
     * Name (or existing cohort name/id?)
     *
     * Pipeline
     *
     * Description
     *
     */
}