// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     Spontaneous (memory only) version of AggregateTopX (a DatabaseEntity class).  See AggregateTopX for description.
/// </summary>
public class SpontaneouslyInventedAggregateTopX : SpontaneousObject, IAggregateTopX
{
    /// <inheritdoc />
    public int TopX { get; }

    /// <inheritdoc />
    public IColumn OrderByColumn { get; }

    /// <inheritdoc />
    public AggregateTopXOrderByDirection OrderByDirection { get; }


    /// <summary>
    ///     Creates a ne memory only TopX constraint for use with <see cref="QueryBuilding.AggregateBuilder" />.
    /// </summary>
    /// <param name="repo"></param>
    /// <param name="topX"></param>
    /// <param name="orderByDirection"></param>
    /// <param name="orderByColumn"></param>
    public SpontaneouslyInventedAggregateTopX(MemoryRepository repo, int topX,
        AggregateTopXOrderByDirection orderByDirection, IColumn orderByColumn) : base(repo)
    {
        TopX = topX;
        OrderByDirection = orderByDirection;
        OrderByColumn = orderByColumn;
    }
}