using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all encrypted <see cref="DataAccessCredentials"/> used to access tables in your database.  This will be empty if you 
    /// connect with integrated security / windows authentication. 
    /// </summary>
    public class AllDataAccessCredentialsNode:SingletonNode
    {
        public AllDataAccessCredentialsNode(): base("Data Access Credentials")
        {
            
        }
    }
}
