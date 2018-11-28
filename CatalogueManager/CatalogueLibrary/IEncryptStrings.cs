namespace CatalogueLibrary
{
    /// <summary>
    /// Interface for classes which can encrypt/decrypt strings.
    /// </summary>
    public interface IEncryptStrings
    {
        /// <summary>
        /// Returns an encrypted representation of <paramref name="toEncrypt"/>
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        string Encrypt(string toEncrypt);

        /// <summary>
        /// Decrypts the provided encrypted string <paramref name="toDecrypt"/> into a clear text string
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        string Decrypt(string toDecrypt);

        /// <summary>
        /// Returns true if the provided string <paramref name="value"/> looks like it is encrypted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsStringEncrypted(string value);
    }
}