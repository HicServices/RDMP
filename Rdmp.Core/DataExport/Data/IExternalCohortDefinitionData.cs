// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Information that is not held in RDMP about an ExtractableCohort but which must be fetched at runtime from the
///     cohort database (ExternalCohortTable)
///     <para>
///         Because RDMP is designed to support a wide range of cohort/release identifier allocation systems,  it takes a
///         very hands-off approach to cohort tables.
///         Things like Cohort Version, Description and even ProjectNumber are not imported into RDMP because they may be
///         part of an existing cohort management system
///         and thus cannot be moved (creating cached/synchronized copies would just be a further pain).
///     </para>
/// </summary>
public interface IExternalCohortDefinitionData
{
    /// <summary>
    ///     The <see cref="IProject.ProjectNumber" /> stored in the remote definition table for this cohort.  This restricts
    ///     which
    ///     <see cref="IProject" /> the cohort can be used with.
    /// </summary>
    int ExternalProjectNumber { get; set; }

    /// <summary>
    ///     The human readable description (name) of the cohort held in the remote definition table for this cohort.  This must
    ///     be the
    ///     same for all cohorts which are versions  of one another (See <see cref="ExternalVersion" />
    /// </summary>
    string ExternalDescription { get; set; }

    /// <summary>
    ///     The externally held version number for the cohort.  When combined with <see cref="ExternalDescription" /> allows
    ///     you to
    ///     identify whether there are newer versions of the cohort available.  All cohorts in the same 'version set' must have
    ///     the
    ///     same <see cref="ExternalDescription" />
    /// </summary>
    int ExternalVersion { get; set; }


    /// <summary>
    ///     The remote table from which the cohort information was fetched
    /// </summary>
    string ExternalCohortTableName { get; set; }

    /// <summary>
    ///     The date (if known) on which the cohort was created (in the remote cohort database)
    /// </summary>
    DateTime? ExternalCohortCreationDate { get; set; }
}