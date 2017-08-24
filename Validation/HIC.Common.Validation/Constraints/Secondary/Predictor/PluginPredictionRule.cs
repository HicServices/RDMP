using System.ComponentModel.Composition;

namespace HIC.Common.Validation.Constraints.Secondary.Predictor
{
    [InheritedExport(typeof(PredictionRule))]
    public abstract class PluginPredictionRule:PredictionRule
    {
    }
}