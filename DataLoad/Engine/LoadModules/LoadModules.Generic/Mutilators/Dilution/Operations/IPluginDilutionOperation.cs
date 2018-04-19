using System.ComponentModel.Composition;
using ANOStore.ANOEngineering;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Mutilators.Dilution.Operations
{
    /// <summary>
    /// MEF discoverable version of IDilutionOperation
    /// </summary>
    [InheritedExport(typeof(IDilutionOperation))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDilutionOperation:IDilutionOperation
    {
    }
}
