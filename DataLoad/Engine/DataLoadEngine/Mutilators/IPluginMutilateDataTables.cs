using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Mutilators
{
    /// <summary>
    /// MEF discoverable version of IMutilateDataTables
    /// </summary>
    [InheritedExport(typeof(IMutilateDataTables))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginMutilateDataTables : IMutilateDataTables
    {
    }
}