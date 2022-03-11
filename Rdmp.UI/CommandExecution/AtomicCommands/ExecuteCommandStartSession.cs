// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Starts a new scoped session for one or more objects in the GUI
    /// </summary>
    public class ExecuteCommandStartSession : BasicUICommandExecution, IAtomicCommand
    {
        private readonly string _sessionName;
        private IMapsDirectlyToDatabaseTable[] _initialSelection;

        public ExecuteCommandStartSession(IActivateItems activator, IMapsDirectlyToDatabaseTable[] initialSelection, string sessionName) : base(activator)
        {
            _initialSelection = initialSelection;
            _sessionName = sessionName;
        }

        public override void Execute()
        {
            base.Execute();

            var name = _sessionName;

            if (string.IsNullOrWhiteSpace(name))
            {
                if (!Activator.TypeText("Session Name", "Name", 100, "Session 0", out string sessionName, false))
                    return;

                name = sessionName;
            }

            Activator.StartSession(name, _initialSelection);
        }
    }
}