namespace CatalogueLibrary
{
    /// <summary>
    /// Interface for classes which can encrypt/decrypt strings.
    /// </summary>
    public interface IEncryptStrings
    {
        string Encrypt(string toEncrypt);
        string Decrypt(string toDecrypt);
        bool IsStringEncrypted(string value);
    }
}