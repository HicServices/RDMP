namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Username and Encrypted Password for use connecting to something.
    /// </summary>
    public interface IDataAccessCredentials : IEncryptedPasswordHost
    {
        string Username { get;}
    }
}