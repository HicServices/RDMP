namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A string which can be decrypted.  GetDecryptedValue might throw if the user doesn't have access  to the decryption algorithm / artifacts. Allows transmission of a 
    /// string without nessesarily having access to the decrypted value.
    /// </summary>
    public interface IEncryptedString
    {
        string Value { get; set; }
        
        string GetDecryptedValue();
        bool IsStringEncrypted(string value);
    }
}