// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class EncryptionTests : DatabaseTests
    {
        [Test]
        public void EncryptAndThenDecryptString()
        {

            var encrypter = CatalogueRepository.EncryptionManager.GetEncrypter();

            string toEncrypt = "Amagad";
            string encrytpedBinaryString = encrypter.Encrypt(toEncrypt);

            Console.WriteLine("Encrypted password was:" +encrytpedBinaryString);
            Assert.AreNotEqual(toEncrypt, encrytpedBinaryString);
            Assert.AreEqual(toEncrypt,encrypter.Decrypt(encrytpedBinaryString));
        }

        [Test]
        public void CheckIfThingsAreEncryptedOrNot()
        {

            var encrypter = CatalogueRepository.EncryptionManager.GetEncrypter();

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
                using (var con = CatalogueTableRepository.GetConnection())
                {
                    string value;
                    using(var cmd = DatabaseCommandHelper.GetCommand("Select Password from DataAccessCredentials where Name='frankieFran'", con.Connection, con.Transaction))
                        value = (string) cmd.ExecuteScalar();

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
                using (var con = CatalogueTableRepository.GetConnection())
                {
                    string value;
                    using(var cmd = DatabaseCommandHelper.GetCommand("Select Password from DataAccessCredentials where Name='frankieFran'", con.Connection, con.Transaction))
                        value = (string) cmd.ExecuteScalar();

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
                using (var con = CatalogueTableRepository.GetConnection())
                {
                    using (var cmd = DatabaseCommandHelper.GetCommand(
                        "UPDATE DataAccessCredentials set Password = 'fish' where Name='frankieFran'", con.Connection,
                        con.Transaction))
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
        public void PasswordTooLong()
        {
            if(RepositoryLocator.CatalogueRepository.EncryptionManager is PasswordEncryptionKeyLocation em && !string.IsNullOrWhiteSpace(em.GetKeyFileLocation()))
                Assert.Inconclusive("Could not run test because there is already an encryption key set up.  Likely one that handles very long passwords");

            string password = "a";
            for (int i = 0; i < 200; i++)
                password += "a";

            var ex = Assert.Throws<InvalidOperationException>(() => TestFreakyPasswordValues(password));
            Assert.AreEqual("The free text Value supplied to this class was too long to be encrypted (Length of string was 201)", ex.Message);
        }
        
    }
}
