using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all <see cref="ExternalDatabaseServer"/> objects.  These are servers that RDMP knows about and can connect to.  These are
    /// distinct from the server attributes of <see cref="TableInfo"/> and may not even be database servers (e.g. they could be FTP server or 
    /// a ticketing server etc).
    /// </summary>
    public class AllExternalServersNode : SingletonNode
    {
        public AllExternalServersNode() : base("External Servers (Including Platform Databases)")
        {

        }
    }
}