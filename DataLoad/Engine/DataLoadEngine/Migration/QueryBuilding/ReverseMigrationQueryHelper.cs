using System;
using System.Linq;

namespace DataLoadEngine.Migration.QueryBuilding
{
    /// <summary>
    /// Reverse  of LiveMigrationQueryHelper.  Builds 'migration' query for updating STAGING to match LIVE when doing a backfill data load (See 
    /// StagingBackfillMutilator).
    /// </summary>
    public class ReverseMigrationQueryHelper : MigrationQueryHelper
    {
        public ReverseMigrationQueryHelper(MigrationColumnSet columnsToMigrate) : base(columnsToMigrate)
        {
        }

        public override string BuildUpdateClauseForRow(string sourceAlias, string destAlias)
        {
            return String.Join(", ", ColumnsToMigrate.FieldsToUpdate.Select(col => 
                string.Format(destAlias + ".[" + col + "] = " + sourceAlias + ".[" + col + "]",col.GetRuntimeName())));
        }

        public override string BuildMergeQuery()
        {
            throw new NotImplementedException("Do not attempt to perform a full merge with the ReverseMigrationQueryHelper.");
        }

        public override string BuildInsertClause()
        {
            throw new NotImplementedException("Do not attempt to insert data into staging from live, this query helper is purely for updating the destination.");
        }
    }
}