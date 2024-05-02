// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates new parameters for <see cref="ExtractionFilterParameterSet" /> when they are
///     in the parent filter but missing in the value set.
/// </summary>
public class ExecuteCommandAddMissingParameters : BasicCommandExecution
{
    private readonly ExtractionFilterParameterSet[] _sets;

    public ExecuteCommandAddMissingParameters(IBasicActivateItems activator, ExtractionFilterParameterSet set) : this(
        activator, new[] { set })
    {
    }

    [UseWithObjectConstructor]
    public ExecuteCommandAddMissingParameters(IBasicActivateItems activator, ExtractionFilterParameterSet[] sets) :
        base(activator)
    {
        _sets = sets;

        // if nobody is missing any entries
        if (!_sets.Any(static s => s.GetMissingEntries().Any())) SetImpossible("There are no missing parameters");
    }

    public override void Execute()
    {
        base.Execute();

        if (_sets.Length == 0) return;

        foreach (var set in _sets) set.CreateNewValueEntries();

        Publish(_sets.First());
    }
}