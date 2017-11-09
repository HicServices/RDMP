using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data.Remoting
{
    /// <summary>
    /// This represent a Remote Installation of RDMP which can accept connections at multiple endpoints
    /// 
    /// Endpoints are usual REST endpoints with a URL and a type which they can accept.
    /// The endpoint format is {Url}/api/{typename}
    /// The typename is used to create the URL for the endpoint.
    /// If you are sending a collection, append this to the URI: ?asarray=true
    /// </summary>
    public class RemoteRDMP : DatabaseEntity, INamed, IEncryptedPasswordHost
    {
        #region Database Properties

        private string _uRL;
        private string _name;
        private string _username;

        private EncryptedPasswordHost _encryptedPasswordHost;

        #endregion

        public string URL
        {
            get { return _uRL; }
            set { SetField(ref _uRL, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value); }
        }
        
        public string Password
        {
            get { return _encryptedPasswordHost.Password; }
            set
            {
                if (_encryptedPasswordHost.Password == value)
                    return;

                _encryptedPasswordHost.Password = value;
                OnPropertyChanged();
            }
        }

        public string GetDecryptedPassword()
        {
            return _encryptedPasswordHost.GetDecryptedPassword();
        }

        public RemoteRDMP(IRepository repository)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                { "Name", "Unnamed remote" },
                { "URL", "https://example.com" }
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }

        public RemoteRDMP(IRepository repository, DbDataReader r) : base(repository, r)
        {
            // need a new copy of the catalogue repository so a new DB connection can be made to use with the encrypted host.
            var catalogueRepository = new CatalogueRepository(((TableRepository)repository).ConnectionStringBuilder);
            _encryptedPasswordHost = new EncryptedPasswordHost(catalogueRepository);

            URL = r["URL"].ToString();
            Name = r["Name"].ToString();
            Username = r["Username"] as string;
            Password = r["Password"] as string;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}