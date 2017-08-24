using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Triggers;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Migration
{
    public class MigrationColumnSet
    {
        public const string DataLoadRunField = SpecialFieldNames.DataLoadRunID;
        public const string ValidFromField = SpecialFieldNames.ValidFrom;

        public string SourceTableName { get; set; }
        public string DestinationTableName { get; set; }

        public List<string> PrimaryKeys { get; set; }

        public static List<string> GetStandardColumnNames()
        {
            return new List<string> { DataLoadRunField, ValidFromField };
        }

        /// <summary>
        /// Fields that will have their values compared for change, to decide whether to overwrite destination data with source data. (some fields might not matter if they are different e.g. dataLoadRunID)
        /// </summary>
        public List<string> FieldsToDiff { get; set; }

        /// <summary>
        /// Fields that will have their values copied across to the new table (this is a superset of fields to diff, and also includes all primary keys).  Note that the non-standard columns (data load run and valid from do not appear in this list, you are intended to handle their update yourself)
        /// </summary>
        public List<string> FieldsToUpdate { get; set; }

        public string DestinationDatabase { get; set; }

        public MigrationColumnSet(string destinationDatabase, string sourceTableName, string destinationTableName, string[] sourceFields, string[] destinationFields, IEnumerable<IColumnMetadata> columns, IMigrationFieldProcessor migrationFieldProcessor)
        {
            migrationFieldProcessor.ValidateFields(sourceFields, destinationFields);

            DestinationDatabase = destinationDatabase;
            SourceTableName = sourceTableName;
            DestinationTableName = destinationTableName;

            PrimaryKeys = new List<string>();
            FieldsToDiff = new List<string>();
            FieldsToUpdate = new List<string>();

            ExtractPrimaryKeys(sourceFields, destinationFields, columns);

            //figure out things to migrate and whether they matter to diffing
            foreach (string field in sourceFields)
            {
                if (field.Equals(DataLoadRunField) || field.Equals(ValidFromField))
                    continue;

                if (!destinationFields.Contains(field))
                    throw new MissingFieldException("Field " + field + " is missing from destination table");

                migrationFieldProcessor.AssignFieldsForProcessing(field, FieldsToDiff, FieldsToUpdate);
            }
        }

        private void ExtractPrimaryKeys(string[] sourceFields, string[] destinationFields, IEnumerable<IColumnMetadata> columns)
        {
            // figure out all the primary keys
            foreach (IColumnMetadata col in columns)
            {
                string colName = col.GetRuntimeName();
                //found something
                if (col.IsPrimaryKey)
                    if (!destinationFields.Contains(colName) || !sourceFields.Contains(colName))
                        throw new MissingFieldException("Column " + colName +
                                                        " is missing from either the source or the destination table");
                    else
                        PrimaryKeys.Add(col.GetRuntimeName());
            }
        }
    }
}