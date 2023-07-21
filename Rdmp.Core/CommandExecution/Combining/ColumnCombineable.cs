// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
///     <see cref="ICombineToMakeCommand" /> for an object of type <see cref="IColumn" />
/// </summary>
public class ColumnCombineable : ICombineToMakeCommand
{
    /// <summary>
    ///     The column for combining (e.g. an <see cref="ExtractionInformation" />)
    /// </summary>
    public readonly IColumn Column;

    /// <summary>
    ///     Creates a new instance indicating that the <paramref name="column" /> has been selected for combining (e.g.
    ///     by starting a drag and drop operation).
    /// </summary>
    /// <param name="column"></param>
    public ColumnCombineable(IColumn column)
    {
        Column = column;
    }

    /// <inheritdoc />
    public string GetSqlString()
    {
        return Column.SelectSQL;
    }
}