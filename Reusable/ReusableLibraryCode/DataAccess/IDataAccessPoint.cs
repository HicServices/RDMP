namespace ReusableLibraryCode.DataAccess
{
    public interface IDataAccessPoint
    {
        string Server { get; }
        string Database { get; }
        DatabaseType DatabaseType { get; }
        
        IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context);
    }
}