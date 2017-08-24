using System;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    public class PasswordEncryptionKeyLocation
    {
        private readonly ICatalogueRepository _repository;

        public PasswordEncryptionKeyLocation(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        public string GetKeyFileLocation()
        {
            using (var con = _repository.GetConnection())
            {
                //Table can only ever have 1 record
                DbCommand cmd = DatabaseCommandHelper.GetCommand("SELECT Path from PasswordEncryptionKeyLocation", con.Connection,con.Transaction);
                return cmd.ExecuteScalar() as string;
            }
        }

        public RSAParameters OpenKeyFile()
        {

            string existingKey = GetKeyFileLocation();

            return DeserializeFromLocation(existingKey);
        }

        private RSAParameters DeserializeFromLocation(string keyLocation)
        {

            if (string.IsNullOrWhiteSpace(keyLocation))
                throw new NotSupportedException("There is no key file configured for this Catalogue database, use CreateNewKeyFile to create one in a shared network location");

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

        public FileInfo CreateNewKeyFile(string path)
        {
            string existingKey = GetKeyFileLocation();
            if(existingKey != null)
                throw new NotSupportedException("There is already a key file at location:" + existingKey);

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(4096);
            RSAParameters p = provider.ExportParameters(true);

            using(var stream = File.Create(path))
            {
                XmlSerializer SerializeXml = new XmlSerializer(typeof(RSAParameters));
                SerializeXml.Serialize(stream,p);
                stream.Flush();
                stream.Close();
            }

            var fileInfo = new FileInfo(path);

            if(!fileInfo.Exists)
                throw new Exception("Created file but somehow it didn't exist!?!");

            using(var con = _repository.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand("INSERT INTO PasswordEncryptionKeyLocation(Path,Lock) VALUES (@Path,'X')", con.Connection,con.Transaction);
                DatabaseCommandHelper.AddParameterWithValueToCommand("@Path",cmd,fileInfo.FullName);
                cmd.ExecuteNonQuery();
            }

            return fileInfo;
        }

        public void ChangeLocation(string newLocation)
        {
            if (!File.Exists(newLocation))
                throw new FileNotFoundException("Could not find key file at:" + newLocation);

            //confirms that it is accessible and deserializable
            DeserializeFromLocation(newLocation);

            using (var con = _repository.GetConnection())
            {
                //Table can only ever have 1 record
                DbCommand cmd = DatabaseCommandHelper.GetCommand(@"if exists (select 1 from PasswordEncryptionKeyLocation)
    UPDATE PasswordEncryptionKeyLocation SET Path = @Path
  else
  INSERT INTO PasswordEncryptionKeyLocation(Path,Lock) VALUES (@Path,'X')
  ", con.Connection, con.Transaction);
                DatabaseCommandHelper.AddParameterWithValueToCommand("@Path", cmd, newLocation);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteKey()
        {
            string existingKey = GetKeyFileLocation();

            if(existingKey == null)
                throw new NotSupportedException("Cannot delete key because there is no key file configured");

            using (var con = _repository.GetConnection())
            {
                //Table can only ever have 1 record
                DbCommand cmd = DatabaseCommandHelper.GetCommand("DELETE FROM PasswordEncryptionKeyLocation", con.Connection,con.Transaction);
                int affectedRows = cmd.ExecuteNonQuery();

                if(affectedRows != 1)
                    throw new Exception("Delete from PasswordEncryptionKeyLocation resulted in " + affectedRows + ", expected 1");
            }
        }
    }
}
