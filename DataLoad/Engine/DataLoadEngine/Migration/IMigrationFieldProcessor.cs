using System.Collections.Generic;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// Handles the routing of columns in a MigrationColumnSet to either FieldsToDiff or FieldsToUpdate during a MERGE query in which records are written into
    /// LIVE from STAGING.  Note that this interface is also usable to describe a reverse flow of records in which records in STAGING are modified depending
    /// on records/fields in LIVE (See StagingBackfillMutilator).
    /// </summary>
    public interface IMigrationFieldProcessor
    {
        void ValidateFields(string[] sourceFields, string[] destinationFields);

        /// <summary>
        /// Assigns the current field to either Diff and/or Update (or neither).
        /// </summary>
        /// <param name="field">the field to assign to one/none/both lists</param>
        /// <param name="fieldsToDiff">Fields that will have their values compared for change, to decide whether to overwrite destination data with source data.
        /// (some fields might not matter if they are different e.g. dataLoadRunID)</param>
        /// <param name="fieldsToUpdate">Fields that will have their values copied across to the new table (this is usually a superset of fields to diff, and also
        /// includes all primary keys).</param>
        void AssignFieldsForProcessing(string field, List<string> fieldsToDiff, List<string> fieldsToUpdate);
    }
}