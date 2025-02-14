// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Describes how to join two tables together.  This is used to during Query Building (See JoinHelper) to build the JOIN section of the query once all required tables
/// have been identified (See SqlQueryBuilderHelper).
/// </summary>
public interface IJoin
{
    /// <summary>
    /// The column in the secondary table that should be joined with the <see cref="PrimaryKey"/> column
    /// </summary>
    ColumnInfo ForeignKey { get; }

    /// <summary>
    /// The column in the main table which should be joined
    /// </summary>
    ColumnInfo PrimaryKey { get; }

    /// <summary>
    /// The collation type to apply to the join if <see cref="ForeignKey"/> and <see cref="PrimaryKey"/> have different column collations.  If there are <see cref="ISupplementalJoin"/>
    /// then they must match on <see cref="Collation"/>
    /// 
    /// <para>Only set this if you are sure you have a collation problem</para>
    /// </summary>
    string Collation { get; }

    /// <summary>
    /// Which SQL join keyword to use when linking the <see cref="PrimaryKey"/> and <see cref="ForeignKey"/>.
    /// </summary>
    ExtractionJoinType ExtractionJoinType { get; }

    /// <summary>
    /// If it is necessary to join on more than one column, use this method to indicate the additional fk / pk pairs (they must belong to the same TableInfos as the
    /// main IJoin)
    /// </summary>
    /// <returns></returns>
    IEnumerable<ISupplementalJoin> GetSupplementalJoins();

    /// <summary>
    /// The ExtractionJoinType Property models Left/Right/Inner when the SqlQueryBuilderHelper finds the PrimaryKey TableInfo and needs to join to the ForeignKey table
    /// (the normal situation). However if the ForeignKey TableInfo is required first (either because it is IsPrimaryExtractionTable or because there are other tables
    /// in the query that force a particular join order) then the Join direction needs to be inverted.  Normally this is a matter of swapping Left=>Right and vice versa
    /// but you might instead want to throw NotSupportedException if you are expecting a specific direction (See Lookup)
    /// </summary>
    /// <returns></returns>
    ExtractionJoinType GetInvertedJoinType();

    /// <summary>
    /// If you want to override the 'ON SQL' when using this join return the custom SQL here.  Normally SQL will be something like "table1.A = table2.A".  Use this method
    /// to turn it into e.g. "table1.A = table2.A OR (table1.A is null AND table2.A is null)"
    /// </summary>
    /// <returns></returns>
    string GetCustomJoinSql();
}