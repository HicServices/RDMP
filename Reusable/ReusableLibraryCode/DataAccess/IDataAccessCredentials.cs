namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Username and Encrypted Password for use connecting to something.
    /// </summary>
    public interface IDataAccessCredentials : IEncryptedPasswordHost
    {
        /// <summary>
        /// The user account name to supply when sending the credentials
        /// </summary>
        string Username { get;}
    }
}