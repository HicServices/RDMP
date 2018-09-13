using System;
using System.Security.Cryptography;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Encrypts a string, providing access to both the encrypted and decrypted values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Value is too long to be encrypted</exception>
    /// <exception cref="CryptographicException" />
    public class EncryptedString : IEncryptedString
    {
        private readonly IEncryptStrings _encrypter;
        private string _value;

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }

        /// <inheritdoc/>
        public string Value
        {
            get
            {
                //if there is a password in memory it will be encrypted (probably) so return that, to decrypt call DecryptPassword
                return _value;
            }
            set
            {

                if (string.IsNullOrWhiteSpace(value))//if it is null
                    _value = null;
                else
                    if (!_encrypter.IsStringEncrypted(value))//it is not null, is it already encrypted?
                        try
                        {
                            _value = _encrypter.Encrypt(value);//not yet encrypted so encrypt it
                        }
                        catch (CryptographicException e)
                        {
                            if (e.Message.StartsWith("Bad Length"))
                                throw new InvalidOperationException("The free text Value supplied to this class was too long to be encrypted (Length of string was " + value.Length + ")", e);

                            //it's some other exception
                            throw;
                        }
                    else
                        _value = value;//it is encrypted already so just store in normally
            }
        }

        /// <summary>
        /// Creates a new encrypted string using <see cref="SimpleStringValueEncryption"/> or the provided <paramref name="encrypter"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="encrypter"></param>
        public EncryptedString(ICatalogueRepository repository, IEncryptStrings encrypter = null)
        {
            _encrypter = encrypter ?? new SimpleStringValueEncryption(repository);
        }

        /// <inheritdoc/>
        public string GetDecryptedValue()
        {
            if (string.IsNullOrWhiteSpace(Value))
                return null;

            if (_encrypter.IsStringEncrypted(Value))
                return _encrypter.Decrypt(Value);

            //its not decrypted... how did that happen
            throw new Exception("Found Value in memory that was not encrypted");
        }

        /// <inheritdoc/>
        public bool IsStringEncrypted(string value)
        {
            return _encrypter.IsStringEncrypted(value);
        }
    }
}