// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandLine.Options;

namespace Rdmp.UI;

/// <summary>
///     Defines the command line arguments of ResearchDataManagementPlatform.exe when run from the command line / shortcut
/// </summary>
public class ResearchDataManagementPlatformOptions : RDMPCommandLineOptions
{
    protected override bool ShouldLoadHelp()
    {
        return true;
    }
}