using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    public class PreLoadDiscardedColumnsCollection
    {
        public TableInfo TableInfo { get; private set; }
        public ExternalDatabaseServer IdentifierDumpServerIfAny { get; private set; }

        public PreLoadDiscardedColumnsCollection(TableInfo tableInfo, ExternalDatabaseServer identifierDumpServerIfAny)
        {
            TableInfo = tableInfo;
            IdentifierDumpServerIfAny = identifierDumpServerIfAny;
        }

        public override string ToString()
        {
            return "Discarded Columns" + (IdentifierDumpServerIfAny == null?"":" (" + IdentifierDumpServerIfAny.Name+")");
        }

        protected bool Equals(PreLoadDiscardedColumnsCollection other)
        {
            return Equals(TableInfo, other.TableInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PreLoadDiscardedColumnsCollection) obj);
        }

        public override int GetHashCode()
        {
            return (TableInfo != null ? TableInfo.GetHashCode() : 0);
        }
    }
}
