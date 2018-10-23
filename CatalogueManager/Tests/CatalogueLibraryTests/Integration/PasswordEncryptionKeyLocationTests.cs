using System;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class PasswordEncryptionKeyLocationTests:DatabaseTests
    {
        [SetUp]
        public void ClearAnyRemnantKeys()
        {
            var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);
            if(keyLocation.GetKeyFileLocation() != null)
                keyLocation.DeleteKey();
        }

        [Test]
        public void NoKeyFileToStartWith()
        {
            var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);

            //there shouldn't already be a key
            Assert.IsNull(keyLocation.GetKeyFileLocation());

            var e = Assert.Throws<NotSupportedException>(keyLocation.DeleteKey);
            Assert.AreEqual("Cannot delete key because there is no key file configured", e.Message);

        }

        [Test]
        public void CreateKeyFile()
        {
            var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);
            var file = keyLocation.CreateNewKeyFile(Path.Combine(TestContext.CurrentContext.WorkDirectory,"my.key"));

            Console.WriteLine("Key file location is:" + file.FullName);
            Console.WriteLine("Text put into file is:" + Environment.NewLine +  File.ReadAllText(file.FullName));

            Assert.IsTrue(file.FullName.EndsWith("my.key"));

            Assert.AreEqual(file.FullName, keyLocation.GetKeyFileLocation());
            keyLocation.DeleteKey();

            Assert.IsNull(keyLocation.GetKeyFileLocation());
        }

        [Test]
        public void Encrypt()
        {
            string value = "MyPieceOfText";

            Console.WriteLine("String is:" + value);

            EncryptedString encrypter = new EncryptedString(CatalogueRepository);
            Assert.IsFalse(encrypter.IsStringEncrypted(value));

            //should do pass through encryption
            encrypter.Value = value;
            Assert.AreNotEqual(value,encrypter.Value);
            Assert.AreEqual(value,encrypter.GetDecryptedValue());

            Console.WriteLine("Encrypted (stock) is:" + encrypter.Value);
            Console.WriteLine("Decrypted (stock) is:" + encrypter.GetDecryptedValue());

            var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);
            keyLocation.CreateNewKeyFile("my.key");
            var p = keyLocation.OpenKeyFile();

            SimpleStringValueEncryption s = new SimpleStringValueEncryption(CatalogueRepository);
            var exception = Assert.Throws<Exception>(()=>s.Decrypt(encrypter.Value));
            Assert.IsTrue(exception.Message.StartsWith("Could not decrypt an encrypted string, possibly you are trying to decrypt it after having changed the PrivateKey "));

            string encrypted = s.Encrypt(value);
            Console.WriteLine("Encrypted (with key) is:" + encrypted);
            Console.WriteLine("Decrypted (with key) is:" + s.Decrypt(encrypted));

            Assert.IsTrue(encrypter.IsStringEncrypted(encrypted));

            keyLocation.DeleteKey();
        }

    }
}
