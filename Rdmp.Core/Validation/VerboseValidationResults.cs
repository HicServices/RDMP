// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Validation.Constraints;

namespace Rdmp.Core.Validation;

/// <summary>
/// Storage class for recording the number of rows failing validation with each Consequence subdivided by Column.
/// </summary>
public class VerboseValidationResults
{
    /// <summary>
    /// Dictionary of column names (Key), Value is a Dictionary of each of the potential consequences
    /// and a count of the number of cells that failed validation with that Consequence (for that Column - Key)
    /// 
    /// <para>e.g. DictionaryOfFailure["Forename"][Consequence.Missing] is a count of the number of cells which are missing
    /// (not there where they were expected) in column Forename</para>
    /// 
    /// </summary>
    public Dictionary<string, Dictionary<Consequence, int>> DictionaryOfFailure { get; private set; }

    /// <summary>
    /// Every time a row is Invalidated (this List gets the reason for Invalidation added to it)
    /// </summary>
    public List<string> ReasonsRowsInvalidated { get; set; }

    /// <summary>
    /// A count of the rows Invalidated due to dodgy data - failed Validations with Consequence.InvalidatesRow
    /// </summary>
    public int CountOfRowsInvalidated { get; set; }

    public VerboseValidationResults(ItemValidator[] validators)
    {
        CountOfRowsInvalidated = 0;
        ReasonsRowsInvalidated = new List<string>();
        DictionaryOfFailure = new Dictionary<string, Dictionary<Consequence, int>>();

        foreach (var iv in validators)
        {
            DictionaryOfFailure.Add(iv.TargetProperty, null);
            DictionaryOfFailure[iv.TargetProperty] = new Dictionary<Consequence, int>
            {
                { Consequence.Missing, 0 },
                { Consequence.Wrong, 0 },
                { Consequence.InvalidatesRow, 0 }
            };
        }
    }


    public Consequence ProcessException(ValidationFailure rootValidationFailure)
    {
        try
        {
            ConfirmIntegrityOfValidationException(rootValidationFailure);

            var worstConsequences = new Dictionary<ItemValidator, Consequence>();

            foreach (var subException in rootValidationFailure.GetExceptionList())
            {
                if (!subException.SourceConstraint.Consequence.HasValue)
                    throw new NullReferenceException(
                        $"ItemValidator of type {subException.SourceItemValidator.GetType().Name} on column {subException.SourceItemValidator.TargetProperty} has not had its Consequence configured");

                //we have encountered a rule that will invalidate the entire row, it's a good idea to keep a track of each of these since it would be rubbish to get a report out the other side that simply says 100% of rows invalid!
                if (subException.SourceConstraint.Consequence == Consequence.InvalidatesRow)
                    if (!ReasonsRowsInvalidated.Contains(
                            $"{subException.SourceItemValidator.TargetProperty}|{subException.SourceConstraint.GetType().Name}"))
                        ReasonsRowsInvalidated.Add(
                            $"{subException.SourceItemValidator.TargetProperty}|{subException.SourceConstraint.GetType().Name}");

                if (worstConsequences.TryGetValue(subException.SourceItemValidator, out var oldConsequence))
                {
                    //see if situation got worse
                    var newConsequence = subException.SourceConstraint.Consequence.Value;

                    if (newConsequence > oldConsequence)
                        worstConsequences[subException.SourceItemValidator] = newConsequence;
                }
                else
                {
                    //new validation error for this column
                    worstConsequences.Add(subException.SourceItemValidator,
                        (Consequence)subException.SourceConstraint.Consequence);
                }
            }

            //now record the worst case event
            if (worstConsequences.ContainsValue(Consequence.InvalidatesRow))
                CountOfRowsInvalidated++;

            foreach (var itemValidator in worstConsequences.Keys)
            {
                var columnName = itemValidator.TargetProperty;

                //increment the most damaging consequence count for this cell
                DictionaryOfFailure[columnName][worstConsequences[itemValidator]]++;
            }

            return worstConsequences.Max(key => key.Value);
        }
        catch (Exception e)
        {
            throw new Exception(
                $"An error occurred when trying to process a ValidationException into Verbose results:{e.Message}", e);
        }
    }

    private static void ConfirmIntegrityOfValidationException(ValidationFailure v)
    {
        if (v.GetExceptionList() == null)
            throw new NullReferenceException(
                $"Expected ValidationException to produce a List of broken validations, not just 1.  Validation message was:{v.Message}");

        foreach (var validationException in v.GetExceptionList())
        {
            if (validationException.SourceItemValidator?.TargetProperty == null)
                throw new NullReferenceException(
                    $"Column name referenced in ValidationException was null!, message in the exception was:{validationException.Message}");

            if (validationException.GetExceptionList() != null)
                throw new Exception(
                    $"Inception ValidationException detected! not only does the root Exception have a list of subexceptions (expected) but one of those has a sublist too! (unexpected).  This Exception message was:{validationException.Message}");
        }
    }
}