// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Annotations;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// <para>Records information about a server.  This can be a system specific database e.g. a Logging database or an ANOStore or it could be a generic
    /// database you use to hold data (e.g. lookups).  These are usually database servers but don't have to be (e.g. you could create a reference to an FTP server).</para>
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
    public class ExternalDatabaseServer : DatabaseEntity, IExternalDatabaseServer, IDataAccessCredentials, INamed, ICheckable
    {
        #region Database Properties

        private string _name;
        private string _createdByAssembly;
        private string _mappedDataPath;
        private readonly SelfCertifyingDataAccessPoint _selfCertifyingDataAccessPoint;

        /// <summary>
        /// Human readable name for the server e.g. 'My Favourite Logging Database'
        /// </summary>
        [Unique]
        [NotNull]
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

                var old = _selfCertifyingDataAccessPoint.Server;
                _selfCertifyingDataAccessPoint.Server = value;
                OnPropertyChanged(old,value);
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

                var old = _selfCertifyingDataAccessPoint.Database;
                _selfCertifyingDataAccessPoint.Database = value;
                OnPropertyChanged(old, value);
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

                var old = _selfCertifyingDataAccessPoint.Username;
                _selfCertifyingDataAccessPoint.Username = value;
                OnPropertyChanged(old, value);
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

                var old = _selfCertifyingDataAccessPoint.Password;
                _selfCertifyingDataAccessPoint.Password = value;
                OnPropertyChanged(old, value);
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
                
                var old = _selfCertifyingDataAccessPoint.DatabaseType;
                _selfCertifyingDataAccessPoint.DatabaseType = value;
                OnPropertyChanged(old, value);
            }
        }

        #endregion

        
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
            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(repository,DatabaseType.MicrosoftSQLServer);
            repository.InsertAndHydrate(this, parameters);
        }

        internal ExternalDatabaseServer(ShareManager shareManager, ShareDefinition shareDefinition)
        {
            var repo = shareManager.RepositoryLocator.CatalogueRepository;
            Repository = repo;
            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(CatalogueRepository, DatabaseType.MicrosoftSQLServer/*will get changed by UpsertAndHydrate*/); 

            shareManager.UpsertAndHydrate(this, shareDefinition);
        }

        internal ExternalDatabaseServer(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            Name = r["Name"] as string;
            CreatedByAssembly = r["CreatedByAssembly"] as string;
            MappedDataPath = r["MappedDataPath"] as string;

            var databaseType = (DatabaseType) Enum.Parse(typeof (DatabaseType), r["DatabaseType"].ToString());

            _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(repository, databaseType)
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

        public void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(Server))
                notifier.OnCheckPerformed(new CheckEventArgs("No Server set", CheckResult.Warning));
            else   
            if (string.IsNullOrWhiteSpace(Database))
                notifier.OnCheckPerformed(new CheckEventArgs("No Database set", CheckResult.Warning));
            else
            try
            {
                DataAccessPortal.GetInstance().ExpectServer(this, DataAccessContext.InternalDataProcessing).TestConnection();
                notifier.OnCheckPerformed(new CheckEventArgs("Successfully connected to server", CheckResult.Success));
            }
            catch (Exception exception)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to connect to server", CheckResult.Fail, exception));
                return;
            }

            //if it's a logging server run logging checks
            if (WasCreatedByDatabaseAssembly(typeof(HIC.Logging.Database.Class1).Assembly))
                new LoggingDatabaseChecker(this).Check(notifier);
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return _selfCertifyingDataAccessPoint.GetQuerySyntaxHelper();
        }
        /// <inheritdoc/>
        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            return _selfCertifyingDataAccessPoint.GetCredentialsIfExists(context);
        }

        /// <inheritdoc/>
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
            DatabaseType = discoveredDatabase.Server.DatabaseType;

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

        public bool WasCreatedByDatabaseAssembly(Tier2DatabaseType type)
        {
            switch (type)
            {
                case Tier2DatabaseType.Logging:
                    return CreatedByAssembly == "HIC.Logging.Database";
                case Tier2DatabaseType.DataQuality:
                    return CreatedByAssembly == "DataQualityEngine.Database";
                case Tier2DatabaseType.QueryCaching:
                    return CreatedByAssembly == "QueryCaching.Database";
                case Tier2DatabaseType.ANOStore:
                    return CreatedByAssembly == "ANOStore.Database";
                case Tier2DatabaseType.IdentifierDump:
                    return CreatedByAssembly == "IdentifierDump.Database";
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <inheritdoc/>
        public DiscoveredDatabase Discover(DataAccessContext context)
        {
            return DataAccessPortal.GetInstance().ExpectDatabase(this, context);
        }
    }
}
