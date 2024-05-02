// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;

namespace Rdmp.Core.CommandLine.DatabaseCreation;

/// <summary>
///     IRDMPPlatformRepositoryServiceLocator which identifies the location of Catalogue and Data Export databases during
///     the runtime of DatabaseCreation.exe
///     <para>
///         Since these connection strings are part of the command line arguments to DatabaseCreation.exe it's a pretty
///         simple class!
///     </para>
/// </summary>
public class PlatformDatabaseCreationRepositoryFinder : IRDMPPlatformRepositoryServiceLocator
{
    private readonly LinkedRepositoryProvider _linkedRepositoryProvider;

    public ICatalogueRepository CatalogueRepository => _linkedRepositoryProvider.CatalogueRepository;

    public IDataExportRepository DataExportRepository => _linkedRepositoryProvider.DataExportRepository;

    public IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName,
        string databaseObjectTypeName, int objectID)
    {
        return _linkedRepositoryProvider.GetArbitraryDatabaseObject(repositoryTypeName, databaseObjectTypeName,
            objectID);
    }

    public bool ArbitraryDatabaseObjectExists(string repositoryTypeName, string databaseObjectTypeName, int objectID)
    {
        return _linkedRepositoryProvider.ArbitraryDatabaseObjectExists(repositoryTypeName, databaseObjectTypeName,
            objectID);
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable GetObjectByID<T>(int value) where T : IMapsDirectlyToDatabaseTable
    {
        return _linkedRepositoryProvider.GetObjectByID<T>(value);
    }

    public IMapsDirectlyToDatabaseTable GetObjectByID(Type t, int value)
    {
        return _linkedRepositoryProvider.GetObjectByID(t, value);
    }

    public IEnumerable<IRepository> GetAllRepositories()
    {
        return _linkedRepositoryProvider.GetAllRepositories();
    }

    public PlatformDatabaseCreationRepositoryFinder(PlatformDatabaseCreationOptions options)
    {
        var cata = options.GetBuilder(PlatformDatabaseCreation.DefaultCatalogueDatabaseName);
        var export = options.GetBuilder(PlatformDatabaseCreation.DefaultDataExportDatabaseName);

        _linkedRepositoryProvider = new LinkedRepositoryProvider(cata.ConnectionString, export.ConnectionString);
    }
}