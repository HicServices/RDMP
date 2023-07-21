// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Repositories;

/// <summary>
///     A class which can find the location (connection strings) of the of Catalogue and Data Export databases.  This might
///     come from a user settings file or from a
///     config file or whatever (depending on how you implement this interface).
/// </summary>
public interface IRDMPPlatformRepositoryServiceLocator : ICatalogueRepositoryServiceLocator,
    IDataExportRepositoryServiceLocator
{
    /// <summary>
    ///     Cross repository method equivallent to GetObjectByID mostly used in persistence recovery (when you startup RDMP
    ///     after closing it down before).  It is better
    ///     to use the specific repository methods on the CatalogueRepository / DataExportRepository.
    /// </summary>
    /// <param name="repositoryTypeName"></param>
    /// <param name="databaseObjectTypeName"></param>
    /// <param name="objectID"></param>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName,
        int objectID);

    bool ArbitraryDatabaseObjectExists(string repositoryTypeName, string databaseObjectTypeName, int objectID);

    /// <summary>
    ///     Cross repository method equivallent to GetObjectByID.  Only works if <typeparamref name="T" /> is a Type supported
    ///     by either
    ///     CatalogueRepository / DataExportRepository (i.e. you can't fetch something like an <see cref="Evaluation" /> -
    ///     which is a DQE object)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable GetObjectByID<T>(int value) where T : IMapsDirectlyToDatabaseTable;


    /// <summary>
    ///     Cross repository method equivallent to GetObjectByID.  Only works if <paramref name="t" /> is a Type supported by
    ///     either
    ///     CatalogueRepository / DataExportRepository (i.e. you can't fetch something like an <see cref="Evaluation" /> -
    ///     which is a DQE object)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable GetObjectByID(Type t, int value);

    /// <summary>
    ///     Returns all repositories (including plugin repositories if supported) known about by this
    ///     repository locator.
    /// </summary>
    /// <returns></returns>
    IEnumerable<IRepository> GetAllRepositories();
}