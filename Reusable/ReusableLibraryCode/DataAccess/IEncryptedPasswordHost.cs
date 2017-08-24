namespace ReusableLibraryCode.DataAccess
{
    public interface IEncryptedPasswordHost
    {
        string Password { get; set; }
        string GetDecryptedPassword();
    }
}