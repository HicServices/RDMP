// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Curation;
using ReusableLibraryCode;

namespace Rdmp.Core.Repositories.Managers
{
    /// <summary>
    /// The file system location of the RSA private decryption key used to decrypt passwords stored in RDMP database.  There can only ever be one PasswordEncryptionKeyLocation
    /// and this is used by all SimpleStringValueEncryption.  This means that passwords can be securely held in the RDMP database so long as suitable windows account management
    /// takes place (only providing access to the key file location to users who should be able to use the passwords).
    /// 
    /// <para>See PasswordEncryptionKeyLocationUI for more information.</para>
    /// </summary>
    public class PasswordEncryptionKeyLocation : IEncryptionManager, IInjectKnown
    {
        private readonly ICatalogueRepository _catalogueRepository;

        public const string RDMP_KEY_LOCATION = "RDMP_KEY_LOCATION";

        /// <summary>
        /// Prepares to retrieve/create the key file for the given platform database
        /// </summary>
        /// <param name="catalogueRepository"></param>
        public PasswordEncryptionKeyLocation(ICatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;

            ClearAllInjections();
        }

        public IEncryptStrings GetEncrypter()
        {
            return new SimpleStringValueEncryption(OpenKeyFile());
        }


        Lazy<string> _knownKeyFileLocation;

        /// <summary>
        /// Gets the physical file path to the currently configured RSA private key for encrypting/decrypting passwords or null if no
        /// custom keyfile has been created yet.  The answer to this question is cached, call <see cref="ClearAllInjections"/> to reset
        /// the cache
        /// </summary>
        /// <returns></returns>
        public string GetKeyFileLocation()
        {
            return _knownKeyFileLocation.Value;
        }

        private string GetKeyFileLocationImpl()
        {
            // Prefer to get it from the environment variable
            var fromEnvVar = Environment.GetEnvironmentVariable(RDMP_KEY_LOCATION);

            if (fromEnvVar != null)
                return fromEnvVar;

            return _catalogueRepository.GetEncryptionKeyPath();
        }


        /// <summary>
        /// Connects to the private key location and returns the encryption/decryption parameters stored in it
        /// </summary>
        /// <returns></returns>
        public RSAParameters? OpenKeyFile()
        {
            string existingKey = GetKeyFileLocation();
            return DeserializeFromLocation(existingKey);
        }

        private RSAParameters? DeserializeFromLocation(string keyLocation)
        {
            if (string.IsNullOrWhiteSpace(keyLocation))
                return null;

            string xml;

            try
            {
                xml = File.ReadAllText(keyLocation);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to open and read key file " + keyLocation + " (possibly it is not in a shared network location or the current user - " + Environment.UserName + ", does not have access to the file?)", ex);
            }


            try
            {
                XmlSerializer DeserializeXml = new XmlSerializer(typeof(RSAParameters));
                return (RSAParameters)DeserializeXml.Deserialize(new StringReader(xml));
            }
            catch (Exception e)
            {
                throw new Exception("Failed to deserialize key file " + keyLocation + ", possibly it is corrupt or has been modified manually to break it?", e);
            }
        }

        /// <summary>
        /// Creates a new private RSA encryption key certificate at the given location and sets the catalogue repository to use it for encrypting passwords.
        /// This will make any existing serialized passwords iretrievable unless you restore and reset the original key file location. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileInfo CreateNewKeyFile(string path)
        {
            ClearAllInjections();

            string existingKey = GetKeyFileLocation();
            if (existingKey != null)
                throw new NotSupportedException("There is already a key file at location:" + existingKey);

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(4096);
            RSAParameters p = provider.ExportParameters(true);

            var fi = new FileInfo(path);

            if(fi.Directory != null && !fi.Directory.Exists)
                fi.Directory.Create();

            using (var stream = fi.Create())
            {
                XmlSerializer SerializeXml = new XmlSerializer(typeof(RSAParameters));
                SerializeXml.Serialize(stream, p);
                stream.Flush();
                stream.Close();
            }

            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
                throw new Exception("Created file but somehow it didn't exist!?!");

            _catalogueRepository.SetEncryptionKeyPath(fileInfo.FullName);

            ClearAllInjections();
            
            return fileInfo;
        }

        /// <summary>
        /// Changes the location of the RSA private key file to a physical location on disk (which must exist)
        /// </summary>
        /// <param name="newLocation"></param>
        public void ChangeLocation(string newLocation)
        {
            ClearAllInjections();

            if (!File.Exists(newLocation))
                throw new FileNotFoundException("Could not find key file at:" + newLocation);

            //confirms that it is accessible and deserializable
            DeserializeFromLocation(newLocation);

            _catalogueRepository.SetEncryptionKeyPath(newLocation);

            ClearAllInjections();
        }

        /// <summary>
        /// Deletes the location of the RSA private key file from the platform database (does not delete the physical
        /// file).
        /// </summary>
        public void DeleteKey()
        {
            ClearAllInjections();

            string existingKey = GetKeyFileLocation();

            if (existingKey == null)
                throw new NotSupportedException("Cannot delete key because there is no key file configured");

            _catalogueRepository.DeleteEncryptionKeyPath();

            ClearAllInjections();
        }

        public void ClearAllInjections()
        {
            _knownKeyFileLocation = new Lazy<string>(GetKeyFileLocationImpl);
        }
    }
}