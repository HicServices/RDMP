// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Connections;
using FAnsi.Discovery;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Since every agency handles cohort management differently the RDMP is built to supports diverse cohort source
///     schemas.  Unlike the logging, dqe, catalogue databases etc there
///     is no fixed managed schema for cohort databases.  Instead you simply have to tell the software where to find your
///     patient identifiers in an ExternalCohortTable record.  This
///     stores:
///     <para>
///         What table contains your cohort identifiers;
///         Which column is the private identifier;
///         Which column is the release identifier.
///     </para>
///     <para>
///         In addition to this you must have a table which describes your cohorts which must have columns called id,
///         projectNumber, description, version and dtCreated.
///     </para>
///     <para>Both the cohort and custom table names table must have a foreign key into the definition table.  </para>
///     <para>
///         You are free to add additional columns to these tables or even base them on views of other existing tables in
///         your database.  You can have multiple ExternalCohortTable sources
///         in your database for example if you need to support different identifier datatypes / formats.
///     </para>
///     <para>
///         <see cref="Rdmp.Core.CohortCommitting.CreateNewCohortDatabaseWizard" /> to automatically generate a database
///         that is compatible with the format requirements and has
///         release identifiers assigned automatically either as autonums or GUIDs (I suggest using GUIDs to prevent
///         accidental crosstalk from ever occuring if you handle magic numbers from
///         other agencies).
///     </para>
/// </summary>
public interface IExternalCohortTable : ICheckable, IDataAccessPoint, IHasDependencies, INamed
{
    /// <summary>
    ///     Name of the table in your database that contains the private to release identifier mappings (e.g. 'Cohort').
    /// </summary>
    string TableName { get; set; }

    /// <summary>
    ///     Name of the inventory table in your database which records the names and versions of cohorts stored in the cohort
    ///     table (e.g. 'CohortDefinition').
    /// </summary>
    string DefinitionTableName { get; set; }

    /// <summary>
    ///     The column in the cohort table which contains private identifiers (e.g. 'chi').  This column must contain
    ///     identifiers in the same format
    ///     as the datasets you want to link with when performing project extractions.
    /// </summary>
    string PrivateIdentifierField { get; set; }

    /// <summary>
    ///     The column in the cohort table which contains release identifiers (e.g. 'ReleaseId).  These are the values that
    ///     will be substituted in during
    ///     the linkage stage of project extractions.
    /// </summary>
    string ReleaseIdentifierField { get; set; }

    /// <summary>
    ///     The name of the field in the cohort (mapping) table which is a foreign key into <see cref="DefinitionTableName" />
    ///     (cohort descriptions) table (e.g. cohortDefinition_id).
    /// </summary>
    string DefinitionTableForeignKeyField { get; set; }

    /// <summary>
    ///     Returns an object for connecting to/interacting with the cohort database referenced by this object.
    /// </summary>
    /// <returns></returns>
    DiscoveredDatabase Discover();

    /// <summary>
    ///     Returns the cohort table (linkage table with the <see cref="PrivateIdentifierField" /> and
    ///     <see cref="ReleaseIdentifierField" />)
    /// </summary>
    /// <returns></returns>
    DiscoveredTable DiscoverCohortTable();

    /// <summary>
    ///     Returns the cohort inventory table (has 1 record per cohort which contains name, description, version etc).
    /// </summary>
    /// <returns></returns>
    DiscoveredTable DiscoverDefinitionTable();

    /// <summary>
    ///     Returns the column in the <see cref="DiscoverCohortTable" /> which matches the
    ///     <see cref="PrivateIdentifierField" />
    /// </summary>
    /// <returns></returns>
    DiscoveredColumn DiscoverPrivateIdentifier();

    /// <summary>
    ///     Returns the column in the <see cref="DiscoverCohortTable" /> which matches the
    ///     <see cref="ReleaseIdentifierField" />
    /// </summary>
    /// <returns></returns>
    DiscoveredColumn DiscoverReleaseIdentifier();

    /// <summary>
    ///     Returns the column in the <see cref="DiscoverCohortTable" /> which matches the
    ///     <see cref="DefinitionTableForeignKeyField" />
    /// </summary>
    /// <returns></returns>
    DiscoveredColumn DiscoverDefinitionTableForeignKey();

    /// <summary>
    ///     Creates a new record in the <see cref="DefinitionTableName" /> table referenced by this object.
    /// </summary>
    /// <param name="newCohortDefinition"></param>
    /// <param name="connection"></param>
    void PushToServer(ICohortDefinition newCohortDefinition, IManagedConnection connection);

    /// <summary>
    ///     Connects to the cohort database and looks for a cohort definition record with the given
    ///     <paramref name="originID" />.  Returns
    ///     true if a record exists.
    /// </summary>
    /// <param name="originID"></param>
    /// <returns></returns>
    bool IDExistsInCohortTable(int originID);
}