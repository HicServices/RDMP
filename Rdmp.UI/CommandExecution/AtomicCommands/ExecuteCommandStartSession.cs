// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

/// <summary>
/// Starts a new scoped session for one or more objects in the GUI
/// </summary>
public class ExecuteCommandStartSession : BasicUICommandExecution
{
    public const string FindResultsTitle = "Find Results";
    private readonly string _sessionName;
    private readonly IMapsDirectlyToDatabaseTable[] _initialSelection;

    /// <summary>
    /// True if the command was cancelled before finishing <see cref="Execute"/>
    /// </summary>
    public bool Cancelled { get; set; } = true;

    public string InitialSearch { get; set; }

    public ExecuteCommandStartSession(IActivateItems activator, IMapsDirectlyToDatabaseTable[] initialSelection,
        string sessionName) : base(activator)
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
            if (!Activator.TypeText("Session Name", "Name", 100, "Session 0", out var sessionName, false))
                return;

            name = sessionName;
        }

        name = MakeNovel(name);

        Activator.StartSession(name, _initialSelection, InitialSearch);
        Cancelled = false;
    }

    private string MakeNovel(string name)
    {
        var novelName = name;
        var sessions = ((IActivateItems)BasicActivator).GetSessions().ToArray();

        var i = 1;

        while (sessions.Any(s => s.Collection.SessionName.Equals(novelName)))
        {
            i++;
            novelName = name + i;
        }

        return novelName;
    }
}