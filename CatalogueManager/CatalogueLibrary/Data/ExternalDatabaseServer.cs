using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.UI.WebControls;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
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
    /// <para>IMPORTANT: do not add an ExternalDatabaseServer just because you store data on it, instead you should import pointers to the data you hold as TableInfo 
    /// objects which themselves store Server/Database which allows for minimal disruption when you decide to move a table to a different server (it also allows
    /// for accessing the data under different accounts based on what is being done - loading vs extraction : see DataAccessCredentials_TableInfo).</para>
    /// 
    /// <para>ExternalDatabaseServer are really only for fixed global entities such as logging/identifier dumps etc.</para>
    /// 
    /// <para>Servers can but do not have to have usernames/passwords in which case integrated security (windows account) is used when openning connections.  Password
    /// is encrypted in the same fashion as in the DataAccessCredentials table.</para>
    /// </summary>
    public class ExternalDatabaseServer : VersionedDatabaseEntity, IExternalDatabaseServer, IDataAccessCredentials, INamed
    {
        #region Database Properties

        private string _name;
        private string _createdByAssembly;
        private string _mappedDataPath;
        private readonly SelfCertifyingDataAccessPoint _selfCertifyingDataAccessPoint;

        /// <summary>
        /// Human readable name for the server e.g. 'My Favourite Logging Database'
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <summary>
        /// If the database was created by an RDMP schema (or plugin schema) this will contain the name of the dll which holds the schema e.g. DataQualityEngine.Database and was
        /// responsible for creating the database.  This determines what roles RDMP lets the database play.
        /// </summary>
        public string CreatedByAssembly
        {
            get { return _createdByAssembly; }
            set { SetField(ref  _createdByAssembly, value); }
        }

        /// <summary>
        /// The public network share of the Data path where the physical database files are stored if applicable.  Sharing your database directory on the network is a 
        /// terrible idea (don't do it).  You can use this to automate detatching and shipping an MDF to your researchers e.g. MsSqlReleaseSource
        /// </summary>
        public string MappedDataPath
        {
            get { return _mappedDataPath; }
            set { SetField(ref _mappedDataPath, value); }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public DatabaseType DatabaseType
        {
            get { return _selfCertifyingDataAccessPoint.DatabaseType; }
            set
            {
                if (Equals(_selfCertifyingDataAccessPoint.DatabaseType, value))
                    return;

                _selfCertifyingDataAccessPoint.DatabaseType = value;
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

        /// <summary>
        /// Creates a new persistent server reference in RDMP platform database that allows it to connect to a (usually database) server.
        /// 
        /// <para>If you are trying to create a database (e.g. a logging database) you should instead use 
        /// <see cref="MapsDirectlyToDatabaseTable.Versioning.MasterDatabaseScriptExecutor"/></para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name"></param>
        /// <param name="databaseAssemblyIfCreatedByOne"></param>
        public ExternalDatabaseServer(ICatalogueRepository repository, string name, Assembly databaseAssemblyIfCreatedByOne = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"Name", name},
                {"DatabaseType",DatabaseType.MicrosoftSQLServer}
            };
            
            if(databaseAssemblyIfCreatedByOne != null)
                parameters.Add("CreatedByAssembly", databaseAssemblyIfCreatedByOne.GetName().Name);

            Repository = repository;
            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint((CatalogueRepository) repository,DatabaseType.MicrosoftSQLServer);
            repository.InsertAndHydrate(this, parameters);
        }

        internal ExternalDatabaseServer(ShareManager shareManager, ShareDefinition shareDefinition)
        {
            var repo = shareManager.RepositoryLocator.CatalogueRepository;
            Repository = repo;
            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint((CatalogueRepository)Repository, DatabaseType.MicrosoftSQLServer/*will get changed by UpsertAndHydrate*/); 

            repo.UpsertAndHydrate(this,shareManager, shareDefinition);
        }

        internal ExternalDatabaseServer(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            Name = r["Name"] as string;
            CreatedByAssembly = r["CreatedByAssembly"] as string;
            MappedDataPath = r["MappedDataPath"] as string;

            var databaseType = (DatabaseType) Enum.Parse(typeof (DatabaseType), r["DatabaseType"].ToString());

            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint((CatalogueRepository)repository, databaseType)
            {
                Database = r["Database"] as string,
                Password = r["Password"] as string,
                Server = r["Server"] as string,
                Username = r["Username"] as string
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return _selfCertifyingDataAccessPoint.GetQuerySyntaxHelper();
        }

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

        /// <inheritdoc/>
        public bool WasCreatedByDatabaseAssembly(Assembly databaseAssembly)
        {
            if (string.IsNullOrWhiteSpace(CreatedByAssembly))
                return false;

            return databaseAssembly.GetName().Name == CreatedByAssembly;
        }

        /// <inheritdoc/>
        public DiscoveredDatabase Discover(DataAccessContext context)
        {
            return DataAccessPortal.GetInstance().ExpectDatabase(this, context);
        }
    }
}
