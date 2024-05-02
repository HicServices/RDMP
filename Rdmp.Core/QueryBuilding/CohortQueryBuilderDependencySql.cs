// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.QueryBuilding.Parameters;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Describes a block of sql built using a <see cref="AggregateBuilder" /> which may need to be slotted into a larger
///     query e.g.
///     as a subsection of a large <see cref="CohortIdentificationConfiguration" />.  The <see cref="Sql" /> is built
///     without considering
///     parameters that exist in any larger query.  Use method <see cref="Use" /> to deploy the <see cref="Sql" /> into a
///     larger query
///     or use it on its own for cache hit checking / running in isolation.
/// </summary>
public class CohortQueryBuilderDependencySql
{
    public string Sql { get; private set; }

    /// <summary>
    ///     New parameters needed by the <see cref="Sql" />
    /// </summary>
    public ParameterManager ParametersUsed { get; }

    private bool _hasBeenUsed;

    public CohortQueryBuilderDependencySql(string sql, ParameterManager parameterManager)
    {
        Sql = sql;
        ParametersUsed = parameterManager;
    }

    /// <summary>
    ///     Can only be called once, updates the <see cref="Sql" /> to resolve parameter naming collisions with the larger
    ///     query
    ///     (see <paramref name="compositeLevelParameterManager" />.  Returns the updated <see cref="Sql" /> (for convenience)
    /// </summary>
    /// <param name="compositeLevelParameterManager">
    ///     <see cref="ParameterManager" /> that reflects this aggregates location within a larger composite query (e.g. with
    ///     UNION / INTERSECT / EXCEPT)
    ///     containers.  This manager may include non global parameters that have been used to satisfy previously written SQL
    ///     and new parameters
    ///     discovered may be subject to rename operations (which can break caching).
    /// </param>
    /// <returns></returns>
    public string Use(ParameterManager compositeLevelParameterManager)
    {
        if (_hasBeenUsed)
            throw new InvalidOperationException("Block can only be used once");

        //import parameters unless caching was used
        compositeLevelParameterManager.ImportAndElevateResolvedParametersFromSubquery(ParametersUsed,
            out var renameOperations);

        //rename in the SQL too!
        foreach (var kvp in renameOperations)
            Sql = ParameterCreator.RenameParameterInSQL(Sql, kvp.Key, kvp.Value);

        _hasBeenUsed = true;

        return Sql;
    }
}