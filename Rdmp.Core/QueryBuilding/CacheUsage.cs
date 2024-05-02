// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CohortCreation.Execution;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Describes the usability of a cohort query cache when building a cohort in a <see cref="CohortCompiler" />.  This
///     depends primarily on whether
///     the cache is on the same server as the other datasets being built
/// </summary>
public enum CacheUsage
{
    /// <summary>
    ///     The cache must be used and all Dependencies must be cached.  This happens if dependencies are on different servers
    ///     / data access
    ///     credentials.  Or the query being built involves SET operations which are not supported by the DBMS of the
    ///     dependencies (e.g. MySql UNION / INTERSECT etc).
    /// </summary>
    MustUse,

    /// <summary>
    ///     All dependencies are on the same server as the cache.  Therefore we can mix and match where we fetch tables from
    ///     (live table or cache) depending on whether the cache contains an entry for it or not.
    /// </summary>
    Opportunistic,

    /// <summary>
    ///     All dependencies are on the same server but the cache is on a different server.  Therefore we can either
    ///     run a fully cached set of queries or we cannot run any cached queries
    /// </summary>
    AllOrNothing
}