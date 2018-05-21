using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Triggers;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// Checks that LIVE has appropriate fields to support the migration of records from STAGING to LIVE and assigns fields roles such that artifact fields
    /// that are generated as part of the load (i.e. computed columns) denoted by the prefix hic_ are not treated as differences in the dataset.  This means
    /// that records in STAGING with a new hic_dataLoadRunID (all of them because each load gets a unique number) will not be identified as UPDATES to the 
    /// LIVE data table and will be ignored (assuming that there are no differences in other fields that are Diffed).
    /// </summary>
    public class StagingToLiveMigrationFieldProcessor : IMigrationFieldProcessor
    {
        public void ValidateFields(string[] sourceFields, string[] destinationFields)
        {
            if (!destinationFields.Any(f => f.Equals(SpecialFieldNames.DataLoadRunID, StringComparison.CurrentCultureIgnoreCase)))
                throw new MissingFieldException("Destination (Live) database table is missing field:" + SpecialFieldNames.DataLoadRunID);

            if (!destinationFields.Any(f=>f.Equals(SpecialFieldNames.ValidFrom,StringComparison.CurrentCultureIgnoreCase)))
                throw new MissingFieldException("Destination (Live) database table is missing field:" + SpecialFieldNames.ValidFrom);

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
