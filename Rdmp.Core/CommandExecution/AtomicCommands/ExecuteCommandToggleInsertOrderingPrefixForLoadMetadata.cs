// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Toggles the LoadMetadata's ability to sort index items by pk before inserting
/// </summary>
public class ExecuteCommandToggleInsertOrderingPrefixForLoadMetadata : BasicCommandExecution
{
    private LoadMetadata _loadMetadata;
    public ExecuteCommandToggleInsertOrderingPrefixForLoadMetadata([DemandsInitialization("The LoadMetadata to update")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
    }

    public override void Execute()
    {
        base.Execute();
        _loadMetadata.OrderInsertsByPrimaryKey = !_loadMetadata.OrderInsertsByPrimaryKey;
        _loadMetadata.SaveToDatabase();
    }
}
