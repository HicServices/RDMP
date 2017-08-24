namespace ReusableLibraryCode.DataAccess
{
    public interface IDataAccessCredentials : IEncryptedPasswordHost
    {
        string Username { get;}
    }
}