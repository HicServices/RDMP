using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// Helper class for generating SQL fragments that relate to columns in a MigrationColumnSet.  This is used by MigrationQueryHelper to generate SQL
    /// for merging STAGING into LIVE during a data load.
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
            string sql = "";
            const string ignorePrefix = "hic_";

            foreach (DiscoveredColumn col in _migrationColumnSet.FieldsToDiff)
            {
                //if it is hic_ or identity specification
                if(col.GetRuntimeName().StartsWith(ignorePrefix) || col.DataType.IsIdentity())
                    continue;

                sql += tableAlias + "[" + col.GetRuntimeName() + "],";
            }

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

        private string BuildPrimaryKeyCondition(string columnPrefix,string condition)
        {
            // Allow either 'prefix' or 'prefix.' to be passed through
            if (!columnPrefix.EndsWith("."))
                columnPrefix += ".";

            return String.Join(" AND ", _migrationColumnSet.PrimaryKeys.Select(col => columnPrefix + "[" + col.GetRuntimeName() + "] IS " + condition));
        }

        public string BuildJoinClause(string sourceAlias = "source", string destAlias = "dest")
        {
            if (!_migrationColumnSet.PrimaryKeys.Any())
                throw new InvalidOperationException("None of the columns to be migrated are configured as a Primary Key, the JOIN clause for migration cannot be created. Please ensure that at least one of the columns in the MigrationColumnSet is configured as a Primary Key.");

            return "ON (" + String.Join(" AND ", _migrationColumnSet.PrimaryKeys.Select(pk => String.Format(sourceAlias + ".[{0}] = " + destAlias + ".[{0}]", pk.GetRuntimeName()))) + ")";
        }
    }
}