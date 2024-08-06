// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;

namespace Rdmp.Core.DataQualityEngine.Reports;

/// <summary>
/// Records the total number of validation failures that occur for each column.  Results are calculated for each novel DataLoadRunId found.  The counts
/// for a column will always add up to the row count even if there are multiple validation rules broken (The worst consequence only is counted).
/// </summary>
public class DQEStateOverDataLoadRunId
{
    private readonly string _pivotCategory;

    //column level results - validation failures
    public Dictionary<int, VerboseValidationResults> ColumnValidationFailuresByDataLoadRunID { get; set; }

    //column level results - everything including unconstrained columns and columns which never had any validation failures at all
    public Dictionary<int, ColumnState[]> AllColumnStates { get; set; }

    public Dictionary<int, Dictionary<Consequence, int>> WorstConsequencesByDataLoadRunID { get; set; }
    public Dictionary<int, int> RowsPassingValidationByDataLoadRunID { get; set; }

    public DQEStateOverDataLoadRunId(string pivotCategory)
    {
        _pivotCategory = pivotCategory;
        InitializeDictionaries();
    }

    public void InitializeDictionaries()
    {
        //column level
        ColumnValidationFailuresByDataLoadRunID = new Dictionary<int, VerboseValidationResults>();
        AllColumnStates = new Dictionary<int, ColumnState[]>();

        //row level
        WorstConsequencesByDataLoadRunID = new Dictionary<int, Dictionary<Consequence, int>>();
        RowsPassingValidationByDataLoadRunID = new Dictionary<int, int>();
    }

    public void AddKeyToDictionaries(int dataLoadRunID, Validator validator, QueryBuilder queryBuilder, Evaluation mostRecentEvaluation = null)
    {
        //ensure keys exit (if it is a novel data load run ID then we will add it to the dictionaries

        //column level
        //ensure validation failures contain it
        if (!ColumnValidationFailuresByDataLoadRunID.ContainsKey(dataLoadRunID))
            ColumnValidationFailuresByDataLoadRunID.Add(dataLoadRunID,
                new VerboseValidationResults(validator.ItemValidators.ToArray()));

        //ensure unconstrained columns have it
        if (!AllColumnStates.ContainsKey(dataLoadRunID))
        {
            var allColumns = new List<ColumnState>();

            foreach (var col in queryBuilder.SelectColumns.Select(s => s.IColumn))
            {
                var runtimeName = col.GetRuntimeName();
                var validationXML = "";

                var itemValidator =
                    validator.ItemValidators.SingleOrDefault(iv => iv.TargetProperty.Equals(runtimeName));

                //if it is a constrained column it is likely to have child ColumnConstraints results but whatever - the important thing is we should document the state of the ItemValidator for this col
                if (itemValidator != null)
                    validationXML = itemValidator.SaveToXml();
                //else it is an unconstrained column, ah well still interesting

                //add the state regardless
                var columnState = new ColumnState(runtimeName, dataLoadRunID, validationXML);
                if (mostRecentEvaluation is not null)
                {
                    var existingColumn = mostRecentEvaluation.ColumnStates.Where(c => c.TargetProperty.Equals(runtimeName) && c.PivotCategory.Equals(_pivotCategory)).FirstOrDefault();
                    if (dataLoadRunID > 0 && existingColumn is not null)
                    {
                        //use the last state as the starting value
                        columnState.CountMissing = existingColumn.CountMissing;
                        columnState.CountCorrect = existingColumn.CountCorrect;
                        columnState.CountDBNull = existingColumn.CountDBNull;
                        columnState.CountInvalidatesRow = existingColumn.CountInvalidatesRow;
                        columnState.CountWrong = existingColumn.CountWrong;
                    }
                }
                allColumns.Add(columnState);
            }

            //and add it to our dictionary under the load batch
            AllColumnStates.Add(dataLoadRunID, allColumns.ToArray());
        }

        //row level
        //ensure key exists in failing rows
        if (!WorstConsequencesByDataLoadRunID.ContainsKey(dataLoadRunID))
        {
            //add the data load run id key
            WorstConsequencesByDataLoadRunID.Add(dataLoadRunID, new Dictionary<Consequence, int>());

            //add each possible consequence as a key too
            WorstConsequencesByDataLoadRunID[dataLoadRunID].Add(Consequence.Wrong, 0);
            WorstConsequencesByDataLoadRunID[dataLoadRunID].Add(Consequence.Missing, 0);
            WorstConsequencesByDataLoadRunID[dataLoadRunID].Add(Consequence.InvalidatesRow, 0);
        }

        //ensure key exists in passing rows
        RowsPassingValidationByDataLoadRunID.TryAdd(dataLoadRunID, 0);
    }

    private bool _correctValuesCalculated;

    /// <summary>
    /// Calculates the final counts for each Column based on the validation failures documented to date.  You can only call this method once and it
    /// must be called before committing to database.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void CalculateFinalValues()
    {
        if (_correctValuesCalculated)
            throw new Exception("Correct values have already been calculated");

        _correctValuesCalculated = true;

        //adjust the counts for each data load run id \ column according to the dictionary of validation failures
        //per run id
        foreach (var dataLoadRunID in AllColumnStates.Keys)
            //per column
            foreach (var column in AllColumnStates[dataLoadRunID])
                //if it is a constrained column
                if (ColumnValidationFailuresByDataLoadRunID[dataLoadRunID]
                    .DictionaryOfFailure.TryGetValue(column.TargetProperty, out var consequence))
                {
                    //adjust our correct value downwards according to the results of the dictionary of failure
                    column.CountMissing = consequence[Consequence.Missing];
                    column.CountWrong = consequence[Consequence.Wrong];
                    column.CountInvalidatesRow = consequence[Consequence.InvalidatesRow];

                    column.CountCorrect -= consequence[Consequence.Missing];
                    column.CountCorrect -= consequence[Consequence.Wrong];
                    column.CountCorrect -= consequence[Consequence.InvalidatesRow];
                }
    }

    public void CommitToDatabase(Evaluation evaluation, ICatalogue catalogue, DbConnection con,
        DbTransaction transaction)
    {
        if (!_correctValuesCalculated)
            throw new Exception("You must call CalculateFinalValues before committing to the database");

        IEnumerable<int> novelDataLoadRunIDs = RowsPassingValidationByDataLoadRunID.Keys;

        //now for every load batch we encountered in our evaluations
        foreach (var dataLoadRunID in novelDataLoadRunIDs)
        {
            //record the row states calculation (how many total rows are good/bad/ugly etc)
            evaluation.AddRowState(dataLoadRunID,
                RowsPassingValidationByDataLoadRunID[dataLoadRunID],
                WorstConsequencesByDataLoadRunID[dataLoadRunID][Consequence.Missing],
                WorstConsequencesByDataLoadRunID[dataLoadRunID][Consequence.Wrong],
                WorstConsequencesByDataLoadRunID[dataLoadRunID][Consequence.InvalidatesRow],
                catalogue.ValidatorXML,
                _pivotCategory,
                con,
                transaction
            );

            //record the column states calculations (how many total values in column x are good/bad/ugly etc)
            foreach (var columnState in AllColumnStates[dataLoadRunID])
                columnState.Commit(evaluation, _pivotCategory, con, transaction);
        }
    }
}