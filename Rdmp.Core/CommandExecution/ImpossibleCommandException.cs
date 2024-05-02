// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     Thrown when the API tries to Execute a command marked IsImpossible
/// </summary>
public class ImpossibleCommandException : Exception
{
    public ICommandExecution Command { get; private set; }
    public string ReasonCommandImpossible { get; private set; }

    public ImpossibleCommandException(ICommandExecution command, string reasonCommandImpossible)
        : base($"Command is marked as IsImpossible and should not be Executed.  Reason is '{reasonCommandImpossible}'")
    {
        Command = command;
        ReasonCommandImpossible = reasonCommandImpossible;
    }
}