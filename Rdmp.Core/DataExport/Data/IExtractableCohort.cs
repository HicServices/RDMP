// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     While actual patient identifiers are stored in an external database (referenced by a ExternalCohortTable), RDMP
///     still needs to have a reference to each cohort for extaction.
///     The ExtractableCohort object is a record that documents the location and ID of a cohort in your
///     ExternalCohortTable.  This record means that the RDMP can record which cohorts
///     are part of which ExtractionConfiguration in a Project without ever having to move the identifiers into the RDMP
///     application database.
///     <para>
///         The most important field in ExtractableCohort is the OriginID, this field represents the id of the cohort in
///         the CohortDefinition table of the ExternalCohortTable.  Effectively
///         this number is the id of the cohort in your cohort database while the ID property of the ExtractableCohort (as
///         opposed to OriginID) is the RDMP ID assigned to the cohort.  This
///         allows you to have two different cohort sources both of which have a cohort id 10 but the RDMP software is able
///         to tell the difference.  In addition it allows for the unfortunate
///         situation in which you delete a cohort in your cohort database and leave the ExtractableCohort orphaned - under
///         such circumstances you will at least still have your RDMP configuration
///         and know the location of the original cohort even if it doesn't exist anymore.
///     </para>
/// </summary>
public interface IExtractableCohort : IHasQuerySyntaxHelper, IMightBeDeprecated, IHasDependencies
{
    /// <summary>
    ///     Runs a (non distinct) count on the number of rows in the private/release identifier mapping table
    ///     stored in the <see cref="ExternalCohortTable" /> which match this cohorts <see cref="OriginID" />
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Runs a count distinct on the release identifier column of the cohort.  Result is
    ///     cached in memory for subsequent calls.
    /// </summary>
    int CountDistinct { get; }

    /// <summary>
    ///     Returns <see cref="CountDistinct" /> without caching and with an optional long timeout
    /// </summary>
    /// <param name="timeout">Timeout in seconds to allow operation to run for or -1 for default</param>
    /// <returns></returns>
    int GetCountDistinctFromDatabase(int timeout = -1);

    /// <summary>
    ///     The location of the cohort database in which this cohort is held (outside RDMP metadata databases).
    /// </summary>
    int ExternalCohortTable_ID { get; }

    /// <summary>
    ///     The id of the row in the remote <see cref="IExternalCohortTable.DefinitionTableName" /> that stores a description
    ///     of what is in this cohort (See <see cref="IExternalCohortDefinitionData" />).
    ///     <para>
    ///         Because you can have multiple cohort databases managed by RDMP, it is possible for 2+ different
    ///         <see cref="IExtractableCohort" />
    ///         to have the same <see cref="OriginID" /> (i.e. cohort 1 from source 1 is not the same as cohort 1 from source
    ///         2).
    ///     </para>
    /// </summary>
    int OriginID { get; }

    /// <summary>
    ///     Allows you to override the release identifier column setting in <see cref="IExternalCohortTable" /> when extracting
    ///     this specific
    ///     cohort list.  Use this only if your mapping table has multiple different types of release identifier (stored in
    ///     seperate columns).
    /// </summary>
    string OverrideReleaseIdentifierSQL { get; set; }

    /// <summary>
    ///     Log of activities relating to this cohort e.g. what it was created from (e.g. a CohortIdentificationConfiguration,
    ///     file etc).  This
    ///     field is held in RDMP database (i.e. not fetched from the remote cohort database -
    ///     <see cref="ExternalCohortTable" />).
    /// </summary>
    string AuditLog { get; set; }

    /// <inheritdoc cref="ExternalCohortTable_ID" />
    IExternalCohortTable ExternalCohortTable { get; }

    /// <summary>
    ///     Fetches all mappings (private to release identifier) for the cohort (also returns any other columns in the
    ///     <see cref="IExternalCohortTable.TableName" />)
    /// </summary>
    /// <returns></returns>
    DataTable FetchEntireCohort();

    /// <summary>
    ///     Returns the name of the identifiable private identifier column e.g. "chi".
    /// </summary>
    /// <param name="runtimeName">
    ///     True to return just the name e.g. "chi".  False to return the fully qualified name e.g.
    ///     "[mydb]..[mytbl].[chi]"
    /// </param>
    /// <returns></returns>
    string GetPrivateIdentifier(bool runtimeName = false);

    /// <summary>
    ///     Returns the name of the anonymous release identifier column e.g. "ReleaseId".
    /// </summary>
    /// <param name="runtimeName">
    ///     True to return just the name e.g. "ReleaseId".  False to return the fully qualified name e.g.
    ///     "[mydb]..[mytbl].[ReleaseId]"
    /// </param>
    /// <returns></returns>
    string GetReleaseIdentifier(bool runtimeName = false);

    /// <summary>
    ///     Returns boolean logic for WHERE Sql that will restrict records fetched from
    ///     <see cref="IExternalCohortTable.TableName" /> to only those in the current <see cref="IExtractableCohort" />
    ///     (by <see cref="OriginID" />).
    ///     <para>Sql returned does not include the prefix WHERE</para>
    /// </summary>
    /// <returns></returns>
    string WhereSQL();

    /// <summary>
    ///     Returns information stored in your cohort database (pointed to by <see cref="IExternalCohortTable" />) for the
    ///     cohort.  Fetches project number, version etc for this cohort.
    /// </summary>
    /// <param name="timeout">Number of seconds to allow query to execute for before giving up</param>
    IExternalCohortDefinitionData GetExternalData(int timeout = -1);

    /// <summary>
    ///     Returns the data type of the identifiable column (e.g. "varchar(10)")
    /// </summary>
    /// <returns></returns>
    string GetPrivateIdentifierDataType();

    /// <summary>
    ///     Returns the data type of the anonymous release identifier column (e.g. "varchar(300)")
    /// </summary>
    /// <returns></returns>
    string GetReleaseIdentifierDataType();

    /// <summary>
    ///     Returns an object for connecting to/interacting with the cohort database in which this cohort's identifiers are
    ///     stored.
    /// </summary>
    /// <returns></returns>
    DiscoveredDatabase GetDatabaseServer();

    /// <summary>
    ///     Looks in <paramref name="toProcess" /> for a column called <see cref="GetReleaseIdentifier" /> and swaps
    ///     identifiers
    ///     found in that column with the corresponding private identifier.  After replacing all the release identifiers
    ///     the column will be renamed <see cref="GetPrivateIdentifier" />
    /// </summary>
    /// <param name="toProcess"></param>
    /// <param name="listener"></param>
    /// <param name="allowCaching"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    void ReverseAnonymiseDataTable(DataTable toProcess, IDataLoadEventListener listener, bool allowCaching);
}