// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;

/// <summary>
///     Helper class for generating SQL fragments that relate to columns in a MigrationColumnSet.  This is used by
///     MigrationQueryHelper to generate SQL
///     for merging STAGING into LIVE during a data load.
/// </summary>
public class MigrationColumnSetQueryHelper
{
    private readonly MigrationColumnSet _migrationColumnSet;

    public MigrationColumnSetQueryHelper(MigrationColumnSet migrationColumnSet)
    {
        _migrationColumnSet = migrationColumnSet;
    }

    public string BuildSelectListForAllColumnsExceptStandard(string tableAlias = "")
    {
        var sql = _migrationColumnSet.FieldsToDiff
            .Where(col => !SpecialFieldNames.IsHicPrefixed(col) && !col.IsAutoIncrement).Aggregate("", (current, col) =>
                $"{current}{tableAlias}[{col.GetRuntimeName()}],");

        return sql.TrimEnd(',');
    }

    public string BuildPrimaryKeyNotNullTest(string columnPrefix)
    {
        return BuildPrimaryKeyCondition(columnPrefix, "NOT NULL");
    }

    public string BuildPrimaryKeyNullTest(string columnPrefix)
    {
        return BuildPrimaryKeyCondition(columnPrefix, "NULL");
    }

    private string BuildPrimaryKeyCondition(string columnPrefix, string condition)
    {
        // Allow either 'prefix' or 'prefix.' to be passed through
        if (!columnPrefix.EndsWith("."))
            columnPrefix += ".";

        return string.Join(" AND ", _migrationColumnSet.PrimaryKeys.Select(col =>
            $"{columnPrefix}[{col.GetRuntimeName()}] IS {condition}"));
    }

    public string BuildJoinClause(string sourceAlias = "source", string destAlias = "dest")
    {
        return !_migrationColumnSet.PrimaryKeys.Any()
            ? throw new InvalidOperationException(
                "None of the columns to be migrated are configured as a Primary Key, the JOIN clause for migration cannot be created. Please ensure that at least one of the columns in the MigrationColumnSet is configured as a Primary Key.")
            : $"ON ({string.Join(" AND ", _migrationColumnSet.PrimaryKeys.Select(pk => string.Format(sourceAlias + ".[{0}] = " + destAlias + ".[{0}]", pk.GetRuntimeName())))})";
    }
}