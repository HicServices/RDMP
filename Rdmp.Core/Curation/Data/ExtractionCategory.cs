// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Determines how accessible a given ExtractionInformation should be.
/// </summary>
public enum ExtractionCategory
{
    /// <summary>
    ///     This column is always available for extraction
    /// </summary>
    Core,

    /// <summary>
    ///     This column is available but might not always be wanted e.g. lookup descriptions where there is already a lookup
    ///     code
    /// </summary>
    Supplemental,

    /// <summary>
    ///     This column is only available to researchers who have additional approvals over and above those required for a
    ///     basic data extract
    /// </summary>
    SpecialApprovalRequired,

    /// <summary>
    ///     This column is for internal use only and shouldn't be released to researchers during data extraction
    /// </summary>
    Internal,

    /// <summary>
    ///     This column used to be supplied to researchers but should no longer be provided
    /// </summary>
    Deprecated,

    /// <summary>
    ///     This column is part of a 'Project Specific Catalogue'.  If a <see cref="Catalogue" /> is for use only with a
    ///     specific data export Project then all
    ///     <see cref="ExtractionInformation" /> in that <see cref="Catalogue" /> must have this
    ///     <see cref="ExtractionCategory" />
    /// </summary>
    ProjectSpecific,

    /// <summary>
    ///     Value can only be used for fetching ExtractionInformations.  This means that all will be returned.  You cannot set
    ///     a column to have an ExtractionCategory of Any
    /// </summary>
    Any,

    /// <summary>
    /// Value used for improved UI experience, will be set to null when executed
    /// </summary>
    NotExtractable
}