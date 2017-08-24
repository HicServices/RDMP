using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Reflection;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Records information about an SQL database.  This can be a system specific database e.g. a Logging database or an ANOStore or it could be a generic
    /// database you use to hold data (e.g. lookups). 
    /// 
    /// IMPORTANT: do not add an ExternalDatabaseServer just because you store data on it, instead you should import pointers to the data you hold as TableInfo 
    /// objects which themselves store Server/Database which allows for minimal disruption when you decide to move a table to a different server (it also allows
    /// for accessing the data under different accounts based on what is being done - loading vs extraction : see DataAccessCredentials_TableInfo).
    /// 
    /// ExternalDatabaseServer are really only for fixed global entities such as logging/identifier dumps etc.
    /// 
    /// Servers can but do not have to have usernames/passwords in which case integrated security (windows account) is used when openning connections.  Password
    /// is encrypted in the same fashion as in the DataAccessCredentials table.
    /// </summary>
    public class ExternalDatabaseServer : VersionedDatabaseEntity, IExternalDatabaseServer, IDataAccessCredentials, INamed
    {
        #region Database Properties

        private string _name;
        private string _createdByAssembly;
        private readonly SelfCertifyingDataAccessPoint _selfCertifyingDataAccessPoint;

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string CreatedByAssembly
        {
            get { return _createdByAssembly; }
            set { SetField(ref  _createdByAssembly, value); }
        }


        public string Server
        {
            get { return _selfCertifyingDataAccessPoint.Server; }
            set
            {
                _selfCertifyingDataAccessPoint.Server = value;
                OnPropertyChanged();
            }
        }

        public string Database
        {
            get { return _selfCertifyingDataAccessPoint.Database; }
            set
            {
                _selfCertifyingDataAccessPoint.Database = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return _selfCertifyingDataAccessPoint.Username; }
            set
            {
                _selfCertifyingDataAccessPoint.Username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _selfCertifyingDataAccessPoint.Password; }
            set
            {
                _selfCertifyingDataAccessPoint.Password = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public static int Server_MaxLength = -1;
        public static int Database_MaxLength = -1;
        public static int Username_MaxLength = -1;
        public static int Password_MaxLength = -1;
        public static int Name_MaxLength = -1;

        public ExternalDatabaseServer(ICatalogueRepository repository, string name, Assembly databaseAssemblyIfCreatedByOne = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"Name", name}
            };


            if(databaseAssemblyIfCreatedByOne != null)
                parameters.Add("CreatedByAssembly", databaseAssemblyIfCreatedByOne.GetName().Name);

            Repository = repository;
            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint((CatalogueRepository) repository,DatabaseType.MicrosoftSQLServer);
            repository.InsertAndHydrate(this, parameters);
        }

        public ExternalDatabaseServer(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            Name = r["Name"] as string;
            CreatedByAssembly = r["CreatedByAssembly"] as string;

            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint((CatalogueRepository)repository,DatabaseType.MicrosoftSQLServer)
            {
                Database = r["Database"] as string,
                Password = r["Password"] as string,
                Server = r["Server"] as string,
                Username = r["Username"] as string
            };
        }
        
        public override string ToString()
        {
            return Name;
        }
        
        
        /// <summary>
        /// Returns true if this objects servername and databasename match the paramters, will return false if any of the
        /// parameters or properties on this object are null.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool IsSameDatabase(string server, string database)
        {
            if(!string.IsNullOrWhiteSpace(Server) && Server.Equals(server))
                if (!string.IsNullOrWhiteSpace(Database) && Database.Equals(database))
                    return true;

            return false;
        }

        public bool RespondsWithinTime(int timeoutInSeconds,DataAccessContext context, out Exception exception)
        {
            return DataAccessPortal.GetInstance().ExpectServer(this, context).RespondsWithinTime(timeoutInSeconds, out exception);
        }

        [NoMappingToDatabase]
        public DatabaseType DatabaseType { get { return _selfCertifyingDataAccessPoint.DatabaseType; } }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            return _selfCertifyingDataAccessPoint.GetCredentialsIfExists(context);
        }
        public string GetDecryptedPassword()
        {
            return _selfCertifyingDataAccessPoint.GetDecryptedPassword();
        }

    }
}
