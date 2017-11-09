using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{ 
    /// <summary>
    /// Stores a username and encrypted password the Password property of the entity will be a hex value formatted as string which can be decrypted at runtime via 
    /// the methods of base class EncryptedPasswordHost which currently uses SimpleStringValueEncryption which is a wrapper for RSACryptoServiceProvider.  The layout
    /// of this hierarchy however allows for future plugin utility e.g. using different encryption keys for different tables / user access rights etc. 
    /// </summary>
    public class DataAccessCredentials : DatabaseEntity, IDataAccessCredentials,INamed,IHasDependencies
    {
        private readonly EncryptedPasswordHost _encryptedPasswordHost;

        #region Database Properties
        private string _name;
        private string _username;

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetField(ref  _username, value); }
        }
        public string Password
        {
            get { return _encryptedPasswordHost.Password; }
            set
            {
                if (Equals(_encryptedPasswordHost.Password,value))
                    return;

                _encryptedPasswordHost.Password = value;
                OnPropertyChanged();
            }
        }
        
        #endregion

        public DataAccessCredentials(ICatalogueRepository repository, string name= null)
        {
            name = name ?? "New Credentials " + Guid.NewGuid();

            _encryptedPasswordHost = new EncryptedPasswordHost(repository);

            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name}
            });
        }

        public DataAccessCredentials(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            var catalogueRepository = new CatalogueRepository(((TableRepository)repository).ConnectionStringBuilder);
            _encryptedPasswordHost = new EncryptedPasswordHost(catalogueRepository);
            
            Name = (string)r["Name"];
            Username = r["Username"].ToString();
            Password = r["Password"].ToString();
        }

        public override void DeleteInDatabase()
        {
            try
            {
                base.DeleteInDatabase();
            }
            catch (Exception e)
            {
                if(e.Message.Contains("FK_DataAccessCredentials_TableInfo_DataAccessCredentials"))
                    throw new CredentialsInUseException("Cannot delete credentials " + Name + " because it is in use by one or more TableInfo objects(" + string.Join("",GetAllTableInfosThatUseThis().Values.Select(t=>string.Join(",",t)))+")",e);

                throw;
            }
        }
        
        public Dictionary<DataAccessContext, List<TableInfo>> GetAllTableInfosThatUseThis()
        {
            return ((CatalogueRepository)Repository).TableInfoToCredentialsLinker.GetAllTablesUsingCredentials(this);
        }

        
        public override string ToString()
        {
            return Name;
        }


        public string GetDecryptedPassword()
        {
            return _encryptedPasswordHost.GetDecryptedPassword();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return GetAllTableInfosThatUseThis().SelectMany(kvp=>kvp.Value).Cast<IHasDependencies>().ToArray();
        }
    }
}
