using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Ticketing
{
    /// <summary>
    /// All implementations of ITicketingSystem will be given this parameter as a constructor argument.  It includes the RDMP configured credentials for
    /// the ticketing system.  Credentials.Password is encrypted, use GetDecryptedPassword() if you want to use the Credentials property (this method can
    /// fail if the user does not have access to the password decryption key (see PasswordEncryptionKeyLocationUI). 
    /// </summary>
    public class TicketingSystemConstructorParameters
    {
        public string Url { get; set; }
        public IDataAccessCredentials Credentials { get; set; }

        public TicketingSystemConstructorParameters(string url, IDataAccessCredentials credentials)
        {
            Url = url;
            Credentials = credentials;
        }
    }
}
