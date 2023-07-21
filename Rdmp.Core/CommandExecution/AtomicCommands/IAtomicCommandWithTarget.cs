// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     An executable command with variable target.  SetTarget should be obvious based on your class name e.g.
///     ExecuteCommandRelease (pass a Project to release).
///     <para>
///         In general you should also provide a constructor overload that hydrates the command properly decorated with
///         [UseWithObjectConstructor] so that it is
///         useable with RunUI
///     </para>
/// </summary>
public interface IAtomicCommandWithTarget : IAtomicCommand
{
    /// <summary>
    ///     Defines the object which this command should operate on
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    IAtomicCommandWithTarget SetTarget(DatabaseEntity target);
}