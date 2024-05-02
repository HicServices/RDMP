// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Providers.Nodes;

public class TableInfoDatabaseNode : Node
{
    public TableInfoServerNode Server { get; }
    public string DatabaseName { get; }
    public TableInfo[] Tables { get; }

    public const string NullDatabaseNode = "Null Database";

    public TableInfoDatabaseNode(string databaseName, TableInfoServerNode server, IEnumerable<TableInfo> tables)
    {
        Server = server;
        Tables = tables.ToArray();
        DatabaseName = databaseName ?? NullDatabaseNode;
    }

    public override string ToString()
    {
        return DatabaseName;
    }

    protected bool Equals(TableInfoDatabaseNode other)
    {
        return Server.Equals(other.Server) &&
               string.Equals(DatabaseName, other.DatabaseName, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TableInfoDatabaseNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Server, DatabaseName);
    }
}