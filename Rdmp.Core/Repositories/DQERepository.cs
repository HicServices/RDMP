// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Repositories;

/// <summary>
///     Pointer to the Data Qualilty Engine Repository database in which all DatabaseEntities related to Data Quality
///     Engine runs are stored (e.g. Evaluation).  Every
///     DatabaseEntity class must exist in a Microsoft Sql Server Database (See DatabaseEntity) and each object is
///     compatible only with a specific type of TableRepository
///     (i.e. the database that contains the table matching their name).
///     <para>
///         This class allows you to fetch objects and should be passed into constructors of classes you want to
///         construct in the Data Quality database.
///     </para>
///     <para>
///         Data Qualilty Engine databases are only valid when you have a CatalogueRepository database too and are always
///         paired to a specific CatalogueRepository database (i.e.
///         there are IDs in the dqe database that specifically map to objects in the Catalogue database).  You can use the
///         CatalogueRepository property to fetch/create objects
///         in the paired Catalogue database.
///     </para>
/// </summary>
public class DQERepository : TableRepository, IDQERepository
{
    /// <inheritdoc />
    public ICatalogueRepository CatalogueRepository { get; }

    /// <summary>
    ///     Creates a new DQERepository storing/reading data from the default
    ///     DQE server configured in <paramref name="catalogueRepository" />
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <exception cref="NotSupportedException">If there is no default DQE database configured</exception>
    public DQERepository(ICatalogueRepository catalogueRepository)
    {
        CatalogueRepository = catalogueRepository;

        var server = CatalogueRepository.GetDefaultFor(PermissableDefaults.DQE) ?? throw new NotSupportedException(
            "There is no DataQualityEngine Reporting Server (ExternalDatabaseServer).  You will need to create/set one in CatalogueManager by using 'Locations=>Manage External Servers...'");
        DiscoveredServer = DataAccessPortal.ExpectServer(server, DataAccessContext.InternalDataProcessing);
        _connectionStringBuilder = DiscoveredServer.Builder;
    }

    /// <summary>
    ///     Creates a new DQERepository storing/reading data from <paramref name="db" /> instead of the default
    ///     (if any) listed in the <paramref name="catalogueRepository" />.  Use this constructor if you do not
    ///     want to use <see cref="IServerDefaults.GetDefaultFor(PermissableDefaults)" /> to find the DQE but instead want to
    ///     use an explicit database (<paramref name="db" />)
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <param name="db"></param>
    public DQERepository(ICatalogueRepository catalogueRepository, DiscoveredDatabase db)
    {
        CatalogueRepository = catalogueRepository;
        DiscoveredServer = db.Server;
        _connectionStringBuilder = DiscoveredServer.Builder;
    }

    /// <inheritdoc />
    public Evaluation GetMostRecentEvaluationFor(ICatalogue c)
    {
        return GetAllEvaluationsFor(c).MaxBy(e => e.DateOfEvaluation);
    }

    /// <inheritdoc />
    public IEnumerable<Evaluation> GetAllEvaluationsFor(ICatalogue catalogue)
    {
        return GetAllObjects<Evaluation>($"where CatalogueID = {catalogue.ID}").OrderBy(e => e.DateOfEvaluation);
    }

    /// <inheritdoc />
    public bool HasEvaluations(ICatalogue catalogue)
    {
        return GetAllEvaluationsFor(catalogue).Any();
    }

    protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
    {
        return ObjectConstructor.ConstructIMapsDirectlyToDatabaseObject(t, this, reader);
    }
}