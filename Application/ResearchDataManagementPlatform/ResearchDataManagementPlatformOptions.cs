// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ResearchDataManagementPlatform
{
    /// <summary>
    /// Defines the command line arguments of ResearchDataManagementPlatform.exe when run from the command line / shortcut
    /// </summary>
    public class ResearchDataManagementPlatformOptions
    {
        [Option('c', Required = false, HelpText = @"Connection string to the main Catalogue RDMP database")]
        public string CatalogueConnectionString { get; set; }

        [Option('d', Required = false, HelpText = @"Connection string to the main DataExport RDMP database")]
        public string DataExportConnectionString { get; set; }


        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Load Default User Connection String Settings", new ResearchDataManagementPlatformOptions());
                yield return new Example("Use These Connection Strings", new ResearchDataManagementPlatformOptions { CatalogueConnectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=RDMP_Catalogue;Integrated Security=True", DataExportConnectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=RDMP_DataExport;Integrated Security=True" });
            }
        }
    }
}