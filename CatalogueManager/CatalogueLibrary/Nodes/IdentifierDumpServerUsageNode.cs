using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes
{
    public class IdentifierDumpServerUsageNode:IDeleteable
    {
        public TableInfo TableInfo { get; private set; }
        public ExternalDatabaseServer IdentifierDumpServer { get; private set; }

        public IdentifierDumpServerUsageNode(TableInfo tableInfo, ExternalDatabaseServer identifierDumpServer)
        {
            TableInfo = tableInfo;
            IdentifierDumpServer = identifierDumpServer;
        }

        public override string ToString()
        {
            return "Usage of:" + IdentifierDumpServer.Name;
        }
        
        protected bool Equals(IdentifierDumpServerUsageNode other)
        {
            return Equals(TableInfo, other.TableInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifierDumpServerUsageNode) obj);
        }

        public override int GetHashCode()
        {
            return (TableInfo != null ? TableInfo.GetHashCode() : 0);
        }

        public void DeleteInDatabase()
        {
            TableInfo.IdentifierDumpServer_ID = null;
            TableInfo.SaveToDatabase();
        }
    }
}