// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ExternalDatabaseServer
    /// </summary>
    public interface IExternalDatabaseServer : IDataAccessPoint, INamed
    {
        /// <summary>
        /// Determines whether the given database server was created by the specified .Database assembly e.g. (DataQualityEngine.Database.dll).  If it is then the 
        /// schema will match, database objects will be retrievable through the host assembly (e.g. DataQualityEngine.dll) etc.
        /// </summary>
        /// <param name="databaseAssembly"></param>
        /// <returns></returns>
        bool WasCreatedByDatabaseAssembly(Assembly databaseAssembly);


        /// <summary>
        /// Provides a live object for interacting directly with the server referenced by this <see cref="IExternalDatabaseServer"/>.  This will wokr
        /// even if the server is unreachable (See <see cref="IMightNotExist"/>)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        DiscoveredDatabase Discover(DataAccessContext context);
    }
}