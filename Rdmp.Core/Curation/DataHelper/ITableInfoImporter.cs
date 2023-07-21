// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Curation.DataHelper;

/// <summary>
///     Shared interface for the two classes which create TableInfos from tables (namely TableInfoImporter and
///     TableValuedFunctionImporter).  A TableInfo the RDMP class that
///     documents a persistent reference to a querable table (See TableInfo).
/// </summary>
public interface ITableInfoImporter
{
    /// <summary>
    ///     Creates references to all columns/tables found on the live database in the RDMP persistence database.
    /// </summary>
    /// <param name="tableInfoCreated"></param>
    /// <param name="columnInfosCreated"></param>
    void DoImport(out ITableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated);

    /// <summary>
    ///     For when a <paramref name="discoveredColumn" /> is not currently documented by an existing
    ///     <see cref="ColumnInfo" />
    ///     in the <paramref name="parent" />.  This method creates one.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="discoveredColumn"></param>
    /// <returns></returns>
    ColumnInfo CreateNewColumnInfo(ITableInfo parent, DiscoveredColumn discoveredColumn);
}