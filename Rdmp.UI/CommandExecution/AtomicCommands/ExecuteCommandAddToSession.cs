// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandAddToSession : BasicUICommandExecution
{
    private IMapsDirectlyToDatabaseTable[] _toAdd;
    private readonly SessionCollectionUI session;

    public ExecuteCommandAddToSession(IActivateItems activator, IMapsDirectlyToDatabaseTable[] toAdd,
        SessionCollectionUI session) : base(activator)
    {
        _toAdd = toAdd;
        this.session = session;

        if (session == null && !activator.GetSessions().Any())
            SetImpossible("There are no active Sessions");

        Weight = 100.2f;
    }

    public override void Execute()
    {
        base.Execute();
        var ses = session;

        if (ses == null)
        {
            var sessions = Activator.GetSessions().ToArray();

            if (sessions.Length == 1)
            {
                ses = sessions[0];
            }
            else
            {
                if (BasicActivator.SelectObject(new DialogArgs
                {
                    TaskDescription = "Choose which session to add the objects to"
                }, sessions, out var selected))
                    ses = selected;
            }
        }

        ses?.Add(_toAdd);
    }
}