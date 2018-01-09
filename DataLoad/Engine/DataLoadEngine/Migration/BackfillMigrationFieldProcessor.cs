using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Triggers;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// IMigrationFieldProcessor for StagingBackfillMutilator (See StagingBackfillMutilator).
    /// </summary>
    public class BackfillMigrationFieldProcessor : IMigrationFieldProcessor
    {
        public void ValidateFields(string[] sourceFields, string[] destinationFields)
        {
            if (!sourceFields.Contains(SpecialFieldNames.DataLoadRunID))
                throw new MissingFieldException(SpecialFieldNames.DataLoadRunID);

            if (!sourceFields.Contains(SpecialFieldNames.ValidFrom))
                throw new MissingFieldException(SpecialFieldNames.ValidFrom);
        }

        public void AssignFieldsForProcessing(string field, List<string> fieldsToDiff, List<string> fieldsToUpdate)
        {
            // todo: CHECK should the if clause be removed here, would we want to update a hic_ field in staging? Why would we not want to do that?
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