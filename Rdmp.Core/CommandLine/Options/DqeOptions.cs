// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CommandLine;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
/// Command line parameters for the Data Quality Engine
/// </summary>
[Verb("dqe", HelpText = "Runs the Data Quality Engine")]
public class DqeOptions : RDMPCommandLineOptions
{
    [Option('c', "Catalogue", HelpText = "ID of the Catalogue to run the DQE on", Required = true)]
    public string Catalogue { get; set; }

    [Option('d', "DataLoad", HelpText = "ID of the Data Load to run the DQE on. Adds new data to existing DQE results if they exist", Required = false)]
    public string DataLoadUpdateID { get; set; }
}