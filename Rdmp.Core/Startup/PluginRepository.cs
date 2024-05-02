// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Startup;

/// <summary>
///     Repository for constructing/saving/deleting <see cref="DatabaseEntity" /> objects that are are stored in your
///     plugin database.
///     The assembly containing your <see cref="PluginRepository" /> must be the same assembly that contains the class
///     definitions.
/// </summary>
public abstract class PluginRepository : TableRepository
{
    public ExternalDatabaseServer ExternalDatabaseServer { get; set; }

    /// <summary>
    ///     Sets up the repository for reading and writing objects out of the given <paramref name="externalDatabaseServer" />.
    /// </summary>
    /// <param name="externalDatabaseServer">The database to connect to</param>
    /// <param name="dependencyFinder">
    ///     Optional class that can forbid deleting objects because you have dependencies on them in
    ///     your database (e.g. if your custom object has a field Catalogue_ID)
    /// </param>
    protected PluginRepository(ExternalDatabaseServer externalDatabaseServer, IObscureDependencyFinder dependencyFinder)
        : base(dependencyFinder,
            externalDatabaseServer.Discover(DataAccessContext.InternalDataProcessing).Server.Builder)
    {
        ExternalDatabaseServer = externalDatabaseServer;
    }

    protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
    {
        return ObjectConstructor.ConstructIMapsDirectlyToDatabaseObject(t, this, reader);
    }
}