// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Alter;

/// <summary>
///     Abstract base command for all commands which change a tables schema (on the live database)
/// </summary>
public abstract class AlterTableCommandExecution : BasicCommandExecution
{
    protected ITableInfo TableInfo;
    protected DiscoveredTable Table;

    protected AlterTableCommandExecution(IBasicActivateItems activator, ITableInfo tableInfo) : base(activator)
    {
        TableInfo = tableInfo;
        try
        {
            Table = TableInfo.Discover(DataAccessContext.InternalDataProcessing);
        }
        catch (Exception)
        {
            SetImpossible("Could not resolve Server/Table connection details");
            return;
        }


        if (!Table.Exists())
        {
            SetImpossible("Table does not exist");
            return;
        }

        if (Table.TableType != TableType.Table)
        {
            SetImpossible($"Table is a {Table.TableType}");
        }
    }

    protected bool Synchronize()
    {
        var sync = new TableInfoSynchronizer(TableInfo);
        return sync.Synchronize(new AcceptAllCheckNotifier());
    }
}