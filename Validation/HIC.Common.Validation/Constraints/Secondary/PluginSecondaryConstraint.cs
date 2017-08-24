using System.ComponentModel.Composition;

namespace HIC.Common.Validation.Constraints.Secondary
{
    [InheritedExport(typeof(SecondaryConstraint))]
    public abstract class PluginSecondaryConstraint : SecondaryConstraint
    {
    }
}
