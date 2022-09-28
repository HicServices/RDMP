// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.Providers.Nodes
{
    public class TableInfoDatabaseNode : Node
    {
        public readonly DatabaseType DatabaseType;
        public string DatabaseName { get; private set; }
        public TableInfo[] Tables { get; }

        public const string NullDatabaseNode = "Null Database";

        public TableInfoDatabaseNode(string databaseName, DatabaseType databaseType, IEnumerable<TableInfo> tables)
        {
            DatabaseType = databaseType;
            Tables = tables.ToArray();
            DatabaseName = databaseName ?? NullDatabaseNode;
        }

        public override string ToString()
        {
            return DatabaseName;
        }

        public override bool Equals(object obj)
        {
            return obj is TableInfoDatabaseNode node &&
                   DatabaseType == node.DatabaseType &&
                   DatabaseName == node.DatabaseName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DatabaseType, DatabaseName);
        }
    }
}
