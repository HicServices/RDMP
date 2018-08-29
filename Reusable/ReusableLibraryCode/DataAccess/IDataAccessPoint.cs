namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Stores the location and credentials to access a database.  There can be multiple credentials available for use with a given IDataAccessPoint depending
    /// on the usage context e.g. DataAccessContext.DataLoad might know credentials for a user account with write permission while it's Credentials for use
    /// under DataAccessContext.DataExport are for a readonly user account.
    /// 
    /// <para>You can translate an IDataAccessPoint into a connection string / DiscoveredServer by using DataAccessPortal.GetInstance().ExpectDatabase(...)</para>
    /// 
    /// <para>IDataAccessCredentials can include Encrypted passwords which the current user may or may not have access to decrypt.  Where no credentials are
    /// available it is assumed that the connection should be made using Integrated Security (Windows Security).</para>
    /// </summary>
    public interface IDataAccessPoint:IHasQuerySyntaxHelper
    {
        /// <summary>
        /// The name of the server e.g. localhost\sqlexpress
        /// </summary>
        string Server { get; }

        /// <summary>
        /// The name of the database to connect to e.g. master, tempdb, MyCoolDb etc
        /// </summary>
        string Database { get; }

        /// <summary>
        /// The DBMS type of the server e.g. Sql Server / MySql / Oracle
        /// </summary>
        DatabaseType DatabaseType { get; }
        
        /// <summary>
        /// The username/password to use when connecting to the server (otherwise integrated security is used)
        /// </summary>
        /// <param name="context">What you intend to do after you have connected (may determine which credentials to use e.g. readonly vs readwrite)</param>
        /// <returns></returns>
        IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context);
    }
}
