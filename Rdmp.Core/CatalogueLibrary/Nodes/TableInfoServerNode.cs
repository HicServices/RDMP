// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using Rdmp.Core.CatalogueLibrary.Data;

namespace Rdmp.Core.CatalogueLibrary.Nodes
{
    public class TableInfoServerNode
    {
        public readonly DatabaseType DatabaseType;
        public string ServerName { get; private set; }

        public const string NullServerNode = "Null Server";

        public TableInfoServerNode(string serverName, DatabaseType databaseType)
        {
            DatabaseType = databaseType;
            ServerName = serverName ?? NullServerNode;
        }

        public override string ToString()
        {
            return ServerName;
        }

        protected bool Equals(TableInfoServerNode other)
        {
            return DatabaseType == other.DatabaseType && string.Equals(ServerName, other.ServerName,StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableInfoServerNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {

                return ((int)DatabaseType * 397) ^ (ServerName != null ? StringComparer.CurrentCultureIgnoreCase.GetHashCode(ServerName) : 0);
            }
        }

        public bool IsSameServer(TableInfo tableInfo)
        {
            return
                ServerName.Equals(tableInfo.Server ?? NullServerNode,StringComparison.CurrentCultureIgnoreCase)
                &&
                DatabaseType == tableInfo.DatabaseType;

        }
    }
}
