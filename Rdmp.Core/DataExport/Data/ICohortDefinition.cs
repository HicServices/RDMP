// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     See CohortDefinition
/// </summary>
public interface ICohortDefinition
{
    /// <summary>
    ///     ID of the existing row in the <see cref="IExternalCohortTable.DefinitionTableName" /> or null if this object is
    ///     trying
    ///     to create a row that doesn't exist yet.  This ID will become <see cref="IExtractableCohort.OriginID" /> when a
    ///     reference
    ///     is created in RDMP to the row.
    /// </summary>
    int? ID { get; set; }

    /// <summary>
    ///     Value to store in or read from <see cref="IExternalCohortTable.DefinitionTableName" />
    /// </summary>
    string Description { get; set; }

    /// <summary>
    ///     Version number to store in or read from <see cref="IExternalCohortTable.DefinitionTableName" />
    /// </summary>
    int Version { get; set; }

    /// <summary>
    ///     Project number to store in or read from <see cref="IExternalCohortTable.DefinitionTableName" />.  This must match
    ///     any <see cref="IProject.ProjectNumber" /> that the cohort is to be used with.
    /// </summary>
    int ProjectNumber { get; set; }

    /// <summary>
    ///     Reference to the remote cohort database in which the row should be saved/read from.
    /// </summary>
    IExternalCohortTable LocationOfCohort { get; }

    /// <summary>
    ///     The cohort replaced if uploading a new version
    /// </summary>
    IExtractableCohort CohortReplacedIfAny { get; set; }

    /// <summary>
    ///     Returns true if the row described by this class would be novel in the destination database (See
    ///     <see cref="LocationOfCohort" />).
    ///     <para>
    ///         Returns false if the name/description/version look like the user is trying to upload an older version or
    ///         duplicate name etc
    ///     </para>
    /// </summary>
    /// <param name="matchDescription"></param>
    /// <returns></returns>
    bool IsAcceptableAsNewCohort(out string matchDescription);
}