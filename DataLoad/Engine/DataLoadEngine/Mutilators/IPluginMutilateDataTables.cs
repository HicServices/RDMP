using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Mutilators
{
    [InheritedExport(typeof(IMutilateDataTables))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginMutilateDataTables : IMutilateDataTables
    {
    }
}