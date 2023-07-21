// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary.Predictor;

/// <summary>
///     Validation rule in which two columns are dependent on one another for validation according to a given
///     PredictionRule.  For example the CHI number contains
///     a gender digit, validation passes if the digit matches the selected gender column (See ChiSexPredictor).
/// </summary>
public class Prediction : SecondaryConstraint
{
    //blank constructor required for XMLSerialization
    public Prediction()
    {
    }

    public Prediction(PredictionRule rule, string targetColumn)
    {
        if (rule == null)
            throw new ArgumentException("You must specify a PredictionRule to follow", nameof(rule));

        Rule = rule;
        TargetColumn = targetColumn;
    }

    [Description("The current value enforces a prediction about the value of this other field")]
    [ExpectsColumnNameAsInput]
    public string TargetColumn { get; set; }

    [Description(
        "The prediction rule that takes as input the current value and uses it to check the target column matches expectations")]
    public PredictionRule Rule { get; set; }

    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (Rule == null)
            throw new InvalidOperationException("PredictionRule has not been set yet");

        if (TargetColumn == null)
            throw new InvalidOperationException("TargetColumn has not been set yet");

        if (OtherFieldInfoIsNotProvided(otherColumns, otherColumnNames))
            throw new ArgumentException("Could not make prediction because no other fields were passed");

        if (otherColumns.Length != otherColumnNames.Length)
            throw new Exception(
                "Could not make prediction because of mismatch between column values and column names array sizes");

        var i = Array.IndexOf(otherColumnNames, TargetColumn);

        if (i == -1)
            throw new MissingFieldException(
                $"Could not find TargetColumn '{TargetColumn}' for Prediction validation constraint.  Supplied column name collection was:({string.Join(",", otherColumnNames)})");

        return Rule.Predict(this, value, otherColumns[i]);
    }


    private static bool OtherFieldInfoIsNotProvided(object[] otherColumns, string[] otherColumnNames)
    {
        return otherColumns == null || otherColumns.Length < 1 || otherColumnNames == null ||
               otherColumnNames.Length < 1;
    }


    public override void RenameColumn(string originalName, string newName)
    {
        if (TargetColumn.Equals(originalName))
            TargetColumn = newName;
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        if (Rule == null)
            return "Normally checks input against prediction rule but no rule has yet been configured";

        return $"Checks that input follows its prediciton rule: '{Rule.GetType().Name}'";
    }
}