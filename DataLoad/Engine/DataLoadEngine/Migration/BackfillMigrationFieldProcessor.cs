using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Triggers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// IMigrationFieldProcessor for StagingBackfillMutilator (See StagingBackfillMutilator).
    /// </summary>
    public class BackfillMigrationFieldProcessor : IMigrationFieldProcessor
    {
        public void ValidateFields(DiscoveredColumn[] sourceFields, DiscoveredColumn[] destinationFields)
        {
            if (!sourceFields.Any(c=>c.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID,StringComparison.CurrentCultureIgnoreCase)))
                throw new MissingFieldException(SpecialFieldNames.DataLoadRunID);


            if (!sourceFields.Any(c => c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom, StringComparison.CurrentCultureIgnoreCase)))
                throw new MissingFieldException(SpecialFieldNames.ValidFrom);
        }

        public void AssignFieldsForProcessing(DiscoveredColumn field, List<DiscoveredColumn> fieldsToDiff, List<DiscoveredColumn> fieldsToUpdate)
        {
            //it is a hic internal field but not one of the overwritten, standard ones
            if (field.GetRuntimeName().StartsWith("hic_"))
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