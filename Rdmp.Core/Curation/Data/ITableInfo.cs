// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     A persistent reference to an existing Database Table (See TableInfo).
/// </summary>
public interface ITableInfo : IComparable, IHasRuntimeName, IDataAccessPoint, IHasDependencies,
    ICollectSqlParameters, INamed, IInjectKnown, ICheckable
{
    /// <summary>
    ///     The Schema scope of the table (or blank if dbo / default / not supported by dbms).  This scope exists below
    ///     Database and Above Table.  Not all database management
    ///     engines support the concept of Schema (e.g. MySql).
    /// </summary>
    string Schema { get; set; }

    /// <summary>
    ///     True if the table referenced is an sql server table valued function (which probably takes parameters)
    /// </summary>
    bool IsTableValuedFunction { get; set; }


    /// <summary>
    ///     <para>
    ///         Indicates that this TableInfo should be the first table joined in any query that has multiple other
    ///         TableInfos
    ///     </para>
    ///     <para>
    ///         When determining how to join a collection of TableInfos the <see cref="QueryBuilder" /> will attempt to find
    ///         <see cref="JoinInfo" /> pairings between <see cref="ColumnInfo" /> in
    ///         the tables.  If it cannot work out how to resolve the join order (e.g. if there are 3+ tables and joins going
    ///         in both directions) then it will demand that one of the
    ///         <see cref="TableInfo" /> be picked as the first table from which all other tables should then be joined.
    ///     </para>
    /// </summary>
    bool IsPrimaryExtractionTable { get; set; }

    /// <summary>
    ///     The server that stores <see cref="PreLoadDiscardedColumn" /> values which do not make it to LIVE during a data load
    ///     e.g. because they contain identifiable data that
    ///     must be split off (e.g. <see cref="DiscardedColumnDestination.StoreInIdentifiersDump" />).
    /// </summary>
    int? IdentifierDumpServer_ID { get; set; }

    /// <inheritdoc cref="IdentifierDumpServer_ID" />
    ExternalDatabaseServer IdentifierDumpServer { get; }

    /// <summary>
    ///     Gets the name of the table in the given RAW=>STAGING=>LIVE section of a DLE run using the provided
    ///     <paramref name="tableNamingScheme" />
    /// </summary>
    /// <param name="bubble"></param>
    /// <param name="tableNamingScheme"></param>
    /// <returns></returns>
    string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);

    /// <inheritdoc cref="GetRuntimeName(LoadBubble,INameDatabasesAndTablesDuringLoads)" />
    string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);

    /// <summary>
    ///     Fetches all the ColumnInfos associated with this TableInfo (This is refreshed every time you call this property)
    /// </summary>
    [NoMappingToDatabase]
    ColumnInfo[] ColumnInfos { get; }

    /// <summary>
    ///     Gets all the <see cref="PreLoadDiscardedColumn" /> declared against this table reference.  These are virtual
    ///     columns which
    ///     do not exist in the LIVE table schema (Unless <see cref="DiscardedColumnDestination.Dilute" />) but which appear in
    ///     the RAW
    ///     stage of the data load.
    ///     <para>See <see cref="PreLoadDiscardedColumn" /> for more information</para>
    /// </summary>
    [NoMappingToDatabase]
    PreLoadDiscardedColumn[] PreLoadDiscardedColumns { get; }

    /// <summary>
    ///     True if the <see cref="TableInfo" /> has <see cref="Lookup" /> relationships declared which make it a linkable
    ///     lookup table in queries.
    /// </summary>
    /// <returns></returns>
    bool IsLookupTable();

    /// <inheritdoc cref="IsLookupTable()" />
    /// <remarks>High performance version for when you have an <see cref="ICoreChildProvider" /></remarks>
    public bool IsLookupTable(ICoreChildProvider childProvider);

    /// <summary>
    ///     Returns the <see cref="IDataAccessPoint.Database" /> name at the given <paramref name="loadStage" /> of a DLE run
    ///     (RAW=>STAGING=>LIVE)
    /// </summary>
    /// <param name="loadStage"></param>
    /// <param name="namer"></param>
    /// <returns></returns>
    string GetDatabaseRuntimeName(LoadStage loadStage, INameDatabasesAndTablesDuringLoads namer = null);

    /// <summary>
    ///     Returns all column names for the given <see cref="LoadStage" /> (RAW=>STAGING=>LIVE) of a data load
    /// </summary>
    /// <param name="loadStage"></param>
    /// <returns></returns>
    IEnumerable<IHasStageSpecificRuntimeName> GetColumnsAtStage(LoadStage loadStage);

    /// <summary>
    ///     Creates an object for interacting with the table as it exists on the live server referenced by this
    ///     <see cref="TableInfo" />
    ///     <para>
    ///         This will not throw if the table doesn't exist, instead you should use <see cref="DiscoveredTable.Exists" /> on
    ///         the
    ///         returned value
    ///     </para>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    DiscoveredTable Discover(DataAccessContext context);

    /// <summary>
    ///     Returns the fully qualified name of the TableInfo
    /// </summary>
    /// <returns></returns>
    string GetFullyQualifiedName();


    /// <summary>
    ///     Returns <see cref="IDataAccessPoint.Database" /> trimmed of any database qualifiers (e.g. square brackets)
    /// </summary>
    /// <returns></returns>
    string GetDatabaseRuntimeName();

    /// <summary>
    ///     True if the object referenced is a database view
    /// </summary>
    bool IsView { get; set; }

    /// <summary>
    ///     The repository this object is stored in
    /// </summary>
    ICatalogueRepository CatalogueRepository { get; }

    /// <summary>
    ///     Returns all Catalogues which have any CatalogueItems which are associated with any of the ColumnInfos of this
    ///     TableInfo.  If this is a lookup table then expect to get back
    ///     a whole bunch of catalogues.  If you have multiple extractable catalogues that all present different views of a
    ///     single TableInfo then they will all be returned.  The normal
    ///     behaviour though for a regular data table with one catalogue used for extraction would be for a single Catalogue to
    ///     get returned.
    /// </summary>
    /// <returns></returns>
    Catalogue[] GetAllRelatedCatalogues();
}