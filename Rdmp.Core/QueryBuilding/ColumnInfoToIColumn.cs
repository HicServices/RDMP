// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Allows you to convert a ColumnInfo into an IColumn (the column concept in query building).  IColumn has Alias and
///     Order which do not exist in ColumnInfo
///     (which is a reference to an existing column on your database only).  The alias will be null and the Order will be
///     -1 meaning that ColumnInfoToIColumn will
///     by default appear above other IColumns in order.
/// </summary>
public class ColumnInfoToIColumn : SpontaneousObject, IColumn
{
    private static Random r = new();

    /// <summary>
    ///     Allows the given <see cref="ColumnInfo" /> to act as an <see cref="IColumn" /> giving it an Order and setting
    ///     extraction flags (e.g. <see cref="HashOnDataRelease" />)to sensible defaults.
    /// </summary>
    /// <param name="repo"></param>
    /// <param name="column"></param>
    public ColumnInfoToIColumn(MemoryRepository repo, ColumnInfo column) : base(repo)
    {
        ColumnInfo = column;
        Order = -1;
        SelectSQL = column.GetFullyQualifiedName();
        Alias = null;
        HashOnDataRelease = false;
        IsExtractionIdentifier = false;
        IsPrimaryKey = false;
    }

    /// <inheritdoc />
    public string GetRuntimeName()
    {
        return ColumnInfo.GetRuntimeName();
    }

    /// <inheritdoc />
    public ColumnInfo ColumnInfo { get; }

    /// <inheritdoc />
    public int Order { get; set; }

    /// <inheritdoc />
    [Sql]
    public string SelectSQL { get; set; }

    /// <inheritdoc />
    public string Alias { get; set; }

    /// <inheritdoc />
    public bool HashOnDataRelease { get; }

    /// <inheritdoc />
    public bool IsExtractionIdentifier { get; }

    /// <inheritdoc />
    public bool IsPrimaryKey { get; }

    /// <summary>
    ///     Checks the syntax of the column
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        new ColumnSyntaxChecker(this).Check(notifier);
    }
}