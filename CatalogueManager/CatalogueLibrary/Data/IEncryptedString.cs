namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A string which can be decrypted.  GetDecryptedValue might throw if the user doesn't have access  to the decryption algorithm / artifacts. Allows transmission of a 
    /// string without nessesarily having access to the decrypted value.
    /// </summary>
    public interface IEncryptedString
    {
        /// <inheritdoc cref="ReusableLibraryCode.DataAccess.IEncryptedPasswordHost.Password"/>
        string Value { get; set; }

        /// <inheritdoc cref="ReusableLibraryCode.DataAccess.IEncryptedPasswordHost.GetDecryptedPassword"/>
        string GetDecryptedValue();

        /// <summary>
        /// Returns true if the <paramref name="value"/> looks like it is encrypted
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsStringEncrypted(string value);
    }
}