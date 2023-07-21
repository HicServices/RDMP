// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Repositories;

/// <summary>
///     Use when you have an already initialized set of repositories and only want to fetch objects from the catalogue/data
///     export repositories
/// </summary>
public class RepositoryProvider : IRDMPPlatformRepositoryServiceLocator
{
    private readonly Dictionary<string, Type> _cachedTypesByNameDictionary = new();

    private readonly object oLockDictionary = new();

    /// <summary>
    ///     Use when you have an already initialized set of repositories.  Sets up the class to fetch objects from the
    ///     Catalogue/Data export databases only.
    ///     <para>
    ///         If possible consider using LinkedRepositoryProvider or Startup (these support plugin repositories, DQE
    ///         repository etc)
    ///     </para>
    /// </summary>
    /// <param name="dataExportRepository"></param>
    public RepositoryProvider(IDataExportRepository dataExportRepository)
    {
        CatalogueRepository = dataExportRepository.CatalogueRepository;
        DataExportRepository = dataExportRepository;
    }

    protected RepositoryProvider()
    {
    }

    public ICatalogueRepository CatalogueRepository { get; protected set; }
    public IDataExportRepository DataExportRepository { get; protected set; }

    public IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName,
        string databaseObjectTypeName, int objectId)
    {
        var repository = GetRepository(repositoryTypeName);
        var objectType = GetTypeByName(databaseObjectTypeName, typeof(IMapsDirectlyToDatabaseTable));

        if (!repository.StillExists(objectType, objectId))
            return null;

        return repository.GetObjectByID(objectType, objectId);
    }

    public bool ArbitraryDatabaseObjectExists(string repositoryTypeName, string databaseObjectTypeName, int objectID)
    {
        //if the repository/object type is unknown then it doesn't exist
        if (string.IsNullOrWhiteSpace(repositoryTypeName) || string.IsNullOrWhiteSpace(databaseObjectTypeName))
            return false;

        var repository = GetRepository(repositoryTypeName);
        var objectType = GetTypeByName(databaseObjectTypeName, typeof(IMapsDirectlyToDatabaseTable));

        return repository.StillExists(objectType, objectID);
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable GetObjectByID<T>(int value) where T : IMapsDirectlyToDatabaseTable
    {
        if (CatalogueRepository.SupportsObjectType(typeof(T)))
            return CatalogueRepository.GetObjectByID<T>(value);
        if (DataExportRepository.SupportsObjectType(typeof(T)))
            return DataExportRepository.GetObjectByID<T>(value);
        throw new ArgumentException($"Did not know what repository to use to fetch objects of Type '{typeof(T)}'");
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable GetObjectByID(Type t, int value)
    {
        if (CatalogueRepository.SupportsObjectType(t))
            return CatalogueRepository.GetObjectByID(t, value);
        if (DataExportRepository.SupportsObjectType(t))
            return DataExportRepository.GetObjectByID(t, value);
        throw new ArgumentException($"Did not know what repository to use to fetch objects of Type '{t}'");
    }

    public virtual IEnumerable<IRepository> GetAllRepositories()
    {
        yield return CatalogueRepository;

        if (DataExportRepository != null)
            yield return DataExportRepository;
    }

    protected virtual IRepository GetRepository(string s)
    {
        var repoType = GetTypeByName(s, typeof(IRepository));

        if (typeof(ICatalogueRepository).IsAssignableFrom(repoType))
            return CatalogueRepository;

        if (typeof(IDataExportRepository).IsAssignableFrom(repoType))
            return DataExportRepository;

        throw new NotSupportedException(
            $"Did not know what instance of IRepository to use for IRepository Type '{repoType}' , expected it to either be CatalogueRepository or DataExportRepository");
    }

    private Type GetTypeByName(string s, Type expectedBaseClassType)
    {
        Type toReturn;
        lock (oLockDictionary)
        {
            if (_cachedTypesByNameDictionary.TryGetValue(s, out var name))
                return name;

            toReturn = CatalogueRepository.MEF.GetType(s, expectedBaseClassType);

            if (toReturn == null)
                throw new TypeLoadException($"Could not find Type called '{s}'");

            if (expectedBaseClassType != null)
                if (!expectedBaseClassType.IsAssignableFrom(toReturn))
                    throw new TypeLoadException(
                        $"Found Type '{s}' which we managed to find but it did not match an expected base Type ({expectedBaseClassType})");

            //cache known type to not hammer reflection all the time!
            _cachedTypesByNameDictionary.Add(s, toReturn);
        }

        return toReturn;
    }
}