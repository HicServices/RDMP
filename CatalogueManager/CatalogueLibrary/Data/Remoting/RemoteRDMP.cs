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
        private string _password;

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
            get { return _password; }
            set { SetField(ref _password, value); }
        }

        public string GetDecryptedPassword()
        {
            return new EncryptedString((ICatalogueRepository) Repository) { Value = Password }.GetDecryptedValue();
        }

        public RemoteRDMP(IRepository repository /*, TODO Required Construction Properties For NEW*/)
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