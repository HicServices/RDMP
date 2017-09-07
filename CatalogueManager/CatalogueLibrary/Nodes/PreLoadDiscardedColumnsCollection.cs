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
    }
}
