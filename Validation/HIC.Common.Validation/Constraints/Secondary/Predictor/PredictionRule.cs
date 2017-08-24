using System;

namespace HIC.Common.Validation.Constraints.Secondary.Predictor
{
    public abstract class PredictionRule
    {
        public abstract ValidationFailure Predict(IConstraint parent,object value, object targetValue);
    }
}
