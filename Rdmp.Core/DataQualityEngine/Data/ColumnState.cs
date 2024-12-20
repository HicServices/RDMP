// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.DataQualityEngine.Data;

/// <summary>
/// Runtime class for DQE used to record the number of records passing/failing validation/null for a given column in a dataset.  These counts are incremented
/// during the DQE evaluation process then finally saved into the ColumnState table in DQE database.
/// </summary>
public class ColumnState
{
    private int _countCorrect;
    private int _countDbNull;
    private int _countMissing;
    private int _countWrong;
    private int _countInvalidatesRow;
    public string TargetProperty { get; set; }
    public int DataLoadRunID { get; set; }

    public int? ID { get; private set; }
    public int? Evaluation_ID { get; private set; }

    public string ItemValidatorXML { get; set; }

    public int CountMissing
    {
        get => _countMissing;
        set
        {
            if (IsCommitted)
                throw new NotSupportedException(
                    "Can only edit these values while the ColumnState is being computed in memory, this ColumnState came from the database and was committed long ago");
            _countMissing = value;
        }
    }

    public int CountWrong
    {
        get => _countWrong;
        set
        {
            if (IsCommitted)
                throw new NotSupportedException(
                    "Can only edit these values while the ColumnState is being computed in memory, this ColumnState came from the database and was committed long ago");
            _countWrong = value;
        }
    }

    public int CountInvalidatesRow
    {
        get => _countInvalidatesRow;
        set
        {
            if (IsCommitted)
                throw new NotSupportedException(
                    "Can only edit these values while the ColumnState is being computed in memory, this ColumnState came from the database and was committed long ago");
            _countInvalidatesRow = value;
        }
    }

    public int CountCorrect
    {
        get => _countCorrect;
        set
        {
            if (IsCommitted)
                throw new NotSupportedException(
                    "Can only edit these values while the ColumnState is being computed in memory, this ColumnState came from the database and was committed long ago");

            _countCorrect = value;
        }
    }

    public int CountDBNull
    {
        get => _countDbNull;
        set
        {
            if (IsCommitted)
                throw new NotSupportedException(
                    "Can only edit these values while the ColumnState is being computed in memory, this ColumnState came from the database and was committed long ago");

            _countDbNull = value;
        }
    }

    public string PivotCategory { get; private set; }

    public bool IsCommitted { get; private set; }

    public ColumnState(string targetProperty, int dataLoadRunID, string itemValidatorXML)
    {
        TargetProperty = targetProperty;
        DataLoadRunID = dataLoadRunID;
        ItemValidatorXML = itemValidatorXML;

        IsCommitted = false;
    }

    public ColumnState(DbDataReader r)
    {
        TargetProperty = r["TargetProperty"].ToString();
        DataLoadRunID = Convert.ToInt32(r["DataLoadRunID"]);
        Evaluation_ID = Convert.ToInt32(r["Evaluation_ID"]);
        ID = Convert.ToInt32(r["ID"]);
        CountCorrect = Convert.ToInt32(r["CountCorrect"]);
        CountDBNull = Convert.ToInt32(r["CountDBNull"]);
        ItemValidatorXML = r["ItemValidatorXML"].ToString();

        CountMissing = Convert.ToInt32(r["CountMissing"]);
        CountWrong = Convert.ToInt32(r["CountWrong"]);
        CountInvalidatesRow = Convert.ToInt32(r["CountInvalidatesRow"]);

        PivotCategory = (string)r["PivotCategory"];

        IsCommitted = true;
    }

    /// <summary>
    /// Constructor for mocks and tests
    /// </summary>
    protected ColumnState()
    {
    }

    public void Commit(Evaluation evaluation, string pivotCategory, DbConnection con, DbTransaction transaction)
    {
        if (IsCommitted)
            return;
            //throw new NotSupportedException("ColumnState was already committed");

        var sql =
            $"INSERT INTO ColumnState(TargetProperty,DataLoadRunID,Evaluation_ID,CountCorrect,CountDBNull,ItemValidatorXML,CountMissing,CountWrong,CountInvalidatesRow,PivotCategory)VALUES({"@TargetProperty"},{DataLoadRunID},{evaluation.ID},{CountCorrect},{CountDBNull},@ItemValidatorXML,{CountMissing},{CountWrong},{CountInvalidatesRow},@PivotCategory)";

        using (var cmd = DatabaseCommandHelper.GetCommand(sql, con, transaction))
        {
            DatabaseCommandHelper.AddParameterWithValueToCommand("@ItemValidatorXML", cmd, ItemValidatorXML);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@TargetProperty", cmd, TargetProperty);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@PivotCategory", cmd, pivotCategory);
            cmd.ExecuteNonQuery();
        }
            
        IsCommitted = true;
    }
}