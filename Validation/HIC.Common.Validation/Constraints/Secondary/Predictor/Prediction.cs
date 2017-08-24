using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using CatalogueLibrary.Data.DataLoad;
using HIC.Common.Validation.UIAttributes;

namespace HIC.Common.Validation.Constraints.Secondary.Predictor
{
    public class Prediction : SecondaryConstraint
    {
        [Description("The current value enforces a prediction about the value of this other field")]
        [ExpectsColumnNameAsInput]
        public string TargetColumn { get; set; }
        
        [Description("The prediction rule that takes as input the current value and uses it to check the target column matches expectations")]
        public PredictionRule Rule { get; set; }

        //blank constructor required for XMLSerialization
        public Prediction()
        {
            
        }

        public Prediction(PredictionRule rule, string targetColumn)
        {
            if(rule == null)
                throw new ArgumentException("You must specify a PredictionRule to follow","rule");

            Rule = rule;
            TargetColumn = targetColumn;
        }

        public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
        {
            if (value == null)
                return null;

            if(Rule == null )
                throw new InvalidOperationException("PredictionRule has not been set yet");

            if(TargetColumn == null)
                throw new InvalidOperationException("TargetColumn has not been set yet");

            if(OtherFieldInfoIsNotProvided(otherColumns, otherColumnNames))
                throw new ArgumentException("Could not make prediction because no other fields were passed");

            if (otherColumns.Length != otherColumnNames.Length)
                throw new Exception("Could not make prediction because of mismatch between column values and column names array sizes");

            int i = Array.IndexOf(otherColumnNames,TargetColumn);

            if (i == -1)
                throw new MissingFieldException("Could not find TargetColumn '" + TargetColumn +"' for Prediction validation constraint.  Supplied column name collection was:(" +string.Join(",", otherColumnNames) + ")");

            return Rule.Predict(this,value,otherColumns[i]);
        }


        private static bool OtherFieldInfoIsNotProvided(object[] otherColumns, string[] otherColumnNames)
        {
            return otherColumns == null || otherColumns.Length < 1 || otherColumnNames == null || otherColumnNames.Length < 1;
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

            return "Checks that input follows its prediciton rule: '" + Rule.GetType().Name + "'";
        }
    }
}
