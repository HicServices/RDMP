using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Triggers;

namespace DataLoadEngine.Migration
{
    public class StagingToLiveMigrationFieldProcessor : IMigrationFieldProcessor
    {
        public const string DataLoadRunField = SpecialFieldNames.DataLoadRunID;
        public const string ValidFromField = SpecialFieldNames.ValidFrom;

        public void ValidateFields(string[] sourceFields, string[] destinationFields)
        {
            if (!destinationFields.Contains(DataLoadRunField))
                throw new MissingFieldException("Destination (Live) database table is missing field:" + DataLoadRunField);

            if (!destinationFields.Contains(ValidFromField))
                throw new MissingFieldException("Destination (Live) database table is missing field:" + ValidFromField);

        }

        public void AssignFieldsForProcessing(string field, List<string> fieldsToDiff, List<string> fieldsToUpdate)
        {
            //it is a hic internal field but not one of the overwritten, standard ones
            if (field.StartsWith("hic_"))
                fieldsToUpdate.Add(field);
            else
            {
                //it is not a hic internal field
                fieldsToDiff.Add(field);
                fieldsToUpdate.Add(field);
            }
        }
    }
}
