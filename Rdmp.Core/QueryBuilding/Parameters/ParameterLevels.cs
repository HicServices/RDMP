// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.QueryBuilding.Parameters;

/// <summary>
///     Describes the hierarchical level at which an ISqlParameter was found at by a ParameterManager.
///     <para>Do not reorder these!</para>
/// </summary>
public enum ParameterLevel
{
    /// <summary>
    ///     lowest, these are table valued function default values
    /// </summary>
    TableInfo,

    /// <summary>
    ///     higher these are explicitly declared properties at the query level e.g. filters, aggregation level (e.g. in the
    ///     WHERE statements of an AggregateConfiguration on extraction query )
    /// </summary>
    QueryLevel,

    /// <summary>
    ///     These are done when joining multiple queries together in an super query (usually separated with set operations such
    ///     as UNION, EXCEPT etc). See CohortQueryBuilder
    /// </summary>
    CompositeQueryLevel,

    /// <summary>
    ///     highest, these are added to the QueryBuilder by the code and should always be preserved, e.g. CohortID is
    ///     explicitly added by the data export manager.
    /// </summary>
    Global
}