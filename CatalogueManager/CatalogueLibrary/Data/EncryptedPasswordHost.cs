using CatalogueLibrary.Repositories;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    public class EncryptedPasswordHost : IEncryptedPasswordHost
    {
        /// <summary>
        /// This is only to support XML de-serialization
        /// </summary>
        internal class FakeEncryptedString : IEncryptedString
        {
            public string Value { get; set; }
            public string GetDecryptedValue()
            {
                throw new System.NotImplementedException();
            }

            public bool IsStringEncrypted(string value)
            {
                throw new System.NotImplementedException();
            }
        }

        private readonly IEncryptedString _encryptedString;

        /// <summary>
        /// For XML serialization
        /// </summary>
        protected EncryptedPasswordHost()
        {
            // This is to get around the issue where during de-serialization we cannot create an EncryptedString because there is no access to a repository.
            // If there is not a valid _encryptedString then de-serialization will fail (_encryptedString.Value is needed).
            // This provides an implementation of IEncryptedString which is only valid for deserializing the encrypted password from an XML representation and providing the encrypted password to a 'real' EncryptedPasswordHost
            _encryptedString = new FakeEncryptedString();
        }

        public EncryptedPasswordHost(ICatalogueRepository repository)
        {
            _encryptedString = new EncryptedString(repository);
        }

        public string Password
        {
            get
            {
                return _encryptedString.Value;
            }
            set
            {
                _encryptedString.Value = value;
            }
        }

        public string GetDecryptedPassword()
        {
            return _encryptedString.GetDecryptedValue();
        }
    }
}