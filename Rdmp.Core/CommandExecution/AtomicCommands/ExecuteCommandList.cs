// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Lists all the objects in RDMP that match search term.
/// </summary>
[Alias("ls")]
public class ExecuteCommandList : BasicCommandExecution
{
    private readonly IMapsDirectlyToDatabaseTable[] _toList;

    [UseWithObjectConstructor]
    public ExecuteCommandList(IBasicActivateItems activator,
        [DemandsInitialization(
            "The objects you want listed e.g. Catalogue:*bob* or a type e.g. TableInfo or blank to list everything")]
        IMapsDirectlyToDatabaseTable[] toList = null) : base(activator)
    {
        _toList = toList;
    }

    public override void Execute()
    {
        base.Execute();

        if (_toList == null)
        {
            ListEveryone();
            return;
        }

        var sb = new StringBuilder();
        foreach (var m in _toList)
            sb.AppendLine($"{m.ID}:{m}");

        BasicActivator.Show(sb.ToString());
    }

    private void ListEveryone()
    {
        var sb = new StringBuilder();

        foreach (var repo in BasicActivator.RepositoryLocator.GetAllRepositories())
        foreach (var o in repo.GetAllObjectsInDatabase())
            sb.AppendLine($"{o.GetType().Name}:{o.ID}:{o}");

        BasicActivator.Show(sb.ToString());
    }
}