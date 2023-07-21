// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.Curation.Data.Cohort;

/// <summary>
///     The Sql set operation for combining sets (lists of patient identifiers) in a
///     <see cref="CohortAggregateContainer" />.  This is done by compiling each
///     <see cref="AggregateConfiguration" /> and <see cref="CohortAggregateContainer" /> in a given container into queries
///     identifying distinct patients.  A master
///     query is then built in which each subquery is interspersed by the appropriate <see cref="SetOperation" />.
/// </summary>
public enum SetOperation
{
    /// <summary>
    ///     Result set is everyone in any sub set
    /// </summary>
    UNION,

    /// <summary>
    ///     Result set is only people appearing in every subset
    /// </summary>
    INTERSECT,

    /// <summary>
    ///     Result set is the people in the first set who do not appear in any of the subsequent sets
    /// </summary>
    EXCEPT
}