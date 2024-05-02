// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;

/// <summary>
///     Generates the MERGE and UPDATE SQL queries responsible for migrating records from STAGING to LIVE as part of a data
///     load.
/// </summary>
public abstract class MigrationQueryHelper
{
    protected readonly MigrationColumnSet ColumnsToMigrate;
    protected readonly MigrationColumnSetQueryHelper McsQueryHelper;

    protected MigrationQueryHelper(MigrationColumnSet columnsToMigrate)
    {
        ColumnsToMigrate = columnsToMigrate;
        McsQueryHelper = new MigrationColumnSetQueryHelper(ColumnsToMigrate);
    }

    public virtual string CreateUpdateQuery()
    {
        var cte = string.Format(
            "WITH ToUpdate AS (SELECT stag.* FROM {0} AS stag LEFT OUTER JOIN {1} AS prod {2} WHERE {3} AND {4} AND EXISTS (SELECT {5} EXCEPT SELECT {6}))",
            ColumnsToMigrate.SourceTable.GetFullyQualifiedName(),
            ColumnsToMigrate.DestinationTable.GetFullyQualifiedName(),
            McsQueryHelper.BuildJoinClause("stag", "prod"),
            McsQueryHelper.BuildPrimaryKeyNotNullTest("stag."),
            // if the joined stag.col is null, then this isn't an update
            McsQueryHelper.BuildPrimaryKeyNotNullTest("prod."),
            // if the joined prod.col is null, shouldn't see this as would have meant an insert
            McsQueryHelper.BuildSelectListForAllColumnsExceptStandard("stag."),
            McsQueryHelper.BuildSelectListForAllColumnsExceptStandard("prod."));

        return $"{cte} {CreateUpdateClause()}";
    }

    public string CreateUpdateClause()
    {
        const string joinTableName = "ToUpdate";

        return string.Format("UPDATE prod SET {0} FROM {1} AS prod INNER JOIN {2} {3} SELECT @@ROWCOUNT",
            BuildUpdateClauseForRow(joinTableName, "prod"),
            ColumnsToMigrate.DestinationTable.GetFullyQualifiedName(),
            joinTableName,
            McsQueryHelper.BuildJoinClause(joinTableName, "prod"));
    }


    public abstract string BuildUpdateClauseForRow(string sourceAlias, string destAlias);
    public abstract string BuildInsertClause();
}