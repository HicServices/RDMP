// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CommandLine;

namespace Rdmp.Core.CommandLine.Options.Abstracts
{
    [Verb("list",HelpText = "Lists objects in the Catalogue / DataExport repositories")]
    public class ListOptions : RDMPCommandLineOptions
    {
        [Option('t', "Type",Required = false, HelpText = "Type name you want to list e.g. Catalogue (does not have to be fully specified)")]
        public string Type { get; set; }

        [Option('p',"Pattern",Required = false, HelpText = "Regex pattern to match on ToString of class",Default =  ".*")]
        public string Pattern { get; set; }

        [Option('i',"ID",Required = false, HelpText = "The ID of the object to fetch")]
        public int? ID { get; set; }
    }
}