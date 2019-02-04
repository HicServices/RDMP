using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;
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

        /// <inheritdoc/>
        [Unique]
        [NotNull]
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <inheritdoc/>
        public string Username
        {
            get { return _username; }
            set { SetField(ref  _username, value); }
        }
        
        /// <inheritdoc/>
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

        /// <summary>
        /// Records a new (initially blank) set of credentials that can be used to access a <see cref="TableInfo"/> or other object requiring authentication.
        /// <para>A single <see cref="DataAccessCredentials"/> can be shared by multiple tables</para>
        /// 
        /// <para>You can also use <see cref="DataAccessCredentialsFactory"/> for easier credentials creation</para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name"></param>
        public DataAccessCredentials(ICatalogueRepository repository, string name= null)
        {
            name = name ?? "New Credentials " + Guid.NewGuid();

            _encryptedPasswordHost = new EncryptedPasswordHost(repository);

            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name}
            });
        }

        internal DataAccessCredentials(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            _encryptedPasswordHost = new EncryptedPasswordHost(repository);
            
            Name = (string)r["Name"];
            Username = r["Username"].ToString();
            Password = r["Password"].ToString();
        }

        /// <inheritdoc/>
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
        
        /// <summary>
        /// Returns all the <see cref="TableInfo"/> that rely on the credentials to access the table(s).  This is split into the contexts under which the 
        /// credentials are used e.g. <see cref="DataAccessContext.DataLoad"/>
        /// </summary>
        /// <returns></returns>
        public Dictionary<DataAccessContext, List<TableInfo>> GetAllTableInfosThatUseThis()
        {
            return ((CatalogueRepository)Repository).TableInfoToCredentialsLinker.GetAllTablesUsingCredentials(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public string GetDecryptedPassword()
        {
            return _encryptedPasswordHost.GetDecryptedPassword();
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return GetAllTableInfosThatUseThis().SelectMany(kvp=>kvp.Value).Cast<IHasDependencies>().ToArray();
        }
    }
}
