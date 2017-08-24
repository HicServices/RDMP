using System;
using System.Linq;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// Build 'migration' query when determining what fields to copy from live to staging during load backfill process.
    /// </summary>
    public class ReverseMigrationQueryHelper : MigrationQueryHelper
    {
        public ReverseMigrationQueryHelper(MigrationColumnSet columnsToMigrate) : base(columnsToMigrate)
        {
        }

        public override string BuildUpdateClauseForRow(string sourceAlias, string destAlias)
        {
            var parts = ColumnsToMigrate.FieldsToUpdate.Select(name => destAlias + ".[" + name + "] = " + sourceAlias + ".[" + name + "]").ToList();

            return String.Join(", ", parts);
        }

        public override string BuildMergeQuery(string sourceTable, string destinationTable)
        {
            throw new NotImplementedException("Do not attempt to perform a full merge with the ReverseMigrationQueryHelper.");
        }

        public override string BuildInsertClause()
        {
            throw new NotImplementedException("Do not attempt to insert data into staging from live, this query helper is purely for updating the destination.");
        }
    }
}