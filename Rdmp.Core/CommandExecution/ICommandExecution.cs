// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Describes an executable command that is available to the user (or assembled and executed in code).  ICommandExecution instances are allowed to be in
    /// illegal states (IsImpossible = true) and this should be respected by Execute i.e. it will throw an exception if Executed.
    /// </summary>
    public interface ICommandExecution
    {
        /// <summary>
        /// True if the command's current state means that it cannot be succesfully executed.  This should ideally be set in the constructor logic
        /// but must be set before <see cref="Execute"/> is called.  
        /// 
        /// <para>If determining a given error state is expensive and therefore not desireable to do in the constructor you should just determine
        /// it in <see cref="Execute"/> and throw at that point</para>
        /// </summary>
        bool IsImpossible { get; }

        /// <summary>
        /// The reason <see cref="IsImpossible"/> is true.  Should be null if <see cref="IsImpossible"/> is false
        /// </summary>
        string ReasonCommandImpossible { get; }
        
        /// <summary>
        /// Runs the command, this should normally only be called once then the command discarded.  Always ensure when overriding that you call base members
        /// because they can include logic such as <see cref="IsImpossible"/> etc.
        /// </summary>
        void Execute();

        /// <summary>
        /// The user friendly name for the command, this is what is rendered in UIs/menus etc if the command is being shown in one
        /// </summary>
        /// <returns></returns>
        string GetCommandName();

        /// <summary>
        /// User understandable description of what the command does in it's current state.  This can change depending on constructor arguments etc.
        /// </summary>
        /// <returns></returns>
        string GetCommandHelp();
    }
}