using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes
{
    public class TableInfoServerNode
    {
        public string ServerName { get; private set; }

        public TableInfoServerNode(string serverName)
        {
            ServerName = serverName;
        }

        public override string ToString()
        {
            return ServerName;
        }

        protected bool Equals(TableInfoServerNode other)
        {
            return string.Equals(ServerName, other.ServerName);
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
            return (ServerName != null ? ServerName.GetHashCode() : 0);
        }
    }
}
