// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;

namespace Rdmp.UI.ProjectUI;

internal class ExtractCommandStateMonitor
{
    Dictionary<IExtractCommand,ExtractCommandState> CommandStates = new();
    private Dictionary<IExtractCommand, Dictionary<object, ExtractCommandState>> CommandSubStates = new();

    private Dictionary<object, ExtractCommandState> GlobalsStates = new();

    public bool Contains(IExtractCommand cmd)
    {
        return CommandStates.ContainsKey(cmd);
    }

    public void Add(IExtractDatasetCommand cmd)
    {
        CommandStates.Add(cmd,cmd.State);
        CommandSubStates.Add(cmd,cmd.DatasetBundle.States);
    }

    public void SaveState(IExtractDatasetCommand cmd)
    {
        CommandStates[cmd] = cmd.State;

        var toUpdateSubstates = CommandSubStates[cmd];

        foreach (var (key, value) in cmd.DatasetBundle.States.ToArray()) toUpdateSubstates[key] = value;
    }
        
    public IEnumerable<object> GetAllChangedObjects(IExtractDatasetCommand cmd)
    {
        if (CommandStates[cmd] != cmd.State)
            yield return cmd;

        foreach (var (key, value) in cmd.DatasetBundle.States.ToArray())
            if (CommandSubStates[cmd][key] != value)
                yield return key;
    }
    public void SaveState(GlobalsBundle globals)
    {
        foreach (var (key, value) in globals.States)
        {
            GlobalsStates[key] = value;
        }
    }

    public IEnumerable<object> GetAllChangedObjects(GlobalsBundle globals)
    {
        foreach (var (key, value) in globals.States)
        {
            if (!GlobalsStates.ContainsKey(key))
            {
                GlobalsStates.Add(key, value);
                yield return key;//new objects also are returned as changed
            }
            else
                //State has changed since last save
            if (GlobalsStates[key] != value)
                yield return key;
        }
            
    }
}