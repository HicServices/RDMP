// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Providers.Nodes;

public class TableInfoServerNode : Node
{
    public readonly DatabaseType DatabaseType;
    public string ServerName { get; }
    public TableInfo[] Tables { get; }

    public const string NullServerNode = "Null Server";

    public TableInfoServerNode(string serverName, DatabaseType databaseType, IEnumerable<TableInfo> tables)
    {
        DatabaseType = databaseType;
        ServerName = serverName ?? NullServerNode;
        Tables = tables.ToArray();
    }

    public override string ToString()
    {
        return ServerName;
    }

    protected bool Equals(TableInfoServerNode other)
    {
        return DatabaseType == other.DatabaseType &&
               string.Equals(ServerName, other.ServerName,
                   StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TableInfoServerNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DatabaseType, ServerName);
    }

    public bool IsSameServer(TableInfo tableInfo)
    {
        return ServerName.Equals(tableInfo.Server ?? NullServerNode, StringComparison.CurrentCultureIgnoreCase)
               &&
               DatabaseType == tableInfo.DatabaseType;
    }
}