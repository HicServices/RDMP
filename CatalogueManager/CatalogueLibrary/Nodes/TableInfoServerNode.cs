using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using ReusableLibraryCode;

namespace CatalogueLibrary.Nodes
{
    public class TableInfoServerNode
    {
        public readonly DatabaseType DatabaseType;
        public string ServerName { get; private set; }

        public TableInfoServerNode(string serverName, DatabaseType databaseType)
        {
            DatabaseType = databaseType;
            ServerName = serverName;
        }

        public override string ToString()
        {
            return ServerName;
        }

        protected bool Equals(TableInfoServerNode other)
        {
            return DatabaseType == other.DatabaseType && string.Equals(ServerName, other.ServerName);
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
                return ((int) DatabaseType*397) ^ (ServerName != null ? ServerName.GetHashCode() : 0);
            }
        }

        public bool IsSameServer(TableInfo tableInfo)
        {
            return
                ServerName.Equals(tableInfo.Server)
                &&
                DatabaseType == tableInfo.DatabaseType;

        }
    }
}
