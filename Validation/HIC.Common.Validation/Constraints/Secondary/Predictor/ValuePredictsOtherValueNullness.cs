namespace HIC.Common.Validation.Constraints.Secondary.Predictor
{
    /// <summary>
    /// Validation rule for use with a Prediction Constraint.  Indicates that the 'nullness' of the columns must match (i.e. if one is null the other must be too)
    /// </summary>
    public class ValuePredictsOtherValueNullness : PredictionRule
    {
        public override ValidationFailure Predict(IConstraint parent, object value, object targetValue)
        {
            if((value == null) != (targetValue == null))
                return new ValidationFailure("Nullness did not match, when one value is null, the other must be null.  When one value has a value the other must also have a value.  Nullness of ConstrainedColumn:" +(value == null) + ". Nullness of TargetColumn:" + (targetValue == null),parent);

            return null;
        }
    }
}