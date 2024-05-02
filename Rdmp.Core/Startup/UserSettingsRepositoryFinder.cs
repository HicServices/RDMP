// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Startup;

/// <summary>
///     Records connection strings to the Catalogue and DataExport databases (See LinkedRepositoryProvider) in the user
///     settings file for the current
///     user.
///     <para>
///         Use properties CatalogueRepository and DataExportRepository for interacting with objects saved in those
///         databases (and to create new ones).
///     </para>
/// </summary>
public class UserSettingsRepositoryFinder : IRDMPPlatformRepositoryServiceLocator
{
    private LinkedRepositoryProvider _linkedRepositoryProvider;

    public ICatalogueRepository CatalogueRepository
    {
        get
        {
            if (_linkedRepositoryProvider == null)
                RefreshRepositoriesFromUserSettings();

            return _linkedRepositoryProvider == null
                ? throw new Exception(
                    "RefreshRepositoriesFromUserSettings failed to populate_linkedRepositoryProvider as expected ")
                : _linkedRepositoryProvider.CatalogueRepository;
        }
    }

    public IDataExportRepository DataExportRepository
    {
        get
        {
            if (_linkedRepositoryProvider == null)
                RefreshRepositoriesFromUserSettings();

            return _linkedRepositoryProvider == null
                ? throw new Exception(
                    "RefreshRepositoriesFromUserSettings failed to populate_linkedRepositoryProvider as expected ")
                : _linkedRepositoryProvider.DataExportRepository;
        }
    }

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

    public void RefreshRepositoriesFromUserSettings()
    {
        CommentStore commentStore = null;

        //if we have a catalogue repository with loaded CommentStore then grab it
        if (_linkedRepositoryProvider is { CatalogueRepository.CommentStore: not null })
            commentStore = _linkedRepositoryProvider.CatalogueRepository.CommentStore;

        //user must have a Catalogue
        var catalogueString = UserSettings.CatalogueConnectionString;

        //user may have a DataExportManager
        var dataExportManagerConnectionString = UserSettings.DataExportConnectionString;

        LinkedRepositoryProvider newrepo;

        try
        {
            newrepo = new LinkedRepositoryProvider(catalogueString, dataExportManagerConnectionString);
        }
        catch (Exception ex)
        {
            throw new CorruptRepositoryConnectionDetailsException(
                $"Unable to create {nameof(LinkedRepositoryProvider)}", ex);
        }

        //preserve the currently loaded MEF assemblies

        //if we have a new repo
        if (newrepo.CatalogueRepository != null)
            newrepo.CatalogueRepository.CommentStore = commentStore ?? newrepo.CatalogueRepository.CommentStore;


        _linkedRepositoryProvider = newrepo;
    }

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
}