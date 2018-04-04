using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.UI.WebControls;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

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
                if (Equals(_selfCertifyingDataAccessPoint.Server, value))
                    return;

                _selfCertifyingDataAccessPoint.Server = value;
                OnPropertyChanged();
            }
        }

        public string Database
        {
            get { return _selfCertifyingDataAccessPoint.Database; }
            set
            {
                if (Equals(_selfCertifyingDataAccessPoint.Database,value))
                    return;

                _selfCertifyingDataAccessPoint.Database = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return _selfCertifyingDataAccessPoint.Username; }
            set
            {
                if (Equals(_selfCertifyingDataAccessPoint.Username, value))
                    return;

                _selfCertifyingDataAccessPoint.Username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _selfCertifyingDataAccessPoint.Password; }
            set
            {
                if (Equals(_selfCertifyingDataAccessPoint.Password, value))
                    return;

                _selfCertifyingDataAccessPoint.Password = value;
                OnPropertyChanged();
            }
        }

        #endregion

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Server_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Database_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Username_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Password_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
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

        internal ExternalDatabaseServer(ICatalogueRepository repository, DbDataReader r): base(repository, r)
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

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return _selfCertifyingDataAccessPoint.GetQuerySyntaxHelper();
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

        /// <summary>
        /// Sets server,database,username and password properties based on the supplied DiscoveredDatabase (which doesn't have to actually exist).  This method also optionally calls
        /// SaveToDatabase which commits the changes to the Catalogue Repository 
        /// </summary>
        /// <param name="discoveredDatabase"></param>
        /// <param name="save">true if you want to call SaveToDatabase after setting the properties</param>
        public void SetProperties(DiscoveredDatabase discoveredDatabase, bool save = true)
        {
            Server = discoveredDatabase.Server.Name;
            Database = discoveredDatabase.GetRuntimeName();
            Username = discoveredDatabase.Server.ExplicitUsernameIfAny;
            Password = discoveredDatabase.Server.ExplicitPasswordIfAny;

            if(save)
                SaveToDatabase();
        }

        public bool WasCreatedByDatabaseAssembly(Assembly databaseAssembly)
        {
            if (string.IsNullOrWhiteSpace(CreatedByAssembly))
                return false;

            return databaseAssembly.GetName().Name == CreatedByAssembly;
        }
    }
}
