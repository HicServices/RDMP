namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Encrypted Password string.  It is expected that GetDecryptedPassword method will throw Exception if the current user doesn't have access
    /// to the resources required to decrypt the Password (e.g. access to an RSA private key)
    /// </summary>
    public interface IEncryptedPasswordHost
    {
        /// <summary>
        /// The encrypted password.  This property should never return a clear text password.  Use GetDecryptedPassword to get the decrypted string.
        /// </summary>
        string Password { get; set; }
        
        /// <summary>
        /// Decrypts the encrypted Password property.  This method will throw an Exception if the user doesn't have access to the resources required
        /// to decrypt the Password (e.g. access to an RSA private key).
        /// </summary>
        /// <returns></returns>
        string GetDecryptedPassword();
    }
}