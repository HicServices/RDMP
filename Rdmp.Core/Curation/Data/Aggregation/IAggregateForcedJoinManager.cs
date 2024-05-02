// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data.Aggregation;

/// <summary>
///     Describes the requirement to include a given TableInfo in an AggregateConfiguration query even though the TableInfo
///     is not the owner of any of the Columns in the
///     query (the usual way of deciding which TableInfos to join).  This is needed if you want a count(*) for example in
///     which both header and result records tables are
///     joined together.
/// </summary>
public interface IAggregateForcedJoinManager
{
    /// <summary>
    ///     Returns all the TableInfos that the provided <see cref="AggregateConfiguration" /> has been explicitly requested
    ///     (by the user) to join to in its FROM section (See
    ///     <see cref="QueryBuilding.AggregateBuilder" />.
    ///     <para>
    ///         This set will be combined with those that would already be joined against because of the
    ///         <see cref="AggregateDimension" /> configured.  Note that your query results
    ///         in multiple TableInfos being needed then you will still need to have defined a way for the TableInfos to be
    ///         joined (See <see cref="JoinInfo" />.
    ///     </para>
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    ITableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration);

    /// <summary>
    ///     Deletes the mandate that the provided AggregateConfiguration should always join with the specified TableInfo
    ///     regardless of what <see cref="AggregateDimension" /> are
    ///     configured.  This will have no effect if there was no forced join declared in the first place.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="tableInfo"></param>
    void BreakLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo);

    /// <summary>
    ///     Creates the mandate that the provided AggregateConfiguration should always join with the specified TableInfo
    ///     regardless of what <see cref="AggregateDimension" /> are
    ///     configured (See <see cref="QueryBuilding.AggregateBuilder" />.
    ///     <para>
    ///         Note that your query results in multiple TableInfos being needed then you will still need to have defined a
    ///         way for the TableInfos to be joined (See <see cref="JoinInfo" />.
    ///     </para>
    /// </summary>
    /// <seealso cref="AggregateForcedJoin.GetAllForcedJoinsFor" />
    /// <param name="configuration"></param>
    /// <param name="tableInfo"></param>
    void CreateLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo);
}