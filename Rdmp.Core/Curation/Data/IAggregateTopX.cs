// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     See AggregateTopX
/// </summary>
public interface IAggregateTopX : IMapsDirectlyToDatabaseTable
{
    /// <summary>
    ///     The number of records to return from the TopX e.g. Top 10
    /// </summary>
    int TopX { get; }

    /// <summary>
    ///     The dimension which the top X applies to, if null it will be the count / sum etc column (The AggregateCountColumn)
    /// </summary>
    IColumn OrderByColumn { get; }

    /// <summary>
    ///     When applying a TopX to an aggregate, this is the direction (Ascending/Descending) for the ORDER BY statement.
    ///     Descending means pick top X where
    ///     count / sum etc is highest.
    /// </summary>
    AggregateTopXOrderByDirection OrderByDirection { get; }
}