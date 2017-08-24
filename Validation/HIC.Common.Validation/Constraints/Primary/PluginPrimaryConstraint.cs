using System.ComponentModel.Composition;

namespace HIC.Common.Validation.Constraints.Primary
{
    [InheritedExport(typeof(PrimaryConstraint))]
    public abstract class PluginPrimaryConstraint : PrimaryConstraint
    {
    }
}
