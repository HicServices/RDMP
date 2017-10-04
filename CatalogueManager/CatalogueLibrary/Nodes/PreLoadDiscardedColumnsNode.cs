using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    public class PreLoadDiscardedColumnsNode
    {
        public TableInfo TableInfo { get; private set; }

        public PreLoadDiscardedColumnsNode(TableInfo tableInfo)
        {
            TableInfo = tableInfo;
        }

        public override string ToString()
        {
            return "Discarded Columns";
        }

        protected bool Equals(PreLoadDiscardedColumnsNode other)
        {
            return Equals(TableInfo, other.TableInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PreLoadDiscardedColumnsNode) obj);
        }

        public override int GetHashCode()
        {
            return (TableInfo != null ? TableInfo.GetHashCode() : 0);
        }

     
    }
}
