namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Stores the location and credentials to access a database.  There can be multiple credentials available for use with a given IDataAccessPoint depending
    /// on the usage context e.g. DataAccessContext.DataLoad might know credentials for a user account with write permission while it's Credentials for use
    /// under DataAccessContext.DataExport are for a readonly user account.
    /// 
    /// You can translate an IDataAccessPoint into a connection string / DiscoveredServer by using DataAccessPortal.GetInstance().ExpectDatabase(...)
    /// 
    /// IDataAccessCredentials can include Encrypted passwords which the current user may or may not have access to decrypt.  Where no credentials are
    /// available it is assumed that the connection should be made using Integrated Security (Windows Security).
    /// </summary>
    public interface IDataAccessPoint:IHasQuerySyntaxHelper
    {
        string Server { get; }
        string Database { get; }
        DatabaseType DatabaseType { get; }
        
        IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context);
    }
}