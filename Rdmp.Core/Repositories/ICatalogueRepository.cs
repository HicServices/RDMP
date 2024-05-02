// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Repositories;

/// <summary>
///     Pointer to the main RDMP Catalogue Repository database in which all DatabaseEntities relating to data curation,
///     governance, load etc are stored.  Every DatabaseEntity class must exist in a
///     Microsoft Sql Server Database (See DatabaseEntity) and each object is compatible only with a specific type of
///     TableRepository (i.e. the database that contains the
///     table matching their name).
///     <para>
///         This class allows you to fetch objects and should be passed into constructors of classes you want to
///         construct in the Catalogue database.
///     </para>
///     <para>
///         It also includes helper properties for setting up relationships and controling records in the non
///         DatabaseEntity tables in the database e.g. <see cref="AggregateForcedJoinManager" />
///     </para>
/// </summary>
public interface ICatalogueRepository : IRepository, IServerDefaults
{
    /// <summary>
    ///     Allows creation/discover/deletion of <see cref="AggregateForcedJoin" /> objects
    /// </summary>
    IAggregateForcedJoinManager AggregateForcedJoinManager { get; }

    IGovernanceManager GovernanceManager { get; }

    /// <summary>
    ///     Allows linking/unlinking <see cref="DataAccessCredentials" /> to <see cref="TableInfo" />
    /// </summary>
    ITableInfoCredentialsManager TableInfoCredentialsManager { get; }


    /// <summary>
    ///     Allows creation/discover of <see cref="JoinInfo" /> objects which describe how to join two <see cref="TableInfo" />
    ///     together in SQL
    /// </summary>
    IJoinManager JoinManager { get; }


    /// <summary>
    ///     Stores class comments discovered at startup
    /// </summary>
    CommentStore CommentStore { get; set; }

    string GetEncryptionKeyPath();

    /// <summary>
    ///     Manages information about what set containers / subcontainers exist under a
    ///     <see cref="CohortIdentificationConfiguration" />
    /// </summary>
    ICohortContainerManager CohortContainerManager { get; }

    /// <summary>
    ///     Handles encrypting/decrypting strings with private/public key encryption
    /// </summary>
    IEncryptionManager EncryptionManager { get; }

    /// <summary>
    ///     Handles forbidding deleting stuff / cascading deletes into other objects
    /// </summary>
    IObscureDependencyFinder ObscureDependencyFinder { get; set; }

    /// <summary>
    ///     Manager for AND/OR WHERE containers and filters
    /// </summary>
    IFilterManager FilterManager { get; }

    /// <summary>
    ///     Returns a new <see cref="LogManager" /> that audits in the default logging server specified by
    ///     <see cref="ServerDefaults" />
    /// </summary>
    /// <returns></returns>
    LogManager GetDefaultLogManager();

    /// <summary>
    ///     Returns all sql parameters declared in the immediate scope of the <paramref name="parent" /> (does not include
    ///     parameters that are declared at a lower scope).
    ///     <para>To determine which parent types are supported see <see cref="AnyTableSqlParameter.IsSupportedType" /></para>
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent);

    /// <summary>
    ///     Returns the persistence object which describes which <see cref="ITicketingSystem" /> should be consulted when
    ///     making governance decisions (e.g. according to
    ///     ticketing system, can I release this dataset?).  Returns null if no ticketing system has been configured.
    ///     <para>Use <see cref="TicketingSystemFactory" /> to instantiate an <see cref="ITicketingSystem" /> instance</para>
    /// </summary>
    /// <returns></returns>
    TicketingSystemConfiguration GetTicketingSystem();

    T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity;


    /// <summary>
    ///     True if the <paramref name="tableInfo" /> has <see cref="Lookup" /> relationships declared which make it a linkable
    ///     lookup table in queries.
    /// </summary>
    /// <returns></returns>
    bool IsLookupTable(ITableInfo tableInfo);

    void SetEncryptionKeyPath(string fullName);

    /// <summary>
    ///     Returns all Catalogues which have any CatalogueItems which are associated with any of the ColumnInfos of this
    ///     TableInfo.  If this is a lookup table then expect to get back
    ///     a whole bunch of catalogues.
    /// </summary>
    /// <returns></returns>
    Catalogue[] GetAllCataloguesUsing(TableInfo tableInfo);

    /// <summary>
    ///     Returns all <see cref="ExternalDatabaseServer" /> which were created by the patcher specified.  The patcher must
    ///     have a blank constructor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    ExternalDatabaseServer[] GetAllDatabases<T>() where T : IPatcher, new();

    void DeleteEncryptionKeyPath();

    /// <summary>
    ///     Returns all <see cref="ExtendedProperty" /> that are declared on
    ///     <paramref name="obj" /> where the property Name is
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName, IMapsDirectlyToDatabaseTable obj);


    /// <summary>
    ///     Returns all <see cref="ExtendedProperty" /> declared on any objects
    ///     Named <paramref name="propertyName" />
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName);

    /// <summary>
    ///     Returns all <see cref="ExtendedProperty" /> declared on <paramref name="obj" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    IEnumerable<ExtendedProperty> GetExtendedProperties(IMapsDirectlyToDatabaseTable obj);
}