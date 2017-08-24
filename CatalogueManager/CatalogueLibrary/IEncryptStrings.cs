namespace CatalogueLibrary
{
    public interface IEncryptStrings
    {
        string Encrypt(string toEncrypt);
        string Decrypt(string toDecrypt);
        bool IsStringEncrypted(string value);
    }
}