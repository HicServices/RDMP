namespace ReusableLibraryCode.DataAccess
{
    public interface IDataAccessPoint:IHasQuerySyntaxHelper
    {
        string Server { get; }
        string Database { get; }
        DatabaseType DatabaseType { get; }
        
        IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context);
    }
}