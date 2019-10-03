// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using ReusableLibraryCode;
using ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Basic implementation of ICommandExecution ensures that if a command is marked IsImpossible then it cannot be run.  Call SetImpossible to render your command 
    /// un runnable with the given arguments.  You cannot make an IsImpossible command Possible again (therefore you should probably make this discision in your 
    /// constructor).  Override Execute to provide the implementation logic of your command but make sure to leave the base.Execute() call in first to ensure 
    /// IsImpossible is respected in the unlikely event that some code or user attempts to execute an impossible command.
    /// 
    /// <para>Override GetCommandHelp and GetCommandName to change the persentation layer of the command (if applicable).</para>
    /// </summary>
    public abstract class BasicCommandExecution : ICommandExecution
    {
        public bool IsImpossible { get; private set; }
        public string ReasonCommandImpossible { get; private set; }
        public string OverrideCommandName { get; set; }

        
        /// <summary>
        /// True to add "..." to the end of the <see cref="GetCommandName"/>
        /// </summary>
        protected bool UseTripleDotSuffix { get; set; }


        public virtual void Execute()
        {
            if(IsImpossible)
                throw new ImpossibleCommandException(this, ReasonCommandImpossible);
        }


        public virtual string GetCommandName()
        {
            if (!string.IsNullOrWhiteSpace(OverrideCommandName))
                return OverrideCommandName;

            var name = GetType().Name;
            var adjusted = name.Replace("ExecuteCommand", "");

            if (UseTripleDotSuffix)
                adjusted += "...";

            return UsefulStuff.PascalCaseStringToHumanReadable(adjusted);
        
        }

        public virtual string GetCommandHelp()
        {
            return String.Empty;
        }

        /// <summary>
        /// disables the command because of the given reason.  This will result in grayed out menu items, crashes when executed programatically etc.
        /// </summary>
        /// <param name="reason"></param>
        protected void SetImpossible(string reason)
        {
            IsImpossible = true;
            ReasonCommandImpossible = reason;
        }
        /// <summary>
        /// disables the command because of the given reason.  This will result in grayed out menu items, crashes when executed programatically etc.
        /// This overload calls string.Format with the <paramref name="objects"/>
        /// </summary>
        /// /// <param name="reason"></param>
        /// <param name="objects">Objects to pass to string.Format</param>
        protected void SetImpossible(string reason, params object[] objects)
        {
            IsImpossible = true;
            ReasonCommandImpossible = string.Format(reason,objects);
        }

        /// <summary>
        /// Resets the IsImpossible status of the command
        /// </summary>
        protected void ResetImpossibleness()
        {
            IsImpossible = false;
            ReasonCommandImpossible = null;
        }
    }
}
