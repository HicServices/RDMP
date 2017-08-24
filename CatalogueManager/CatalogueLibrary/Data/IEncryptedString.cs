namespace CatalogueLibrary.Data
{
    public interface IEncryptedString
    {
        string Value { get; set; }
        
        string GetDecryptedValue();
        bool IsStringEncrypted(string value);
    }
}