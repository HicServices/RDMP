// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
///     Describes the role of a <see cref="Rdmp.Core.Curation.Data.TableInfo" /> or
///     <see cref="Rdmp.Core.Curation.Data.ColumnInfo" /> in
///     a <see cref="Rdmp.Core.Curation.Data.JoinInfo" />
/// </summary>
public enum JoinInfoType
{
    /// <summary>
    ///     Either <see cref="ForeignKey" /> or <see cref="PrimaryKey" /> (only valid for querying)
    /// </summary>
    AnyKey,

    /// <summary>
    ///     The child table which contains a field referencing the primary key of the parent table
    /// </summary>
    ForeignKey,

    /// <summary>
    ///     The parent table which has a primary key
    /// </summary>
    PrimaryKey
}