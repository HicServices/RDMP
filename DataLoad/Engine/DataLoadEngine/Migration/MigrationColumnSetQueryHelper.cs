using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLoadEngine.Migration
{
    public class MigrationColumnSetQueryHelper
    {
        private readonly MigrationColumnSet _migrationColumnSet;

        public MigrationColumnSetQueryHelper(MigrationColumnSet migrationColumnSet)
        {
            _migrationColumnSet = migrationColumnSet;
        }

        public string BuildSelectListForAllColumnsExceptStandard(string tableAlias = "")
        {
            return String.Join(", ", GetAllColumnNamesExceptStandard(_migrationColumnSet.FieldsToDiff).Select(name => tableAlias + "[" + name + "]"));
        }

        public string BuildPrimaryKeyNotNullTest(string columnPrefix)
        {
            var parts = new List<string>();

            // Allow either 'prefix' or 'prefix.' to be passed through
            if (!columnPrefix.EndsWith("."))
                columnPrefix += ".";

            _migrationColumnSet.PrimaryKeys.ForEach(name => parts.Add(columnPrefix + "[" + name + "] IS NOT NULL"));
            return String.Join(" AND ", parts);
        }

        public string BuildPrimaryKeyNullTest(string columnPrefix)
        {
            var parts = new List<string>();

            // Allow either 'prefix' or 'prefix.' to be passed through
            if (!columnPrefix.EndsWith("."))
                columnPrefix += ".";

            _migrationColumnSet.PrimaryKeys.ForEach(name => parts.Add(columnPrefix + "[" + name + "] IS NULL"));
            return String.Join(" AND ", parts);
        }

        public string BuildJoinClause(string sourceAlias = "source", string destAlias = "dest")
        {
            if (!_migrationColumnSet.PrimaryKeys.Any())
                throw new InvalidOperationException("None of the columns to be migrated are configured as a Primary Key, the JOIN clause for migration cannot be created. Please ensure that at least one of the columns in the MigrationColumnSet is configured as a Primary Key.");

            var parts = new List<string>();
            _migrationColumnSet.PrimaryKeys.ForEach(pk => parts.Add(String.Format(sourceAlias + ".[{0}] = " + destAlias + ".[{0}]", pk)));
            return "ON (" + String.Join(" AND ", parts) + ")";
        }


        private IEnumerable<string> GetAllColumnNamesExceptStandard(IEnumerable<string> columnNames)
        {
            const string ignorePrefix = "hic_";
            var standardColumns = new List<string> { "Id" }; // TODO: Need to sort out Id. It isn't standard but is a problem artificial key in BC lookups
            return columnNames.Where(name =>
                !(name.StartsWith(ignorePrefix)
                  ||
                  standardColumns.Contains(name))
                );
        }

    }
}