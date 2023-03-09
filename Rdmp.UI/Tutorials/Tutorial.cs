// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.TransparentHelpSystem;

namespace Rdmp.UI.Tutorials
{
    /// <summary>
    /// Wrapper for a <see cref="ICommandExecution"/> which should launch a user interaction that guides them through some activity
    /// (e.g. a <see cref="HelpWorkflow"/>).  Each <see cref="Tutorial"/> is associated with a specific <see cref="Guid"/> to ensure
    /// its completeness can can be tracked.
    ///
    /// <para>Instances should only be constructed in <see cref="TutorialTracker"/></para>
    /// </summary>
    public class Tutorial
    {
        public readonly ICommandExecution CommandExecution;

        public string Name { get; set; }
        public Guid Guid { get; set; }
        public Type CommandType { get; private set; }

        public bool UserHasSeen
        {
            get { return UserSettings.GetTutorialDone(Guid); }
            set {  UserSettings.SetTutorialDone(Guid,value); }
        }

        public Tutorial(string name, ICommandExecution commandExecutionExecution, Guid guid)
        {
            CommandExecution = commandExecutionExecution;
            Name = name;
            Guid = guid;
            CommandType = commandExecutionExecution.GetType();
        }

    }
}