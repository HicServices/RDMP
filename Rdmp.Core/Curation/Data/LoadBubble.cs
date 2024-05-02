// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     A data load engine database stage, all tables being loaded go through each of these stages (RAW=>STAGING=>LIVE).
///     Archive is where redundant old replaced records are moved to on successful data loading
/// </summary>
public enum LoadBubble
{
    /// <summary>
    ///     The temporary unconstrained database created during a data load execution into which identifiable data is loaded
    ///     and data
    ///     integrity issues (null records, normalisation etc) occurs
    /// </summary>
    Raw,

    /// <summary>
    ///     The constrained database into which all records in a data load are written to before applying an UPSERT into the
    ///     live table of
    ///     new records.
    /// </summary>
    Staging,

    /// <summary>
    ///     The live database containing your clinical data
    /// </summary>
    Live,

    /// <summary>
    ///     The _Archive table in your live database into which historic records are moved when an UPDATE happens during data
    ///     load
    /// </summary>
    Archive
}