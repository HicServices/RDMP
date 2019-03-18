namespace CatalogueLibrary.Repositories.Managers
{
    public interface IEncryptionManager
    {
        /// <summary>
        /// Enables encryption/decryption of strings using a custom RSA key stored in a secure location on disk
        /// </summary>
        IEncryptStrings GetEncrypter();
    }
}