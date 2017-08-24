using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using DataQualityEngine.Data;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;

namespace DataQualityEngine.Reports
{
    public class DQEStateTesseract
    {
        private readonly string _pivotCategory;

        //column level results - validation failures
        public Dictionary<int, VerboseValidationResults> ColumnValidationFailuresByDataLoadRunID { get; set; }

        //column level results - everything including unconstrained columns and columns which never had any validation failures at all
        public Dictionary<int, ColumnState[]> AllColumnStates { get; set; }

        public Dictionary<int, Dictionary<Consequence, int>> WorstConsequencesByDataLoadRunID { get; set; }
        public Dictionary<int, int> RowsPassingValidationByDataLoadRunID { get; set; }

        public DQEStateTesseract(string pivotCategory)
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

        public void AddKeyToDictionaries(int dataLoadRunID, Validator validator, QueryBuilder queryBuilder)
        {
            //ensure keys exit (if it is a novel data load run ID then we will add it to the dictionaries

            //column level
            //ensure validation failures contain it
            if (!ColumnValidationFailuresByDataLoadRunID.ContainsKey(dataLoadRunID))
                ColumnValidationFailuresByDataLoadRunID.Add(dataLoadRunID, new VerboseValidationResults(validator.ItemValidators.ToArray()));

            //ensure unconstrained columns have it
            if (!AllColumnStates.ContainsKey(dataLoadRunID))
            {
                List<ColumnState> allColumns = new List<ColumnState>();

                foreach (IColumn col in queryBuilder.SelectColumns.Select(s => s.IColumn))
                {

                    string runtimeName = col.GetRuntimeName();
                    string validationXML = "";

                    var itemValidator = validator.ItemValidators.SingleOrDefault(iv => iv.TargetProperty.Equals(runtimeName));

                    //if it is a constrained column it is likely to have child ColumnConstraints results but whatever - the important thing is we should document the state of the ItemValidator for this col
                    if (itemValidator != null)
                        validationXML = itemValidator.SaveToXml();
                    //else it is an unconstrained column, ah well still interesting

                    //add the state regardless
                    allColumns.Add(new ColumnState(runtimeName, dataLoadRunID, validationXML));
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
            if (!RowsPassingValidationByDataLoadRunID.ContainsKey(dataLoadRunID))
                RowsPassingValidationByDataLoadRunID.Add(dataLoadRunID, 0);
        }

        private bool _dandyValuesAdjusted = false;
        public void AdjustDandyValuesDown()
        {
            if(_dandyValuesAdjusted)
                throw new Exception("Dandy values have already been adjusted down");

            _dandyValuesAdjusted = true;

            //adjust the dandy state downwards according to the dictionary of validation failures
            //per run id
            foreach (var dataLoadRunID in AllColumnStates.Keys)
            {
                //per column
                foreach (var column in AllColumnStates[dataLoadRunID])
                {
                    //if it is a constrained column
                    if (ColumnValidationFailuresByDataLoadRunID[dataLoadRunID]
                        .DictionaryOfFailure.ContainsKey( //with entries in the dictionary of failure
                            column.TargetProperty))
                    {
                        //adjust our dandy value downwards according to the results of the dictionary of failure
                        var kvp = ColumnValidationFailuresByDataLoadRunID[dataLoadRunID].DictionaryOfFailure[column.TargetProperty];

                        column.CountMissing = kvp[Consequence.Missing];
                        column.CountWrong = kvp[Consequence.Wrong];
                        column.CountInvalidatesRow = kvp[Consequence.InvalidatesRow];

                        column.CountCorrect -= kvp[Consequence.Missing];
                        column.CountCorrect -= kvp[Consequence.Wrong];
                        column.CountCorrect -= kvp[Consequence.InvalidatesRow];
                    }
                }
            }
        }

        public void CommitToDatabase(Evaluation evaluation, Catalogue catalogue, DbConnection con, DbTransaction transaction)
        {

            if(!_dandyValuesAdjusted)
                throw new Exception("You must call AdjustDandyValuesDown before committing to the database");

            IEnumerable<int> novelDataLoadRunIDs = RowsPassingValidationByDataLoadRunID.Keys;

            //now for every load batch we encountered in our evaluations
            foreach (int dataLoadRunID in novelDataLoadRunIDs)
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
                    columnState.Commit(evaluation,_pivotCategory, con, transaction);
            }

        }
    }
}