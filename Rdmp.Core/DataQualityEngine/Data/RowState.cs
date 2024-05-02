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
///     Class for inserting/retrieving records into the RowState table of the DQE database.  This table stores counts of
///     the total number of rows (divided by
///     PivotCategory - if any) passing, failing validation during a DQE run on a dataset.
/// </summary>
public class RowState
{
    public int Correct { get; private set; }
    public int Missing { get; private set; }
    public int Wrong { get; private set; }
    public int Invalid { get; private set; }
    public int DataLoadRunID { get; private set; }
    public string ValidatorXML { get; private set; }
    public string PivotCategory { get; private set; }


    public RowState(DbDataReader r)
    {
        Correct = Convert.ToInt32(r["Correct"]);
        Missing = Convert.ToInt32(r["Missing"]);
        Wrong = Convert.ToInt32(r["Wrong"]);
        Invalid = Convert.ToInt32(r["Invalid"]);
        PivotCategory = (string)r["PivotCategory"];
        DataLoadRunID = Convert.ToInt32(r["DataLoadRunID"]);
        ValidatorXML = r["ValidatorXML"].ToString();
    }


    public RowState(Evaluation evaluation, int dataLoadRunID, int correct, int missing, int wrong, int invalid,
        string validatorXml, string pivotCategory, DbConnection con, DbTransaction transaction)
    {
        var sql =
            $"INSERT INTO RowState(Evaluation_ID,Correct,Missing,Wrong,Invalid,DataLoadRunID,ValidatorXML,PivotCategory)VALUES({evaluation.ID},{correct},{missing},{wrong},{invalid},{dataLoadRunID},@validatorXML,@pivotCategory)";

        using (var cmd = DatabaseCommandHelper.GetCommand(sql, con, transaction))
        {
            DatabaseCommandHelper.AddParameterWithValueToCommand("@validatorXML", cmd, validatorXml);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@pivotCategory", cmd, pivotCategory);
            cmd.ExecuteNonQuery();
        }

        Correct = correct;
        Missing = missing;
        Wrong = wrong;
        Invalid = invalid;
        ValidatorXML = validatorXml;
        DataLoadRunID = dataLoadRunID;
    }
}