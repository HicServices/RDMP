// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using FAnsi.Discovery;

namespace Rdmp.Core.DataLoad.Triggers.Implementations;

/// <summary>
///     Handles the creation of the appropriate <see cref="ITriggerImplementer" /> for any given
///     <see cref="DatabaseType" />
/// </summary>
public class TriggerImplementerFactory
{
    private readonly DatabaseType _databaseType;

    public TriggerImplementerFactory(DatabaseType databaseType)
    {
        _databaseType = databaseType;
    }

    public ITriggerImplementer Create(DiscoveredTable table, bool createDataLoadRunIDAlso = true)
    {
        return _databaseType switch
        {
            DatabaseType.MicrosoftSQLServer => new MicrosoftSQLTriggerImplementer(table, createDataLoadRunIDAlso),
            DatabaseType.MySql => new MySqlTriggerImplementer(table, createDataLoadRunIDAlso),
            DatabaseType.Oracle => new OracleTriggerImplementer(table, createDataLoadRunIDAlso),
            DatabaseType.PostgreSql => new PostgreSqlTriggerImplementer(table, createDataLoadRunIDAlso),
            _ => throw new ArgumentOutOfRangeException(nameof(_databaseType))
        };
    }
}