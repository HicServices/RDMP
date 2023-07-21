// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <inheritdoc cref="PreLoadDiscardedColumn" />
public interface IPreLoadDiscardedColumn : IResolveDuplication, IMapsDirectlyToDatabaseTable
{
    /// <summary>
    ///     Where the RAW column values will end up during a load.  Either dropped completely, diluted into LIVE or routed to
    ///     an identifier dump
    /// </summary>
    DiscardedColumnDestination Destination { get; set; }

    /// <summary>
    ///     The name of the virtual column (that will exist only in RAW).
    /// </summary>
    string RuntimeColumnName { get; set; }

    /// <summary>
    ///     The type of the virtual column when creating it in RAW during a data load
    /// </summary>
    string SqlDataType { get; set; }

    #region Relationships

    /// <summary>
    ///     The table the virtual column is associated with.  When creating RAW during a DLE execution, all
    ///     <see cref="IPreLoadDiscardedColumn" /> will be created in addition
    ///     to the normal LIVE columns in the <see cref="TableInfo" /> live schema.
    /// </summary>
    [NoMappingToDatabase]
    ITableInfo TableInfo { get; }

    #endregion
}