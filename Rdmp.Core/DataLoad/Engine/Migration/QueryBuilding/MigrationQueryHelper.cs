// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;

/// <summary>
/// Generates the MERGE and UPDATE SQL queries responsible for migrating records from STAGING to LIVE as part of a data load.
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
        return $"WITH ToUpdate AS (SELECT stag.* FROM {ColumnsToMigrate.SourceTable.GetFullyQualifiedName()} AS stag LEFT OUTER JOIN {ColumnsToMigrate.DestinationTable.GetFullyQualifiedName()} AS prod {McsQueryHelper.BuildJoinClause("stag", "prod")} WHERE {McsQueryHelper.BuildPrimaryKeyNotNullTest("stag.")} AND {McsQueryHelper.BuildPrimaryKeyNotNullTest("prod.")} AND EXISTS (SELECT {McsQueryHelper.BuildSelectListForAllColumnsExceptStandard("stag.")} EXCEPT SELECT {McsQueryHelper.BuildSelectListForAllColumnsExceptStandard("prod.")})) {CreateUpdateClause()}";
    }

    public string CreateUpdateClause()
    {
        const string joinTableName = "ToUpdate";

        return
            $"UPDATE prod SET {BuildUpdateClauseForRow(joinTableName, "prod")} FROM {ColumnsToMigrate.DestinationTable.GetFullyQualifiedName()} AS prod INNER JOIN {joinTableName} {McsQueryHelper.BuildJoinClause(joinTableName, "prod")} SELECT @@ROWCOUNT";
    }


    public abstract string BuildUpdateClauseForRow(string sourceAlias, string destAlias);
    public abstract string BuildInsertClause();
}