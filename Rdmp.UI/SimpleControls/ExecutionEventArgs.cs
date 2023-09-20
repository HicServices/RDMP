// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandLine.Runners;

namespace Rdmp.UI.SimpleControls;

/// <summary>
/// EventArgs describing the final exit code of an <see cref="IRunner"/> running in <see cref="CheckAndExecuteUI"/> (this will be the exit code that
/// would have been returned had the <see cref="IRunner"/> been running on the command line rdmp.exe).
/// </summary>
public class ExecutionEventArgs : EventArgs
{
    public int? ExitCode { get; set; }

    public ExecutionEventArgs(int exitCode)
    {
        ExitCode = exitCode;
    }
}