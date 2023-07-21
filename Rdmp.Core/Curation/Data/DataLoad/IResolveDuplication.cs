// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Naming;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Common interface for columns which can be used in RAW to resolve primary key duplication (See
///     PrimaryKeyCollisionResolver).  This includes both PreLoadDiscardedColumns
///     and ColumnInfos.
/// </summary>
public interface IResolveDuplication : IHasRuntimeName, ISaveable, IHasStageSpecificRuntimeName
{
    /// <summary>
    ///     The preference order for the non primary key column in deleting rows to obtain a unique primary key.  Records will
    ///     be evaluated on the first column in the order
    ///     and if records hold the same value the next column in the order will be used. Once a differing field value is found
    ///     one row will be selected based on
    ///     <see cref="DuplicateRecordResolutionIsAscending" />)
    /// </summary>
    int? DuplicateRecordResolutionOrder { get; set; }

    /// <summary>
    ///     True to prefer short strings, low numbers, nulls.  False to prefer longer strings, not null values, high numbers
    ///     when comparing which record to delete
    /// </summary>
    bool DuplicateRecordResolutionIsAscending { get; set; }

    /// <summary>
    ///     The proprietary SQL datatype of the column in the underlying database table this record points at.
    ///     <para>E.g. datetime2 or varchar2 (Oracle) or int etc</para>
    /// </summary>
    string Data_type { get; }
}