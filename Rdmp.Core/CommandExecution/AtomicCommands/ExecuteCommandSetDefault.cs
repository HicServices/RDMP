// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Changes the default server for a given role (e.g. Logging) to a new server (which must
///     already exist and have the correct schema).  Use <see cref="ExecuteCommandCreateNewExternalDatabaseServer" />
///     if you want to create a new server from scratch.
/// </summary>
public class ExecuteCommandSetDefault : BasicCommandExecution
{
    private readonly PermissableDefaults _toSet;
    private readonly ExternalDatabaseServer _server;

    public ExecuteCommandSetDefault(IBasicActivateItems basicActivator, PermissableDefaults toSet,
        ExternalDatabaseServer server) : base(basicActivator)
    {
        _toSet = toSet;
        _server = server;
    }

    public override void Execute()
    {
        base.Execute();

        BasicActivator.ServerDefaults.SetDefault(_toSet, _server);
    }
}