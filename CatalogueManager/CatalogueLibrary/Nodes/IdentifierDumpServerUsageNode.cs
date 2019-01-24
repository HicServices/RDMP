using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Identifies a database which is used to 'split off' identifiable data (columns) during a data load instead of loading it into the LIVE database (from which you
    /// execute data extractions).
    /// </summary>
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