// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Describes a series of commands to run in sequence.
/// </summary>
public class RdmpScript
{
    /// <summary>
    ///     Commands which should be run by a <see cref="ExecuteCommandRunner" />
    /// </summary>
    /// <value></value>
    public string[] Commands { get; set; }

    /// <summary>
    ///     True (default) to use a <see cref="NewObjectPool" /> to track objects created as the script is run
    ///     to improve argument matching when there are multiple objects that match a parameter e.g. CatalogueItem:chi
    /// </summary>
    public bool UseScope { get; set; } = true;
}