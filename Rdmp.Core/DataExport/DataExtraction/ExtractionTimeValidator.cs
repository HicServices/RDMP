// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Validation;

namespace Rdmp.Core.DataExport.DataExtraction;

/// <summary>
///     Applies Catalogue.ValidationXML to rows extracted during a Data Extraction Pipeline (See
///     ExecuteDatasetExtractionSource).  Because the columns which
///     are extracted can be a subset of the columns in the Catalogue and can include transforms the validation rules have
///     to be adjusted (some are not applied).
///     <para>
///         A count of the number of rows failing validation is stored in VerboseValidationResults (divided by column) and
///         is available for writing to the word
///         metadata document that accompanies the extracted records (See WordDataWriter).
///     </para>
///     <para>
///         This is similar to CatalogueConstraintReport (DQE) but is applied to a researchers extract instead of the
///         Catalogue as a whole.
///     </para>
/// </summary>
public class ExtractionTimeValidator
{
    private readonly List<IColumn> _columnsToExtract;

    private bool _initialized;

    public Validator Validator { get; set; }
    public VerboseValidationResults Results { get; set; }

    public List<ItemValidator> IgnoredBecauseColumnHashed { get; }

    public ExtractionTimeValidator(ICatalogue catalogue, List<IColumn> columnsToExtract)
    {
        _columnsToExtract = columnsToExtract;

        Validator = Validator.LoadFromXml(catalogue.ValidatorXML);

        if (string.IsNullOrWhiteSpace(catalogue.ValidatorXML))
            throw new ArgumentException($"No validations are configured for catalogue {catalogue.Name}");

        IgnoredBecauseColumnHashed = new List<ItemValidator>();
    }

    public void Validate(DataTable dt, string validationColumnToPopulateIfAny)
    {
        if (!_initialized)
            Initialize(dt);
        dt.BeginLoadData();
        foreach (DataRow r in dt.Rows)
        {
            //additive validation results, Results is a class that wraps DictionaryOfFailure which is an array of columns and each element is another array of consequences (with a row count for each consequence)
            //think of it like a 2D array with X columns and Y consquences and a number in each box which is how many values in that column failed validation with that consequence
            Results = Validator.ValidateVerboseAdditive(r, Results, out var consequenceOnLastRowProcessed);


            if (validationColumnToPopulateIfAny != null)
                r[validationColumnToPopulateIfAny] = consequenceOnLastRowProcessed;
        }

        dt.EndLoadData();
    }

    private void Initialize(DataTable dt)
    {
        var toDiscard = new List<ItemValidator>();

        //discard any item validators that don't exist in our colmn collection (from schema) - These are likely just columns that are not used during validation
        foreach (var iv in Validator.ItemValidators)
            if (!dt.Columns.Contains(iv.TargetProperty)) //if target property is not in the column collection
            {
                toDiscard.Add(iv);
            }
            else
            {
                //also discard any that have an underlying column that is Hashed as they will not match validation constraints post hash (hashing is done in SQL so we will never see original value)
                if (_columnsToExtract.Exists(c => c.ToString().Equals(iv.TargetProperty)))
                {
                    var ec = _columnsToExtract.First(c => c.ToString().Equals(iv.TargetProperty));
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

        foreach (var itemValidator in toDiscard)
            Validator.ItemValidators.Remove(itemValidator);

        _initialized = true;
    }
}