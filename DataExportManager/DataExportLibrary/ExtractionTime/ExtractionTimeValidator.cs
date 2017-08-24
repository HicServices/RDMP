using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;

namespace DataExportLibrary.ExtractionTime
{
    public class ExtractionTimeValidator
    {
        private readonly ICatalogue _catalogue;
        private readonly List<IColumn> _columnsToExtract;
        
        private bool _initialized = false;
  


        public Validator Validator { get; set; }
        public VerboseValidationResults Results { get; set; }

        public List<ItemValidator> IgnoredBecauseColumnHashed { get; private set; }

        public ExtractionTimeValidator(ICatalogue catalogue, List<IColumn> columnsToExtract)
        {
            _catalogue = catalogue;
            _columnsToExtract = columnsToExtract;
            
            Validator = Validator.LoadFromXml(_catalogue.ValidatorXML);

            if (string.IsNullOrWhiteSpace(_catalogue.ValidatorXML))
                throw new ArgumentException("No validations are configured for catalogue " + catalogue.Name);

            IgnoredBecauseColumnHashed = new List<ItemValidator>();
        }

        public void Validate(DataTable dt,string validationColumnToPopulateIfAny)
        {
            if (!_initialized)
                Initialize(dt);
            Consequence? consequenceOnLastRowProcessed;

            foreach (DataRow r in dt.Rows)
            {
                //additive validation results, Results is a class that wraps DictionaryOfFailure which is an array of columns and each element is another array of consequences (with a row count for each consequence)
                //think of it like a 2D array with X columns and Y consquences and a number in each box which is how many values in that column failed validation with that consequence
                Results = Validator.ValidateVerboseAdditive(r, Results, out consequenceOnLastRowProcessed);


                if (validationColumnToPopulateIfAny != null)
                    r[validationColumnToPopulateIfAny] = consequenceOnLastRowProcessed;
            }

        }

        private void Initialize(DataTable dt)
        {
            List<ItemValidator> toDiscard = new List<ItemValidator>();

            //discard any item validators that don't exist in our colmn collection (from schema) - These are likely just columns that are not used during validation
            foreach (ItemValidator iv in Validator.ItemValidators)
                if (!dt.Columns.Contains(iv.TargetProperty))  //if target property is not in the column collection
                    toDiscard.Add(iv);
                else
                {
                    //also discard any that have an underlying column that is Hashed as they will not match validation constraints post hash (hashing is done in SQL so we will never see original value)
                    if (_columnsToExtract.Exists(c => c.ToString().Equals(iv.TargetProperty)))
                    {
                        IColumn ec = _columnsToExtract.First(c => c.ToString().Equals(iv.TargetProperty));
                        if (ec.HashOnDataRelease)
                        {
                            IgnoredBecauseColumnHashed.Add(iv);
                            toDiscard.Add(iv);
                        }
                    }
                    else //also discard any CHI validations as the CHI column will be swapped for a PROCHI
                        if (iv.TargetProperty.Equals("CHI", StringComparison.InvariantCultureIgnoreCase))
                        {
                            IgnoredBecauseColumnHashed.Add(iv);
                            toDiscard.Add(iv);
                        }

                }

            foreach (ItemValidator itemValidator in toDiscard)
                Validator.ItemValidators.Remove(itemValidator);

            _initialized = true;

        }
    }
}

