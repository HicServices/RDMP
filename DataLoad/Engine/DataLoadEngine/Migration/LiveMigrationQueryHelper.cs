using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Triggers;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// See MigrationQueryHelper
    /// </summary>
    public class LiveMigrationQueryHelper : MigrationQueryHelper
    {
        private readonly int _dataLoadRunID;

        public LiveMigrationQueryHelper(MigrationColumnSet columnsToMigrate, int dataLoadRunID) : base (columnsToMigrate)
        {
            _dataLoadRunID = dataLoadRunID;
        }

        public override string BuildUpdateClauseForRow(string sourceAlias, string destAlias)
        {
            var parts = ColumnsToMigrate.FieldsToUpdate.Select(name => destAlias + ".[" + name + "] = " + sourceAlias + ".[" + name + "]").ToList();
            parts.Add(destAlias + "." + SpecialFieldNames.DataLoadRunID + " = " + _dataLoadRunID);

            return String.Join(", ", parts);
        }

        public override string BuildInsertClause()
        {
            var inserts = GetListOfInsertColumnFields(ColumnsToMigrate, _dataLoadRunID);
            
            return String.Format("INSERT ({0}) VALUES ({1})", 
                String.Join(", ", inserts.Select(pair => pair.Key)),
                String.Join(", ", inserts.Select(pair => pair.Value)));
        }

        public List<KeyValuePair<string, string>> GetListOfInsertColumnFields(MigrationColumnSet columnsToMigrate, int dataLoadRunID)
        {
            var inserts = new List<KeyValuePair<string, string>>();

            columnsToMigrate.FieldsToUpdate.ToList().ForEach(column =>
                inserts.Add(new KeyValuePair<string, string>("[" + column + "]", "source.[" + column + "]")));

            inserts.Add(new KeyValuePair<string, string>(SpecialFieldNames.DataLoadRunID, dataLoadRunID.ToString()));

            return inserts;
        }
    }
}