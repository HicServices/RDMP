using System.Collections.Generic;

namespace DataLoadEngine.Migration
{
    public interface IMigrationFieldProcessor
    {
        void ValidateFields(string[] sourceFields, string[] destinationFields);
        void AssignFieldsForProcessing(string field, List<string> fieldsToDiff, List<string> fieldsToUpdate);
    }
}