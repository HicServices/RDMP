// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Databases;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <inheritdoc cref="IExternalDatabaseServer"/>
public class ExternalDatabaseServer : DatabaseEntity, IExternalDatabaseServer, IDataAccessCredentials, INamed,
    ICheckable
{
    #region Database Properties

    private string _name;
    private string _createdByAssembly;
    private string _mappedDataPath;
    private readonly SelfCertifyingDataAccessPoint _selfCertifyingDataAccessPoint;

    /// <summary>
    /// Human-readable name for the server e.g. 'My Favourite Logging Database'
    /// </summary>
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// If the database was created by an RDMP schema (or plugin schema) this will contain the name of the dll which holds the schema e.g. DataQualityEngine.Database and was
    /// responsible for creating the database.  This determines what roles RDMP lets the database play.
    /// </summary>
    public string CreatedByAssembly
    {
        get => _createdByAssembly;
        set => SetField(ref _createdByAssembly, value);
    }

    /// <summary>
    /// The public network share of the Data path where the physical database files are stored if applicable.  Sharing your database directory on the network is a
    /// terrible idea (don't do it).  You can use this to automate detaching and shipping an MDF to your researchers e.g. MsSqlReleaseSource
    /// </summary>
    public string MappedDataPath
    {
        get => _mappedDataPath;
        set => SetField(ref _mappedDataPath, value);
    }

    /// <inheritdoc/>
    public string Server
    {
        get => _selfCertifyingDataAccessPoint.Server;
        set
        {
            if (Equals(_selfCertifyingDataAccessPoint.Server, value))
                return;

            var old = _selfCertifyingDataAccessPoint.Server;
            _selfCertifyingDataAccessPoint.Server = value;
            OnPropertyChanged(old, value);
        }
    }

    /// <inheritdoc/>
    public string Database
    {
        get => _selfCertifyingDataAccessPoint.Database;
        set
        {
            if (Equals(_selfCertifyingDataAccessPoint.Database, value))
                return;

            var old = _selfCertifyingDataAccessPoint.Database;
            _selfCertifyingDataAccessPoint.Database = value;
            OnPropertyChanged(old, value);
        }
    }

    /// <inheritdoc/>
    public string Username
    {
        get => _selfCertifyingDataAccessPoint.Username;
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
        get => _selfCertifyingDataAccessPoint.Password;
        set
        {
            _selfCertifyingDataAccessPoint.Password = value;
            OnPropertyChanged(null, value);
        }
    }

    /// <inheritdoc/>
    public DatabaseType DatabaseType
    {
        get => _selfCertifyingDataAccessPoint.DatabaseType;
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

    public ExternalDatabaseServer()
    {
        _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint
        {
            DatabaseType = DatabaseType.MicrosoftSQLServer
        };
    }

    /// <summary>
    /// Creates a new persistent server reference in RDMP platform database that allows it to connect to a (usually database) server.
    /// 
    /// <para>If you are trying to create a database (e.g. a logging database) you should instead use
    /// <see cref="MapsDirectlyToDatabaseTable.Versioning.MasterDatabaseScriptExecutor"/></para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    /// <param name="creatorIfAny">If the database referenced was created according to a specific SQL schema, this is the schema provider</param>
    public ExternalDatabaseServer(ICatalogueRepository repository, string name, IPatcher creatorIfAny)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Name", name },
            { "DatabaseType", DatabaseType.MicrosoftSQLServer }
        };

        if (creatorIfAny != null)
            parameters.Add("CreatedByAssembly", creatorIfAny.Name);

        Repository = repository;
        _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(repository, DatabaseType.MicrosoftSQLServer);
        repository.InsertAndHydrate(this, parameters);
    }


    internal ExternalDatabaseServer(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        var repo = shareManager.RepositoryLocator.CatalogueRepository;
        Repository = repo;
        _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(CatalogueRepository,
            DatabaseType.MicrosoftSQLServer /*will get changed by UpsertAndHydrate*/);

        shareManager.UpsertAndHydrate(this, shareDefinition);
    }

    internal ExternalDatabaseServer(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Name = r["Name"] as string;
        CreatedByAssembly = r["CreatedByAssembly"] as string;
        MappedDataPath = r["MappedDataPath"] as string;

        var databaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), r["DatabaseType"].ToString());

        _selfCertifyingDataAccessPoint = new SelfCertifyingDataAccessPoint(repository, databaseType)
        {
            Database = r["Database"] as string,
            Password = r["Password"] as string,
            Server = r["Server"] as string,
            Username = r["Username"] as string
        };
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    public void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(Server))
            notifier.OnCheckPerformed(new CheckEventArgs("No Server set", CheckResult.Warning));
        else if (string.IsNullOrWhiteSpace(Database))
            notifier.OnCheckPerformed(new CheckEventArgs("No Database set", CheckResult.Warning));
        else
            try
            {
                DataAccessPortal.ExpectServer(this, DataAccessContext.InternalDataProcessing).TestConnection();
                notifier.OnCheckPerformed(new CheckEventArgs("Successfully connected to server", CheckResult.Success));
            }
            catch (Exception exception)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs("Failed to connect to server", CheckResult.Fail, exception));
                return;
            }

        //if it's a logging server run logging checks
        if (WasCreatedBy(new LoggingDatabasePatcher()))
            new LoggingDatabaseChecker(this).Check(notifier);
    }

    /// <inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() => _selfCertifyingDataAccessPoint.GetQuerySyntaxHelper();

    /// <inheritdoc/>
    public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context) =>
        _selfCertifyingDataAccessPoint.GetCredentialsIfExists(context);

    /// <inheritdoc/>
    public string GetDecryptedPassword() => _selfCertifyingDataAccessPoint.GetDecryptedPassword() ?? "";

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

        if (save)
            SaveToDatabase();
    }

    /// <inheritdoc/>
    public bool WasCreatedBy(IPatcher patcher) => !string.IsNullOrWhiteSpace(CreatedByAssembly) &&
                                                  (patcher.Name == CreatedByAssembly ||
                                                   patcher.LegacyName == CreatedByAssembly);

    /// <inheritdoc/>
    public DiscoveredDatabase Discover(DataAccessContext context) => _selfCertifyingDataAccessPoint.Discover(context);

    /// <inheritdoc/>
    public bool DiscoverExistence(DataAccessContext context, out string reason) =>
        _selfCertifyingDataAccessPoint.DiscoverExistence(context, out reason);


    public override void DeleteInDatabase()
    {

         if (WasCreatedBy(new LoggingDatabasePatcher())){
            //If you're trying to delete a logging server, remove all references to it first
            var catalogues = Repository.GetAllObjectsWhere<Catalogue>("LiveLoggingServer_ID",ID);
            foreach(var catalogue in catalogues){
                catalogue.LiveLoggingServer_ID = null;
                catalogue.SaveToDatabase();
            }
         }
         base.DeleteInDatabase();

         // normally in database schema deleting an ExternalDatabaseServer will cascade to clear defaults
         // but some repositories do not support this implicit removal so lets double check there are no references
         foreach (PermissableDefaults d in Enum.GetValues(typeof(PermissableDefaults)))
         {
             var existingDefault = CatalogueRepository.GetDefaultFor(d);
             if (Equals(existingDefault)) CatalogueRepository.ClearDefault(d);
         }
    }

    public void SetRepository(ICatalogueRepository repository)
    {
        _selfCertifyingDataAccessPoint.SetRepository(repository);
    }
}