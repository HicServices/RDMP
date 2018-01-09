using System;

namespace DataLoadEngine.Migration
{
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

        public virtual string CreateUpdateQuery(string sourceTable, string destTable)
        {
            var cte = String.Format(
                "WITH ToUpdate AS (SELECT stag.* FROM {0} AS stag LEFT OUTER JOIN {1} AS prod {2} WHERE {3} AND {4} AND EXISTS (SELECT {5} EXCEPT SELECT {6}))",
                sourceTable,
                destTable,
                McsQueryHelper.BuildJoinClause("stag", "prod"),
                McsQueryHelper.BuildPrimaryKeyNotNullTest("stag."),
                // if the joined stag.col is null, then this isn't an update
                McsQueryHelper.BuildPrimaryKeyNotNullTest("prod."),
                // if the joined prod.col is null, shouldn't see this as would have meant an insert
                McsQueryHelper.BuildSelectListForAllColumnsExceptStandard("stag."),
                McsQueryHelper.BuildSelectListForAllColumnsExceptStandard("prod."));

            return cte + " " + CreateUpdateClause(destTable);
        }

        public string CreateUpdateClause(string destTable, string joinTableName = "ToUpdate")
        {
            return string.Format("UPDATE prod SET {0} FROM {1} AS prod INNER JOIN {2} {3} SELECT @@ROWCOUNT",
                BuildUpdateClauseForRow(joinTableName, "prod"),
                destTable,
                joinTableName,
                McsQueryHelper.BuildJoinClause(joinTableName, "prod"));
        }

        public virtual string BuildMergeQuery(string sourceTable, string destinationTable)
        {
            return "MERGE " + destinationTable + " AS dest " +
                   "USING " + sourceTable + " " +
                   "AS source " + McsQueryHelper.BuildJoinClause() + " " +
                   "WHEN NOT MATCHED BY TARGET THEN " +
                   BuildInsertClause() + " " +
                   "OUTPUT $action, inserted.*;";
        }


        public abstract string BuildUpdateClauseForRow(string sourceAlias, string destAlias);
        public abstract string BuildInsertClause();
    }
}