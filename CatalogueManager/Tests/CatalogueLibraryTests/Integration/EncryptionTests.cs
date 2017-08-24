using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class EncryptionTests : DatabaseTests
    {
        [Test]
        public void EncryptAndThenDecryptString()
        {
            
            SimpleStringValueEncryption encrypter = new SimpleStringValueEncryption(CatalogueRepository);

            string toEncrypt = "Amagad";
            string encrytpedBinaryString = encrypter.Encrypt(toEncrypt);

            Console.WriteLine("Encrypted password was:" +encrytpedBinaryString);
            Assert.AreNotEqual(toEncrypt, encrytpedBinaryString);
            Assert.AreEqual(toEncrypt,encrypter.Decrypt(encrytpedBinaryString));
        }

        [Test]
        public void CheckIfThingsAreEncryptedOrNot()
        {

            SimpleStringValueEncryption encrypter = new SimpleStringValueEncryption(CatalogueRepository);

            string toEncrypt = "Amagad";
            string encrytpedBinaryString = encrypter.Encrypt(toEncrypt);

            Console.WriteLine("Encrypted password was:" + encrytpedBinaryString);
            
            Assert.True(encrypter.IsStringEncrypted(encrytpedBinaryString));
            Assert.False(encrypter.IsStringEncrypted(toEncrypt));
        }


        [Test]
        public void MultiEncryptingShouldntBreakIt()
        {
            //cleanup
            foreach (var c in CatalogueRepository.GetAllObjects<DataAccessCredentials>().Where(c => c.Name.Equals("frankieFran")))
                c.DeleteInDatabase();

            DataAccessCredentials creds = new DataAccessCredentials(CatalogueRepository, "frankieFran");
            try
            {
                //as soon as you set a password it should be encrypted by the credentials class in memory
                creds.Password = "fish";
                
                Assert.AreNotEqual("fish", creds.Password);
                Assert.AreEqual("fish", creds.GetDecryptedPassword()); //but we should still be able to decrypt it
                
                //set the password to the encrypted password
                creds.Password = creds.Password;

                //should still work
                Assert.AreNotEqual("fish", creds.Password);
                Assert.AreEqual("fish", creds.GetDecryptedPassword()); //but we should still be able to decrypt it
            }
            finally
            {
                creds.DeleteInDatabase();
            }
        }


        [Test]
        public void DataAccessCredentialsEncryption()
        {
            //cleanup
            foreach (var c in CatalogueRepository.GetAllObjects<DataAccessCredentials>().Where(c => c.Name.Equals("frankieFran")))
                c.DeleteInDatabase();

            DataAccessCredentials creds = new DataAccessCredentials(CatalogueRepository, "frankieFran");
            try
            {
                //as soon as you set a password it should be encrypted by the credentials class in memory
                creds.Password = "fish";
                Assert.AreNotEqual("fish",creds.Password);
                Assert.AreEqual("fish", creds.GetDecryptedPassword());//but we should still be able to decrypt it
            
                //save it
                creds.SaveToDatabase();
                using (var con = CatalogueRepository.GetConnection())
                {
                    var cmd = DatabaseCommandHelper.GetCommand("Select Password from DataAccessCredentials where Name='frankieFran'", con.Connection, con.Transaction);
                    string value = (string) cmd.ExecuteScalar();

                    //ensure password in database is encrypted
                    Assert.AreNotEqual("fish",value);
                    Assert.AreEqual(creds.Password,value);//does value in database match value in memory (encrypted)
                }

                //get a new copy out of the database
                DataAccessCredentials newCopy = CatalogueRepository.GetObjectByID<DataAccessCredentials>(creds.ID);
                Assert.AreEqual(creds.Password,newCopy.Password);//passwords should match
                Assert.AreNotEqual("fish",creds.Password);//neither should be fish
                Assert.AreNotEqual("fish", newCopy.Password);
            
                //both should decrypt to the same value (fish
                Assert.AreEqual("fish",creds.GetDecryptedPassword());
                Assert.AreEqual("fish", newCopy.GetDecryptedPassword());

            }
            finally
            {
                creds.DeleteInDatabase();
            }
        }

        [Test]
        [TestCase("bob")]
        [TestCase("  bob  ")]
        [TestCase("  b@!#*$(!#W$999sdf0------------ob  ")]
        public void TestFreakyPasswordValues(string freakyPassword)
        {
            //cleanup
            foreach (var c in CatalogueRepository.GetAllObjects<DataAccessCredentials>().Where(c => c.Name.Equals("frankieFran")))
                c.DeleteInDatabase();

            DataAccessCredentials creds = new DataAccessCredentials(CatalogueRepository, "frankieFran");
            try
            {
                //as soon as you set a password it should be encrypted by the credentials class in memory
                creds.Password = freakyPassword;
                Assert.AreNotEqual(freakyPassword, creds.Password);
                Assert.AreEqual(freakyPassword, creds.GetDecryptedPassword());//but we should still be able to decrypt it

                //save it
                creds.SaveToDatabase();
                using (var con = CatalogueRepository.GetConnection())
                {
                    var cmd = DatabaseCommandHelper.GetCommand("Select Password from DataAccessCredentials where Name='frankieFran'", con.Connection, con.Transaction);
                    string value = (string)cmd.ExecuteScalar();

                    //ensure password in database is encrypted
                    Assert.AreNotEqual(freakyPassword, value);
                    Assert.AreEqual(creds.Password, value);//does value in database match value in memory (encrypted)
                }

                //get a new copy out of the database
                DataAccessCredentials newCopy = CatalogueRepository.GetObjectByID<DataAccessCredentials>(creds.ID);
                Assert.AreEqual(creds.Password, newCopy.Password);//passwords should match
                Assert.AreNotEqual(freakyPassword, creds.Password);//neither should be fish
                Assert.AreNotEqual(freakyPassword, newCopy.Password);

                //both should decrypt to the same value (fish
                Assert.AreEqual(freakyPassword, creds.GetDecryptedPassword());
                Assert.AreEqual(freakyPassword, newCopy.GetDecryptedPassword());

            }
            finally
            {
                creds.DeleteInDatabase();
            }
        }


        [Test]
        public void MigrationOfOldPasswordsTest()
        {
            //cleanup
            foreach (var c in CatalogueRepository.GetAllObjects<DataAccessCredentials>().Where(c => c.Name.Equals("frankieFran")))
                c.DeleteInDatabase();

            //create a new credentials
            DataAccessCredentials creds = new DataAccessCredentials(CatalogueRepository, "frankieFran");
            try
            {
                //update the database to an unencrypted password (like would be the case before software patch)
                using (var con = CatalogueRepository.GetConnection())
                {
                    var cmd = DatabaseCommandHelper.GetCommand("UPDATE DataAccessCredentials set Password = 'fish' where Name='frankieFran'", con.Connection, con.Transaction);
                    Assert.AreEqual(1, cmd.ExecuteNonQuery());
                }

                DataAccessCredentials newCopy = CatalogueRepository.GetObjectByID<DataAccessCredentials>(creds.ID);
                
                Assert.AreEqual("fish",newCopy.GetDecryptedPassword());
                Assert.AreNotEqual("fish", newCopy.Password);
            }
            finally
            {
                creds.DeleteInDatabase();
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "The free text Value supplied to this class was too long to be encrypted (Length of string was 38)")]
        public void PasswordTooLong()
        {
            string password = "a";
            for(int i = 0;i<200;i++)
            {
                password += "a";
                TestFreakyPasswordValues(password);
            }
        }
        
    }
}
