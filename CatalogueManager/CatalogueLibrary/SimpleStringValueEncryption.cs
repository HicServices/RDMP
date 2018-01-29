using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary
{
    /// <summary>
    /// Core RDMP implementation of RSA puublic/private key encryption.  In order to be secure you should create a private key (See PasswordEncryptionKeyLocationUI).  If
    /// no private key is configured then the default Key will be used (this is not secure and anyone with access to the RDMP source code could decrypt your strings - which
    ///  is open source!). Strings are encrypted based on the key file.  Note that because RSA is a good encryption technique you will get a different output (encrypted) string
    /// value for repeated calls to Encrypt even with the same input string.
    /// </summary>
    public class SimpleStringValueEncryption : IEncryptStrings
    {

        private Encoding encoding = Encoding.ASCII;
        private PasswordEncryptionKeyLocation _location;

        private const string Key =
            @"<?xml version=""1.0"" encoding=""utf-16""?>
<RSAParameters xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <Exponent>AQAB</Exponent>
    <Modulus>sMDeszVErUmbqOxQavw5OsWpL3frccEGtTJYM8G54Fw7NK6xFVUrq79nWB6px4/B</Modulus>
    <P>6kcXnTVJrVuD9j6qUm+F71jIL2H92lgN</P>
    <Q>wSRbrdj1qGBPBnYMO5dx11gvfNCKKdWF</Q>
    <DP>aKdxaQzQ6Nwkyu+bbk/baNwkMOZ5W/xR</DP>
    <DQ>B/B8rErM3l0HIpbbrd9t2JJRcWoJI+sZ</DQ>
    <InverseQ>NFv4Z26nbMpOkOcAnO3rktoMffza+3Ul</InverseQ>
    <D>Y8zC8dUF7gI9zeeAkKfReInauV6wpg4iVh7jaTDN5DAmKFURTAyv6Il6LEyr07JB</D>
</RSAParameters>";

        public SimpleStringValueEncryption(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        private bool _initialised;
        private RSAParameters _privateKey;
        private ICatalogueRepository _repository;

        public RSAParameters PrivateKey
        {
            get
            {
                if (_initialised)
                    return _privateKey;

                //if there isn't a key file
                if (string.IsNullOrWhiteSpace(_location.GetKeyFileLocation()))
                {
                    //use the memory one
                    string xml = Key;
                    XmlSerializer DeserializeXml = new XmlSerializer(typeof (RSAParameters));
                    _privateKey = (RSAParameters)DeserializeXml.Deserialize(new StringReader(xml));
                }
                else
                    _privateKey = _location.OpenKeyFile();

                _initialised = true;
                return _privateKey;
            }
        }

        /// <summary>
        /// Encrypts using its PublicKey then returns a the encrypted byte[] as a string by using BitConverter.ToString()
        /// </summary>
        /// <returns></returns>
        public string Encrypt(string toEncrypt)
        {
            InitializeKey();

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportParameters(PrivateKey);
               return BitConverter.ToString(RSA.Encrypt(encoding.GetBytes(toEncrypt), false));
            }
        }

        private void InitializeKey()
        {
            if(_location == null)
                _location = new PasswordEncryptionKeyLocation(_repository);
        }


        /// <summary>
        /// Takes an encrypted byte[] (in string format as produced by BitConverter.ToString() 
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string Decrypt(string toDecrypt)
        {
            InitializeKey();

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportParameters(PrivateKey);

                try
                {
                    return encoding.GetString(RSA.Decrypt(ByteConverterGetBytes(toDecrypt), false));
                }
                catch (CryptographicException e)
                {
                    throw new Exception("Could not decrypt an encrypted string, possibly you are trying to decrypt it after having changed the PrivateKey to a different one than at the time it was encrypted?",e);
                }
            }
        }

        private static byte[] ByteConverterGetBytes(string encodedstring)
        {
            String[] arr = encodedstring.Split('-');
            byte[] array = new byte[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                array[i] = Convert.ToByte(arr[i], 16);

            return array;
        }

        public bool IsStringEncrypted(string value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value))
                return false;

            return value.Count(c=>c== '-') >= 47;
        }
    }
}