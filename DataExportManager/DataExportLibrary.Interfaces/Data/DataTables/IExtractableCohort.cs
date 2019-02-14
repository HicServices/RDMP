// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractableCohort
    /// </summary>
    public interface IExtractableCohort :  IHasQuerySyntaxHelper, IMightBeDeprecated
    {
        /// <summary>
        /// Runs a count distinct on the release identifier column of the cohort.  Result is
        /// cached in memory for subsequent calls.
        /// </summary>
        int CountDistinct { get; }

        /// <summary>
        /// The location of the cohort database in which this cohort is held (outside RDMP metadata databases).
        /// </summary>
        int ExternalCohortTable_ID { get; }

        /// <summary>
        /// The id of the row in the <see cref="IExternalCohortTable.DefinitionTableName"/> that stores description
        /// of what is in this cohort (See <see cref="IExternalCohortDefinitionData"/>).
        /// 
        /// <para>Since you can have multiple cohort databases managed by RDMP it is possible for 2+ different <see cref="IExtractableCohort"/> 
        /// to have the same <see cref="OriginID"/> but be referencing different cohort lists.</para>
        /// </summary>
        int OriginID { get; }

        /// <summary>
        /// Allows you to override the release identifier column setting in <see cref="IExternalCohortTable"/> when extracting this specific
        /// cohort list.  Use this only if your mapping table has multiple different types of release identifier (stored in seperate columns).
        /// </summary>
        string OverrideReleaseIdentifierSQL { get; set; }

        /// <inheritdoc cref="ExternalCohortTable_ID"/>
        IExternalCohortTable ExternalCohortTable { get; }

        /// <summary>
        /// Fetches all mappings (private to release identifier) for the cohort (also returns any other columns in the <see cref="IExternalCohortTable.TableName"/>)
        /// </summary>
        /// <returns></returns>
        DataTable FetchEntireCohort();

        /// <summary>
        /// Returns the name of the identifiable private identifier column e.g. "chi".
        /// </summary>
        /// <param name="runtimeName">True to return just the name e.g. "chi".  False to return the fully qualified name e.g. "[mydb]..[mytbl].[chi]"</param>
        /// <returns></returns>
        string GetPrivateIdentifier(bool runtimeName = false);

        /// <summary>
        /// Returns the name of the anonymous release identifier column e.g. "ReleaseId".
        /// </summary>
        /// <param name="runtimeName">True to return just the name e.g. "ReleaseId".  False to return the fully qualified name e.g. "[mydb]..[mytbl].[ReleaseId]"</param>
        /// <returns></returns>
        string GetReleaseIdentifier(bool runtimeName = false);

        /// <summary>
        /// Returns boolean logic for WHERE Sql that will restrict records fetched from <see cref="IExternalCohortTable.TableName"/> to only those in the current <see cref="IExtractableCohort"/>
        /// (by <see cref="OriginID"/>).
        /// 
        /// <para>Sql returned does not include the prefix WHERE</para>
        /// </summary>
        /// <returns></returns>
        string WhereSQL();

        /// <summary>
        /// Returns information stored in your cohort database (pointed to by <see cref="IExternalCohortTable"/>) for the cohort.
        /// </summary>
        /// <returns></returns>
        IExternalCohortDefinitionData GetExternalData();
        
        /// <summary>
        /// Returns the data type of the identifiable column (e.g. "varchar(10)")
        /// </summary>
        /// <returns></returns>
        string GetPrivateIdentifierDataType();

        /// <summary>
        /// Returns the data type of the anonymous release identifier column (e.g. "varchar(300)")
        /// </summary>
        /// <returns></returns>
        string GetReleaseIdentifierDataType();

        DiscoveredDatabase GetDatabaseServer();

        /// <summary>
        /// Looks in <paramref name="toProcess"/> for a column called <see cref="GetReleaseIdentifier"/> and swaps identifiers
        /// found in that column with the corresponding private identifier.  After replacing all the release identifiers
        /// the column will be renamed <see cref="GetPrivateIdentifier"/>
        /// </summary>
        /// <param name="toProcess"></param>
        /// <param name="listener"></param>
        /// <param name="allowCaching"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        void ReverseAnonymiseDataTable(DataTable toProcess, IDataLoadEventListener listener, bool allowCaching);
        
    }
}