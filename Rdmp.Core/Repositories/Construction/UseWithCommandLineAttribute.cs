// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandLine.Interactive.Picking;

namespace Rdmp.Core.Repositories.Construction;

/// <summary>
///     Indicates a constructor which should be used when instantiating from the command line with an unknown number of
///     arguments.
///     Constructors decorated with this attribute must take a <see cref="CommandLineObjectPicker" />
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class UseWithCommandLineAttribute : Attribute
{
    /// <summary>
    ///     List of the expected arguments the command should take in a format suitable for displaying in CLI help e.g. "&lt;
    ///     param1&gt; &lt;param2&gt;"
    /// </summary>
    public string ParameterHelpList { get; set; } = @"<dynamic>";

    /// <summary>
    ///     Help for each parameter listed in <see cref="ParameterHelpList" /> with descriptions of what you expect to be in
    ///     them (suitable for displaying in CLI)
    /// </summary>
    public string ParameterHelpBreakdown { get; set; } = @"Unknown";
}